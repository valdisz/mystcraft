namespace advisor {
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using advisor.Persistence;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using System.Linq;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using MediatR;
    using advisor.Features;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using Microsoft.Extensions.Options;
    using HotChocolate.Types.Relay;
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;

    public record UserPlayers(long UserId, long[] PlayerIds, long Timestamp);

    [AllowAnonymous]
    [Route("account")]
    public class AccountController : ControllerBase {
        public AccountController(Database db, IAccessControl accessControl, IMediator mediator, IHttpClientFactory httpFactory,
            IOptionsSnapshot<DiscordOptions> discordOptions, IIdSerializer idSerializer, IMemoryCache cache
        ) {
            this.db = db;
            this.accessControl = accessControl;
            this.mediator = mediator;
            this.httpFactory = httpFactory;
            this.idSerializer = idSerializer;
            this.cache = cache;
            this.discordOptions = discordOptions.Value;
        }

        private readonly Database db;
        private readonly IAccessControl accessControl;
        private readonly IMediator mediator;
        private readonly IHttpClientFactory httpFactory;
        private readonly IIdSerializer idSerializer;
        private readonly IMemoryCache cache;
        private readonly DiscordOptions discordOptions;


        private static string GetUserPlayersCacheKey(long userId) => $"user-{userId}-players";
        private static string GetUserPlayersCacheKey(string userId) => $"user-{userId}-players";

        private static Task<DbUser> GetUserByIdAsync(Database db, long userId)
            => db.Users.Include(x => x.Players).SingleOrDefaultAsync(x => x.Id == userId);

        private static Task<DbUser> GetUserByEamilAsync(Database db, string email)
            => db.Users.Include(x => x.Players).SingleOrDefaultAsync(x => x.Email == email);

        private static ClaimsIdentity MapIdentity(IMemoryCache cache, string schema, DbUser user) {
            var claims = new List<Claim> {
                new Claim(WellKnownClaimTypes.UserId, user.Id.ToString()),
                new Claim(WellKnownClaimTypes.Email, user.Email)
            };

            var cacheKey = GetUserPlayersCacheKey(user.Id);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var playerIds = user.Players.Select(x => x.Id).ToArray();

            cache.Set(cacheKey, new UserPlayers(user.Id, playerIds, timestamp));

            claims.AddRange(user.Roles.Select(role => new Claim(WellKnownClaimTypes.Role, role)));
            claims.AddRange(playerIds.Select(playerId => new Claim(WellKnownClaimTypes.Player, playerId.ToString())));
            claims.Add(new Claim(WellKnownClaimTypes.Timestamp, timestamp.ToString()));

            var identity = new ClaimsIdentity(claims, schema, null, WellKnownClaimTypes.Role);
            return identity;
        }

        private async Task SignInIdentity(ClaimsIdentity identity) {
            await HttpContext.SignOutAsync(identity.AuthenticationType);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(identity.AuthenticationType, principal);
        }

        public static async Task ValidatePrinciaplAsync(CookieValidatePrincipalContext context) {
            var principal = context.Principal;

            var userId = principal.FindFirstValue(WellKnownClaimTypes.UserId);
            if (userId == null) {
                context.RejectPrincipal();
                return;
            }

            var timestamp = principal.FindFirstValue(WellKnownClaimTypes.Timestamp);
            if (timestamp == null) {
                context.RejectPrincipal();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var cache = services.GetService<IMemoryCache>();

            var cacheKey = GetUserPlayersCacheKey(userId);
            var players = cache.Get<UserPlayers>(cacheKey);
            if (players != null && players.Timestamp == long.Parse(timestamp)) {
                return;
            }

            var db = services.GetRequiredService<Database>();
            var user = await GetUserByIdAsync(db, long.Parse(userId));
            if (user == null) {
                context.RejectPrincipal();
                return;
            }

            var identity = MapIdentity(cache, CookieAuthenticationDefaults.AuthenticationScheme, user);
            var newPrincipal = new ClaimsPrincipal(identity);

            context.ReplacePrincipal(newPrincipal);
            context.ShouldRenew = true;
        }

        [HttpGet("login-as/{userId}")]
        [Authorize(Policy = Policies.Root)]
        public async Task<IActionResult> LoginAsAsync([FromRoute, Required] string userId) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var id = idSerializer.Deserialize(userId);
            var idValue = (long) id.Value;

            var user = await GetUserByIdAsync(db, idValue);
            if (user == null) {
                return NotFound();
            }

            var identity = MapIdentity(cache, CookieAuthenticationDefaults.AuthenticationScheme, user);
            await SignInIdentity(identity);

            return Redirect("/");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromForm] LoginModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var user = await GetUserByEamilAsync(db, model.Email);
            if (user == null) {
                return Unauthorized();
            }

            if (!accessControl.VerifyPassword(user.Salt, model.Password, user.Digest)) {
                return Unauthorized();
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            var identity = MapIdentity(cache, CookieAuthenticationDefaults.AuthenticationScheme, user);
            await SignInIdentity(identity);

            return NoContent();
        }

        [HttpGet("login/{provider}")]
        public IActionResult LoginExternal([FromRoute, Required] string provider) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var auth = new AuthenticationProperties {
                RedirectUri = Url.Action("ExternalCallback", "Account", new { provider }, Request.Scheme)// $"/accounts/external/{provider}"
            };

            return Challenge(auth, DiscordDefaults.AuthenticationScheme);
        }

        [HttpGet("external/{provider}", Name = "ExternalCallback")]
        public async Task<IActionResult> ExternalCallbackAsync([FromRoute, Required] string provider) {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(ExternalAuthentication.AuthenticationScheme);

            if (result?.Succeeded != true) {
                return this.Unauthorized();
            }

            var accessToken = await HttpContext.GetTokenAsync(result.Ticket.AuthenticationScheme, "access_token");

            using var http = httpFactory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new ("Bearer", accessToken);

            var response = await http.GetStringAsync($"{discordOptions.Address}/v{discordOptions.ApiVersion}/users/@me");
            var json = JObject.Parse(response);

            var email = json.Value<string>("email");

            var user = await GetUserByEamilAsync(db, email);
            if (user == null) {
                user = await mediator.Send(new UserCreate(email, null));
            }
            else {
                user.LastLoginAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }

            var identity = MapIdentity(cache, CookieAuthenticationDefaults.AuthenticationScheme, user);
            await SignInIdentity(identity);

            return Redirect("/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync() {
            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(User.Identity.AuthenticationType);
            }

            return Redirect("/");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm] LoginModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(User.Identity.AuthenticationType);
            }

            var user = await mediator.Send(new UserCreate(model.Email, model.Password));
            if (user == null) {
                return BadRequest(new {
                    General = "User with such email already exists."
                });
            }

            return NoContent();
        }
    }

    public class LoginModel {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Must look like valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(64, ErrorMessage = "Password must be from 6 till 64 characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
