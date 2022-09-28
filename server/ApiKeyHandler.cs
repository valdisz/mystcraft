namespace advisor {
    using System;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using advisor.Persistence;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Net.Http.Headers;

    public interface IApiKeyStore {
        bool ValidateApiKey(string apiKey);

        string GetUsername(string apiKey);
    }

    public class ConfigurationApiKeyStore : IApiKeyStore {
        public ConfigurationApiKeyStore(IOptions<Options> options) {
            this.apiKeys = options.Value.Keys;
        }

        private readonly List<string> apiKeys;

        public bool ValidateApiKey(string apiKey) => apiKeys.Contains(apiKey);

        public string GetUsername(string apiKey) => apiKey.Split(":")[1];

        public class Options {
            public List<string> Keys { get; set; }
        }
    }

    public static class ApiKeyDefaults {
        public const string AuthenticationScheme = "api-key";
    }

    public class ApiKeyHandlerOtions : AuthenticationSchemeOptions {
        public string ApiKeyHeaderName { get; set; } = "X-API-KEY";
    }

    public class ApiKeyHandler : AuthenticationHandler<ApiKeyHandlerOtions>
    {
        public ApiKeyHandler(
            IOptionsMonitor<ApiKeyHandlerOtions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            Database db,
            IApiKeyStore apiKeys)
            : base(options, logger, encoder, clock)
        {
            this.db = db;
            this.apiKeys = apiKeys;
        }

        private readonly Database db;
        private readonly IApiKeyStore apiKeys;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            string xApiKey = null;

            var apiKeyHeader = Request.Headers[Options.ApiKeyHeaderName];
            var authHeader = Request.Headers.Authorization;
            var apiQuery = Request.Query[Options.ApiKeyHeaderName];

            if (apiKeyHeader.Count > 0) {
                xApiKey = apiKeyHeader;
            }
            else if (authHeader.Count > 0 && AuthenticationHeaderValue.TryParse(authHeader, out var value)) {
                xApiKey = value.Parameter;
            }
            else if (apiQuery.Count > 0) {
                xApiKey = apiQuery;
            }

            if (xApiKey == null) {
                 return AuthenticateResult.NoResult();
            }

            if (!apiKeys.ValidateApiKey(xApiKey)) {
                return AuthenticateResult.Fail("API key is invalid");
            }

            var email = apiKeys.GetUsername(xApiKey);
            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) {
                return AuthenticateResult.Fail("User not found");
            }

            var scheme = Scheme.Name;
            var roles = user.Roles.Select(role => new Claim(WellKnownClaimTypes.Role, role));
            var identity = new ClaimsIdentity(new[] {
                new Claim(WellKnownClaimTypes.UserId, user.Id.ToString()),
                new Claim(WellKnownClaimTypes.Email, user.Email),
            }.Concat(roles), scheme, null, WellKnownClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, scheme);

            return AuthenticateResult.Success(ticket);
        }
    }

    public static class ApiKeyExtensions {
        public static IServiceCollection ConfigureApiKeys(
            this IServiceCollection services,
            IConfiguration configuration,
            string containerSectionName = "ApiKeys") {

            services.Configure<ConfigurationApiKeyStore.Options>(configuration.GetSection(containerSectionName));
            return services;
        }

        public static AuthenticationBuilder AddApiKeys(this AuthenticationBuilder builder)
        {
            return builder.AddApiKeys(opt => { });
        }
        public static AuthenticationBuilder AddApiKeys(this AuthenticationBuilder builder, Action<ApiKeyHandlerOtions> configureOptions)
        {
            return builder.AddApiKeys(ApiKeyDefaults.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddApiKeys(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyHandlerOtions> configureOptions)
        {
            return builder
                .AddScheme<ApiKeyHandlerOtions, ApiKeyHandler>(authenticationScheme, configureOptions);
        }
    }
}
