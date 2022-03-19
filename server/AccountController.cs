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

    [AllowAnonymous]
    [Route("account")]
    public class AccountController : ControllerBase {
        public AccountController(Database db, AccessControl accessControl, IMediator mediator, IHttpClientFactory httpFactory,
            IOptionsSnapshot<DiscordOptions> discordOptions, IIdSerializer idSerializer
        ) {
            this.db = db;
            this.accessControl = accessControl;
            this.mediator = mediator;
            this.httpFactory = httpFactory;
            this.idSerializer = idSerializer;
            this.discordOptions = discordOptions.Value;
        }

        private readonly Database db;
        private readonly AccessControl accessControl;
        private readonly IMediator mediator;
        private readonly IHttpClientFactory httpFactory;
        private readonly IIdSerializer idSerializer;
        private readonly DiscordOptions discordOptions;

        private ClaimsIdentity MapIdentity(string schema, DbUser user) {
            var claims = new List<Claim> {
                new Claim(WellKnownClaimTypes.UserId, user.Id.ToString()),
                new Claim(WellKnownClaimTypes.Email, user.Email)
            };

            claims.AddRange(user.Roles.Select(role => new Claim(WellKnownClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, schema, null, WellKnownClaimTypes.Role);
            return identity;
        }

        private async Task SignInIdentity(ClaimsIdentity identity) {
            await HttpContext.SignOutAsync(identity.AuthenticationType);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(identity.AuthenticationType, principal);
        }

        [HttpGet("login-as/{userId}")]
        [Authorize(Policy = Policies.Root)]
        public async Task<IActionResult> LoginAsAsync([FromRoute, Required] string userId) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var id = idSerializer.Deserialize(userId);
            var idValue = (long) id.Value;

            var user = await db.Users.SingleOrDefaultAsync(x => x.Id == idValue);
            if (user == null) {
                return NotFound();
            }

            var identity = MapIdentity(CookieAuthenticationDefaults.AuthenticationScheme, user);
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

            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == model.Email && x.Digest != null);
            if (user == null) {
                return Unauthorized();
            }

            if (!accessControl.VerifyPassword(user.Salt, model.Password, user.Digest)) {
                return Unauthorized();
            }

            var identity = MapIdentity(CookieAuthenticationDefaults.AuthenticationScheme, user);
            await SignInIdentity(identity);

            return NoContent();
        }

        [HttpGet("login/{provider}")]
        public IActionResult LoginExternal([FromRoute, Required] string provider) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var auth = new AuthenticationProperties {
                RedirectUri = Url.Action("ExternalCallback", "Account", new { provider })// $"/accounts/external/{provider}"
            };

            return Challenge(auth, DiscordDefaults.AuthenticationScheme);
        }

        [HttpGet("external/{provider}")]
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

            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) {
                user = await mediator.Send(new UserCreate(email, null));
            }

            var identity = MapIdentity(CookieAuthenticationDefaults.AuthenticationScheme, user);
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
