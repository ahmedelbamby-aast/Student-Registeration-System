# Demo Identity Provisioning Contract

**Contract version:** `demo-identity-provisioning/1.0`  
**Approved:** 2026-07-14 by Ahmed ELbamby  
**Scope:** Development and Testing only

## Purpose

The demo bootstrap creates realistic synthetic Student, Admin, Lecturer, and
Teaching Assistant identities without importing real institutional data. It is
repeatable for a given seed, safe to run after migrations, and unavailable in
Production.

## Generated records

- Student identities receive a unique synthetic University ID, display name,
  program/level details required by the demo, an enabled account, the Student
  role, and an unused `StudentActivation` row.
- Staff identities receive a unique synthetic username and staff number,
  display name, enabled state, and one or more server-owned Admin, Lecturer, or
  TeachingAssistant role assignments.
- Initial PIN/password values are generated with a cryptographic random source
  and satisfy the SPEC007 15-128 character password policy. The canonical
  ASP.NET Core Identity password hasher persists only a salted IdentityV3 hash.
- Seed reconciliation is idempotent by stable synthetic external key. It does
  not overwrite an activated password, reset security state, duplicate a
  University ID, or silently expand an existing role set.

## Environment behavior

| Environment | Credential handoff | Lifecycle |
|---|---|---|
| Testing | Returned to the isolated test fixture in process memory | Discarded with the per-run database and process |
| Development | Written once beneath `.local/credentials/` using create-new semantics | Explicitly ignored by Git and deleted within seven days |
| Production or any other environment | Bootstrap and local credential handoff reject startup/operation | No synthetic account or artifact is created |

Development credential sheets contain only the bounded synthetic login
identifier, initial credential, generated-at time, and expiry time. They are
never appended, overwritten, logged, served by the web host, committed, placed
in test output, or copied into release evidence. A guarded cleanup operation
removes expired local sheets; it never traverses outside the configured
repository-local `.local/credentials/` directory.

## Recovery proof boundary

Testing supplies an in-memory `IAccountRecoveryProofDelivery` adapter.
Development may write a bounded create-new proof artifact beneath
`.local/recovery/`, subject to the same seven-day maximum and path containment
rules. The recovery request HTTP response never contains the proof. Production
startup fails closed until an approved institutional delivery adapter is
configured.

## Admin-import credential handoff

Admin import publication crosses only the server-side
`IProvisionedCredentialHandoff`. In Development and Testing, `PrepareAsync`
creates or reuses a non-visible pending handoff keyed by import ID. The SQL
publisher then commits every user, password hash, role, activation record, and
bounded import result atomically before `CompleteAsync` makes that handoff
available. A rollback calls `AbortAsync`; a retry of an already-published import
idempotently completes its pending handoff. No secret, local artifact path, or
handoff reference is returned by the HTTP API or stored in SQL. Production has
no local adapter and fails closed until an institutional delivery mechanism is
approved.

## Prohibited persistence and disclosure

Plaintext passwords, PINs, recovery proofs, and full personal profiles must not
appear in SQL Server, migrations, source, configuration, logs, telemetry,
snapshots, exports, screenshots, test reports, or release evidence. Hashes are
verified with the canonical password/proof verifier; tests never compare
nondeterministic hash bytes.

## Operational sequence

1. Validate the exact `Development` or `Testing` environment and guarded demo
   options.
2. Apply the owner-approved Code First migrations.
3. In one bounded transaction, reconcile deterministic synthetic identities,
   hash newly generated credentials, and create activation/role records.
4. Commit before handing transient credentials to the environment-specific
   sink. If the Development create-new handoff fails, report a safe support
   reference and never expose credentials through logs or HTTP.
5. Clear all in-process plaintext buffers/references as soon as handoff is
   complete and execute bounded expired-artifact cleanup.

This contract authorizes a non-production design-capability demo only. It is
not an AASTMT production identity process or credential policy.
