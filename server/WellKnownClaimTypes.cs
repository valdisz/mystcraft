namespace atlantis
{
    using System.Security.Claims;

    public static class WellKnownClaimTypes {
        public const string UserId = ClaimTypes.Sid;
        public const string Email = ClaimTypes.Email;
    }
}
