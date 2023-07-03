namespace advisor;

/// <summary>
/// Represents a player registration ID.
/// </summary>
public sealed class RegistrationId : NewType<RegistrationId, long> {
    public RegistrationId(long value) : base(value) { }
}
