namespace StudentRegistration.IdentityAccess.Application.Ports;

public interface IAccountRecoveryProofDelivery
{
    Task<RecoveryProofDeliveryResult> DeliverAsync(
        RecoveryProofDeliveryRequest request,
        CancellationToken cancellationToken);
}

public sealed record RecoveryProofDeliveryRequest(
    Guid ApplicationUserId,
    string SubjectReference,
    string Proof,
    DateTime ExpiresAtUtc);

public sealed record RecoveryProofDeliveryResult(bool Accepted, string DeliveryReference);
