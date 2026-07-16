# SPEC-009 Dependency Baseline

**Recorded:** 2026-07-16  
**Feature:** SPEC-009 Catalogue, Prerequisites, and Policy Administration  
**Accepted repository revision:** `ab51a8c91c0615edfd6dc8ffe7e95d9237e1c2f8`  
**Result:** PASS

This baseline freezes the approved upstream contracts consumed by SPEC-009.
It authorizes dependency-ordered implementation for the non-production demo
only. It does not authorize production catalogue or policy publication,
Gate B-D, official AASTMT go-live, or release sign-off.

## T001 — SPEC-002 policy and provenance

- Accepted inputs: `requirements.md`, `policy-sources.md`,
  `policy-rules.md`, `data-model.md`, and the Gate A approval under
  `specs/002-aastmt-policy-rulebook/`.
- Runtime policy aggregates belong to SPEC-009; SPEC-002 remains the governed
  rulebook and provenance source.
- The approved profile is `DEMO-POC-2026.1`. Typed rules cover registration
  window, prerequisites, course GPA/earned-credit gates, standing, holds,
  normal load 9 through 18 credits, GPA-below-2.0 maximum 12 credits,
  capacity, and exact timetable overlap.
- The normal recommended target and hard maximum are 18 credits. The
  GPA-below-2.0 hard maximum is 12 credits. Waitlist, override, advisor,
  waiver, add/drop, withdrawal, and arbitrary executable policy scripts remain
  excluded and fail closed.
- `SRC-DATA-SCIENCE` is `OfficialAASTMT` provenance for the 19 selected codes,
  titles, sequence, and displayed prerequisite facts. `Credits=3` is the
  `DEMO-CREDITS-3` synthetic-demo field and is never represented as an
  official AASTMT value.
- Published rule versions are immutable and superseded. Decisions retain
  policy version, effective period, source/access metadata, classification,
  explanation, and privacy-safe input summary.

## T002 — SPEC-003 ADM-05 frontend contract

- Accepted inputs: `design/pages/ADM-05.md`,
  `design/components/catalogue.md`, `design/route-inventory.md`, and the
  approved responsive/accessibility contracts under
  `specs/003-ux-storyboard-accessibility/`.
- SPEC-009 is the canonical implementation owner for
  `CatalogueAdministrationPage.razor`; SPEC-003 remains the design and
  frontend-test contract owner. SPEC-017 may contribute later without
  duplicating the page or owner endpoints.
- Required ADM-05 states are loading, empty, success, validation error,
  service error, unauthorized, session expired, stale, and offline, plus the
  named draft, invalid, cycle, simulate, publish, and stale-publish fixtures.
- The page preserves semantic/focus order at 320, 375, 768, 1024, 1280, and
  1920 CSS pixels, uses keyboard-operable controls, visible focus, status/live
  regions, actionable error summaries, and no color-only meaning.
- The contributor is currently `not-pinned`; SPEC-009 pins it only after its
  contract, component, accessibility, E2E, and visual evidence exists.

## T003 — SPEC-005 persistence, history, and audit

- Accepted inputs: `immutable-history/1.0`, `unique-invariants/1.0`,
  `check-constraints/1.0`, `concurrency-tokens/1.0`,
  `docs/data/relational-invariants.md`,
  `docs/data/import-provenance-contract.md`, and
  `docs/data/code-first-ownership-map.md`.
- Infrastructure.SqlServer remains the sole DbContext and migration composer.
  SPEC-009 contributes only
  `Persistence/Configurations/CatalogueModelConfiguration.cs`.
- Programs, courses, curricula, and prerequisites cannot cross a catalogue
  version. Normalized codes are unique in scope; prerequisite edges cannot
  reference themselves or form a cycle.
- Published catalogue and policy versions are immutable and superseded.
  Drafts and imports are versioned mutable aggregates using SQL rowversion.
- Import evidence includes target draft, source/reference, content hash,
  access/import time, classification, synthetic-field manifest, state,
  privacy-safe row errors, and correlated actor/audit evidence.
