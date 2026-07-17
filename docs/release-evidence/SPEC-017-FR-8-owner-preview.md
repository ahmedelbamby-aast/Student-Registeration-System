# SPEC-017 FR-8 Owner Preview and Confirmation Evidence

Date: 2026-07-17
Evidence tests: `AdminCommandInvariantTests`,
`AdminConfirmationConcurrencyTests` (8/8 combined green)

Governed changes stay with their canonical owners:

| Owner | Preview/confirmation boundary | Server-authority evidence |
|---|---|---|
| SPEC-007 Identity | UI confirmation plus expected user rowversion for status/role changes; validated import before publish | stale version and `FINAL_ADMIN_REQUIRED` remain owner results |
| SPEC-008 Academics | expected term/window versions and server overlap validation | no client conflict bypass |
| SPEC-009 Catalogue | signed actor/scope/content/dependency/version/expiry-bound publication preview | stale, replay, and payload mismatch tests green |
| SPEC-010 Scheduling | server validation preview plus offering/group/room/availability versions | capacity, resource, availability, and conflict checks remain authoritative |

No preview grants permission, no preview bypasses validation, and SPEC-017 adds
no generic confirmation service. Enrollment correction remains excluded.
