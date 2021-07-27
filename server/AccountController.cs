namespace advisor {
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using advisor.Persistence;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using MediatR;
    using advisor.Features;

    [AllowAnonymous]
    [Route("account")]
    public class AccountController : ControllerBase {
        public AccountController(Database db, AccessControl accessControl, IMediator mediator) {
            this.db = db;
            this.accessControl = accessControl;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly AccessControl accessControl;
        private readonly IMediator mediator;

        [HttpPost("login-as")]
        [Authorize(Policy = Policies.Root)]
        public async Task<IActionResult> LoginAsync([FromForm, Required, EmailAddress] string email) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) {
                return NotFound();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var roles = user.Roles.Select(x => new Claim(WellKnownClaimTypes.Role, x.Role));

            var identity = new ClaimsIdentity(new[] {
                new Claim(WellKnownClaimTypes.UserId, user.Id.ToString()),
                new Claim(WellKnownClaimTypes.Email, user.Email),
            }.Concat(roles), CookieAuthenticationDefaults.AuthenticationScheme, null, WellKnownClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromForm] LoginModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
            if (user == null) {
                return Unauthorized();
            }

            if (!accessControl.VerifyPassword(user.Salt, model.Password, user.Digest)) {
                return Unauthorized();
            }

            var roles = user.Roles.Select(x => new Claim(WellKnownClaimTypes.Role, x.Role));

            var identity = new ClaimsIdentity(new[] {
                new Claim(WellKnownClaimTypes.UserId, user.Id.ToString()),
                new Claim(WellKnownClaimTypes.Email, user.Email),
            }.Concat(roles), CookieAuthenticationDefaults.AuthenticationScheme, null, WellKnownClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return NoContent();
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync() {
            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Redirect("/");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm] LoginModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if (User.Identity.IsAuthenticated) {
                await HttpContext.SignOutAsync();
            }

            var user = await mediator.Send(new CreateUser(model.Email, model.Password));
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
