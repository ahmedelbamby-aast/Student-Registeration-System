namespace StudentRegistration.IdentityAccess.Application.Ports;

/// <summary>
/// Supplies the replica-shared secret used only to pseudonymize abuse subjects.
/// Implementations must source the key outside tracked configuration.
/// </summary>
public interface IIdentityAbuseKeyProvider
{
    ReadOnlyMemory<byte> GetKey();
}
