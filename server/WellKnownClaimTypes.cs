namespace advisor;

public static class WellKnownClaimTypes {
    public const string PREFIX    =  "advisor";
    public const string USER_ID   = $"{PREFIX}.id";
    public const string EMAIL     = $"{PREFIX}.email";
    public const string ROLE      = $"{PREFIX}.role";
    public const string PLAYER    = $"{PREFIX}.player";

    /// <summary>
    /// The time stamp claim stores information about the time when user player list snapshot was taken.
    /// So that we can invalidate the cache when player list changes for given user.
    /// </summary>
    public const string TIMESTAMP = $"{PREFIX}.ts";
}
