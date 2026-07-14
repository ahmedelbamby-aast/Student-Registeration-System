# SPEC-007 NFR-1 Login-Profile Evidence

**Profile:** `SPEC007-IDENTITY-LOGIN-1.0.0`  
**Run window:** 2026-07-14 12:13:16Z–12:23:16Z
**Scope:** SPEC-007 application-service demo gate  
**Result: PASS.**

Profile cardinality: 25,000 synthetic accounts.

## Exact profile

The measured run lasted 10 minutes at 25 login attempts/second across two
stateless replicas of `StudentAuthenticationService` and 25,000 synthetic
accounts. The 15,000 scheduled attempts used the required 80% valid, 15%
invalid credential, and 5% already locked distribution:

| Request class | Required ratio | Completed |
|---|---:|---:|
| Valid credential | 80% valid | 12,000 |
| Wrong password | 15% invalid credential | 2,250 |
| Pre-locked identity | 5% already locked | 750 |
| Total | 100% | 15,000 |

The profile used two independently constructed, stateless
`StudentAuthenticationService` replicas. Each replica had its own store adapter;
the adapters crossed one concurrency-safe shared-state boundary containing the
25,000 accounts. A separate always-on test rejects a profile fixture that
collapses those adapters into one replica-local object. Every request executed
the configured ASP.NET Core IdentityV3 password verifier at 100,000 PBKDF2
iterations. The timer includes scheduler delay and service execution from each
exact 40 ms target. A mismatched public outcome or thrown request increments a
blocking invariant or unexpected-error counter.

## Measured result

| Metric | Value |
|---|---:|
| Duration (seconds) | 600 |
| Scheduled/completed requests | 15,000 / 15,000 |
| Measured login p95 (ms) | 106.573 |
| Maximum permitted p95 (ms) | 500 |
| Unexpected errors | 0 |
| Unexpected error rate (%) | 0.0000 |
| Shared-state invariant violations | 0 |

The run therefore finished below the 500 ms p95 budget, below the 1% unexpected
error budget, and with zero shared-state invariant violations.

## Reproduction and evidence handling

The executable path is
`NFR_1EvidenceTests.Exact_profile_can_generate_measured_local_evidence`.
Setting `SPEC007_RUN_LOAD_PROFILE=1` runs the exact 600-second profile and writes
the metric-only JSON to `.local/evidence/SPEC-007-NFR-1.json`. `.local/` is
Git-ignored; the artifact contains no University IDs, usernames, passwords,
hashes, or recovery material. Normal test runs validate this checked-in
evidence without repeating the 10-minute measurement.

## Boundary retained

This measurement proves the SPEC-007 authentication service, configured
password hasher, exact account cardinality/mix, two independently composed
stateless service/store-adapter replicas over shared state, and outcome
invariants. It is not a claim of Production network, reverse-proxy, SQL
topology, or institutional identity-provider performance. The migrated
end-to-end HTTP/SQL two-host profile remains part of the complete-system
SPEC-018 release gate after downstream migrations and deployment topology
exist; Production remains not approved.
