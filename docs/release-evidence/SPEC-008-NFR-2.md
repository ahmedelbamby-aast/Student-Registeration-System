# SPEC-008 NFR-2 Release Evidence

**Owner:** Ahmed ELbamby

**Recorded UTC:** 2026-07-16T10:12:21.0011556Z

**Profile:** SPEC008-AUTHENTICATED-CONTEXT-1.0
**Result: PASS.**

## Executed gate

The executable gate used 25,000 synthetic student accounts in one migrated
SQL Server database. Two independently hosted, stateless API replicas shared
that database and a shared SQL data-protection key ring. After 100 excluded
warm-up reads, the runner scheduled a continuous ten-minute run of authenticated
`GET /api/context` requests at 300 reads per second with no retries.

Authentication used ASP.NET Core cookie authentication and the real production
composition. Replica 1 protected each session ticket; the fixture proved the
same cross-replica ticket was accepted by both replicas. Every measured request
then passed through `IdentityCookieAuthenticationEvents`, security-stamp validation
against SQL, the `Context.Read` policy, the student academic-scope reader, and
`AcademicSessionContextAdapter`. No fake or header authentication handler was
registered.

## Required metrics

| Metric | Value |
|---|---:|
| Synthetic students | 25,000 |
| Duration seconds | 600 |
| Scheduled reads per second | 300 |
| Total attempted reads | 180,000 |
| Replica count | 2 |
| Replica 1 attempted reads | 90,000 |
| Replica 2 attempted reads | 90,000 |
| Actual p95 latency milliseconds | 15.6454 |
| Unexpected failure count | 0 |
| Unexpected failure rate | 0.000000% |

## Supporting measurements

| Measurement | Result |
|---|---:|
| Completed reads | 180,000 |
| Non-success HTTP responses | 0 |
| Invalid application contexts | 0 |
| Transport failures | 0 |
| Scheduler elapsed milliseconds | 600,002.1577 |
| Scheduler p95 lag milliseconds | 0.3972 |
| Scheduler maximum lag milliseconds | 9.3736 |
| Maximum observed in flight | 29 |
| Backpressure events | 0 |
| Warm-up reads excluded | 100 |

The measured scheduler rate was approximately 299.999 reads per second. The
nearest-rank p95 was 15.6454 ms, below the 300 ms limit. All 180,000 attempts
completed, each replica received exactly 90,000 attempts, no backpressure was
observed, and the unexpected failure rate was below 0.1%.

## Reproducibility and binding

Run from the repository root with Docker available:

```powershell
dotnet test tests/StudentRegistration.LoadTests/StudentRegistration.LoadTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.LoadTests.Specs.Spec008.AcademicContextLoadTests.Authenticated_context_meets_the_exact_two_replica_gate" -p:TreatWarningsAsErrors=true --logger "console;verbosity=minimal"
```

- SQL image: `mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56`
- Migration: `20260713010000_IdentityAcademicFoundation`
- Seed profile: `synthetic-fixture/1.0`
- .NET SDK: `10.0.301`
- Docker Server: `29.4.2`
- Host: Microsoft Windows 11 Pro `10.0.26200`, 12th Gen Intel Core i7-12700H, 20 logical processors
- Total focused test duration reported by the test runner: 10 minutes 34 seconds

Load test normalized-LF SHA-256: `8DECDF5EB91DB4C9D882289D8A3A0D78C0DE983B187342C6B2849CF8BCB1B6BE`

Fixture normalized-LF SHA-256: `812FA0DFC9433D995ABBF96FB945BE48CE3463F56AA575377122222B54F2BAB8`

## Privacy and measurement boundary

The generated JSON and this document contain aggregate-only results:

- no University IDs
- no credentials
- no session cookies
- no security stamps
- no full student profiles

The ignored JSON artifact remains under `load-test-results/spec008` and is not
committed.

The replicas use in-process TestServer with the real application pipeline and a
real SQL Server container. Therefore this gate measures application and database
behavior but does not measure external network or TLS latency.

This SPEC-008 read-only context gate does not replace SPEC-018 or its later
mixed-load gate for concurrent reads, registration submissions, spikes, soak,
and failover behavior.
