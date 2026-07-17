# SPEC-017 FR-10 Command Metadata Evidence

Date: 2026-07-17
Evidence test: `AdminConfirmationConcurrencyTests` (3/3 green)

| Owner command | Expected version | Idempotency metadata |
|---|---|---|
| Create term | N/A (create) | `ClientRequestId` |
| Update term/windows | term and window rowversions | N/A (versioned update) |
| Create identity import | N/A (create) | `ClientRequestId` plus content/request hash |
| Publish identity import | import rowversion | `ClientRequestId` |
| Change user status / replace roles | user rowversion | N/A (versioned update) |
| Create catalogue import/policy | N/A (create) | `ClientRequestId` |
| Publish catalogue/policy | draft/policy rowversion and bound preview | `ClientRequestId` |
| Update policy | policy rowversion | N/A (versioned update) |
| Create offering/room | N/A (create) | `ClientRequestId` |
| Update group/room | aggregate and dependency rowversions | N/A (versioned update) |
| Publish offering | aggregate and dependency rowversions plus preview | `ClientRequestId` |
| Request audit export | immutable scoped request payload | `ClientRequestId` plus request hash |

The focused test had compile-safe expected-red evidence while the export
service was absent, then passed after its idempotency enforcement arrived.
