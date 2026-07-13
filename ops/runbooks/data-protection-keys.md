# Data Protection key-ring runbook

## Development and test topology

Run the demo with **two replicas** against the same SQL Server database and
the same Data Protection application name. Both replicas read and write the
`DataProtectionKeys` table, so authentication and protected registration state
work **without sticky sessions**.

The key ring is protected by one **external certificate**. Supply its absolute
PFX path and password through environment-specific secret configuration or .NET
User Secrets. Never commit the PFX, its password, or generated key material.
The active certificate should be mounted outside the repository; the
Git-ignored `.local/credentials` path is only for short-lived demo exports.

Required settings:

- `DataProtection:ApplicationName`: identical on every replica.
- `DataProtection:Repository`: `SqlServer`.
- `DataProtection:Encryption`: `ExternalCertificate`.
- `DataProtection:CertificatePath`: absolute path to the mounted PFX.
- `DataProtection:CertificatePassword`: supplied from a secret source.

## Rotation

ASP.NET Core performs Data Protection key rotation in the shared SQL key ring.
Keep the same application name and wrapping certificate available to every
replica during ordinary key rotation. Rotate the wrapping certificate only in
a controlled maintenance window: retain the old certificate for recovery,
deploy the new certificate to every replica, then re-protect or regenerate demo
keys according to the approved operating procedure.

## Recovery

Restore the database and the same external certificate plus password before
starting either replica. If the certificate is lost in a non-production demo,
Ahmed may authorize a guarded reset of the synthetic key ring; that reset
invalidates existing cookies and protected demo data. It must never be treated
as Production recovery.

## Production authority gate

Production repository topology, certificate custody, backup, recovery, and
rotation remain fail-closed until explicit **Security/DevOps approval**. Both
`DataProtection:ProductionRepositoryApproved` and
`DataProtection:ProductionEncryptionApproved` must be true before Production
startup can proceed. The flags record approval; they do not replace an approved
secret store, certificate owner, or recovery exercise.