- Publication and its append-only audit fact commit in one local SQL
  transaction. Audit failure rolls back publication and activation changes.
- The `S2CatalogueScheduling` migration remains owned by SPEC-010. SPEC-009
  delivers its mapping contribution now without creating a second migration
  or DbContext writer.

## T004 — SPEC-006 HTTP and command conventions

- Accepted inputs: `contracts/api.md`, `contracts/dto-isolation.md`, canonical
  `ApiError`, `Page<T>`, `ExpectedRowVersion`, and idempotency contracts.
- Lists default to page 1 and page size 20, allow at most 100, return
  `400 PAGE_SIZE_INVALID` for invalid values without silent capping, and use
  an allow-listed sort ending in immutable ID.
- Protected operations authorize before lookup or version disclosure.
  Unauthorized requests return 401/403 without resource, rowversion, or
  `currentVersion` disclosure.
- Versioned updates carry expected rowversion in the request body. Stale
  authorized commands return `409 STALE_VERSION`; `If-Match`/412 is outside
  the MVP.
- Retryable create/publish commands bind an opaque `clientRequestId` to owner,
  scope, and server-canonical payload. Same-payload replay returns the stored
  result; different-payload reuse returns
  `409 IDEMPOTENCY_KEY_REUSED`.
- Preview tokens bind actor, normalized scope, canonical content hash,
  dependency versions, and expiry. Changed content/version/dependency returns
  `409 STALE_PREVIEW` with no write.
- DTOs are explicit allow-listed shapes. EF entities, navigation graphs,
  arbitrary property names, executable expressions, and unbounded diagnostics
  never cross the API boundary.

## T005 — SPEC-008 academic identifiers and fixtures

- Accepted inputs: `contracts/api.md`, `data-model.md`, the Academics runtime
  contracts, and completed SPEC-008 release evidence.
- SPEC-009 consumes authoritative `AcademicTerm.Id`, stable ProgramCode,
  Cohort, GPA, EarnedCredits, Standing, transcript course codes, and
  student-term context. It does not redefine SPEC-008 term/profile entities.
- Program and course references remain normalized sourced codes at the
  SPEC-008 boundary; SPEC-009 resolves them through published catalogue
  versions without introducing a reverse foreign key into existing profile
  history.
- Policy simulation fixtures include normal and GPA-below-2.0 load boundaries,
  completed prerequisite codes, GPA 2.0, earned credits 95/96, active
  standing, holds, and explicit term identifiers.
- Server time and configured term state are authoritative. Browser time and
  client-selected academic scope are never accepted as policy authority.

## T006 — SPEC-018 security, load, audit, and operations

- Accepted inputs: `requirements.md`, the reviewed STRIDE threat model,
  CI/release gates, synthetic-data rules, and approved POC load profiles.
- Catalogue reads inherit the 300 ms p95 target at the 300-read/s workload.
  SPEC-009 separately measures 10,000-row validation within 30 seconds and
  deterministic simulations for fixed version/input.
- All test data is synthetic. Secrets and generated credentials do not enter
  source, SQL, migrations, telemetry, or evidence.
- Protected Admin endpoints require positive permission evidence and negative
  unauthenticated, wrong-role, missing-permission, and direct-object tests.
- Logs/audit use server time, correlation, allow-listed safe fields, bounded
  cardinality, and no raw imported content, credentials, or full student
  profiles.
- SQL or audit failure returns a safe no-partial result. Transaction,
  idempotency, and publication correctness cannot be relaxed by load or
  availability conditions.
- Production hosting, production catalogue sources, final secret provider,
  recovery authority, and official AASTMT publication remain separate,
  fail-closed release decisions.

## Dependency result

The dependency graph
`002/003/005/006/008/018 -> 009` is acyclic. Canonical owner and writer
boundaries are preserved: Academics owns SPEC-009 domain/application/endpoints,
Contracts owns shared transport primitives, Infrastructure.SqlServer composes
the single persistence model, and Client owns only the approved ADM-05 page.

