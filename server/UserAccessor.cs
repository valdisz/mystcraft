using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace advisor;

public interface IUserAccessor {
    ClaimsPrincipal Principal { get; }
}

public class UserAccessor : IUserAccessor {
    public UserAccessor(IHttpContextAccessor httpContextAccessor) {
        Principal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    }

    public ClaimsPrincipal Principal { get; }
}
