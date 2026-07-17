# SPEC-015 Dependency Baseline

**Recorded:** 2026-07-17  
**Feature:** SPEC-015 Student Registration Records  
**Repository source baseline:** `9510aeea58dab6b6f98e2cff1075e306644d6159`  
**Result:** PASS for T002-T005; route execution remains subject to the
contributor-pin gate below.

This baseline freezes only the approved contracts consumed by SPEC-015. It
authorizes dependency-ordered non-production demo work after T006/T007 pass.
It does not authorize production deployment, official AASTMT use, Gate B-D,
or release sign-off.

## Accepted artifacts

| Task | Specification | Accepted status/version | Artifact SHA-256 (`requirements`, `plan`, `data-model`, `contracts/api`) |
|---|---|---|---|
| T002 | SPEC-003 | Gate A approved; `page-design-record/1.1`, route manifest `2.1.0`, route inventory `1.1`, immutable STU-06/STU-07 records `1.0` | `0461b4d864a5898cc02ee124103d3199517754173400df6c280bb8dba19c3491`, `283c7efd72ae5a3e0a55abc7d81d45e471b4a9bc9a58a0cad81daba34d9d281f`, `e0af1871709e4e28de2cf99eee7f31ebf63344d1b71e0864879ec7e1cfb566df`, `03218fb4aadae2e2b9420559316816776aa4530eeec117fcedd9b98798a9c31e` |
| T003 | SPEC-008 | Approved clarified academic/context baseline, standing approval 2026-07-14 | `152f102d24c3c7f44b7b57400e76c590082306cea5e0fdbed20773dcd312c5b5`, `ba6c8fd308b85fe3db8d51d2d87b0ecc72871cb9a82f57f0ac6f7257c392949f`, `6bbf406a1f9d3afb55424e4dc5091bfe3162a01d464671b19c02afbfc8d49678`, `2e4d6177c66ab60c2214e9fd008c8531c7fecb28dfccfcc804cb50debc61e257` |
| T004 | SPEC-014 | Approved reconciled registration baseline, reaffirmed 2026-07-17; implementation tasks 123/123 complete | `b853cd5885b4bd859b6d563ea76b1b1dd1a8b473f7ff9614da4ffd415af578f9`, `2a1912f7aebfd1854e962f84eabc0f4bb8c270dbdb7f64f36f89752f6090a861`, `fb62f2be5ed8ea8b60b5e14779bc67b0ad6e579053abad350e5c39e0e5c97231`, `9f222078261d4be486de20e9cb101e6ea102ac5bcf63dc52c90bc70ef25cfb07` |
| T005 | SPEC-018 | Gate A approved quality/security/operations baseline | `9e0973c6670fec97733c2c0de15ffb4a5597889497216829c0a90f87fcd7118d`, `9dc035269854e7f142a9d4db323240360edcf43a187ca7af1b85e56215f6565a`, `940e519d5e9fad78908a00a40f9c0b905f4e41edc190ec8766e6220450814486`, `0423b7ab07d56a1e529a00378f24c0238a12bdcb628d107cbbb8eeee2dd07c6b` |

## T002 - SPEC-003 frontend contract

- STU-06 is `/student/registration/result/{id}`, implemented only by SPEC-015,
  and consumes the SPEC-014 by-request result plus the SPEC-015 owned-detail
  contract.
- STU-07 is `/student/registrations`, implemented only by SPEC-015, and
  consumes the SPEC-015 list, detail, and current-timetable reads.
- Both immutable Page Design Records are accepted at version `1.0`: STU-06
  SHA-256 `0f5b7018989afd96dab5c672f708fa18d9bfb37496ae8b28ee944bfc4876ab3b`;
  STU-07 SHA-256
  `f4789fe8d69e5cc95764c8d63d795f2e4eb16183f56680f847b9e85d92169ba4`.
- WCAG 2.2 AA, keyboard/focus, equivalent calendar/list, responsive,
  deterministic-fixture, browser, visual, and manual assistive-technology
  obligations remain binding.
- The immutable design records retain `design-only` provenance. T050-T053 MUST
  NOT start until the finalized SPEC-014/SPEC-015 contributor contract hashes
  are recorded in this downstream owner package. That downstream pin does not
  rewrite the approved design record.

## T003 - SPEC-008 academic context

- Server `TimeProvider`, configured IANA timezone, teaching term, registration
  term/window, and archived state are authoritative; the browser never derives
  current context.
- Term identity and lifecycle use the canonical shared DTO vocabulary.
  SPEC-015 reads the resulting context and does not redefine AcademicTerm,
  RegistrationWindow, Student, or StudentTermAcademicState.
- Student academic/profile data remains self or explicitly approved staff
  scoped and privacy-minimized.

## T004 - SPEC-014 canonical registration record

- SPEC-014 exclusively owns RegistrationSubmission, Enrollment,
  DecisionSnapshot, Reference, ReceiptSnapshot, the S6Registration migration,
  and the atomic write transaction.
- Accepted submissions have one globally unique human-safe Reference and
  immutable ReceiptSnapshot stored on the canonical RegistrationSubmission.
  Rejected submissions have no Reference or ReceiptSnapshot and retain a
  replayable rejection/decision result.
- Receipt snapshots retain term, course/group, credits, meeting
  identity/activity, meeting-bound Lecturer/TA, room/location, local day/time,
  PolicySet ID/version, and server submission time. DayOfWeek uses `0..6` in
  the configured term timezone.
- Lost responses are recovered through the authenticated term-scoped
  by-request lookup. SPEC-015 adds read projections only and creates no second
  receipt row, write path, migration, or transaction owner.
- The prerequisite architecture repair moved the shared registration boundary
  behind `Academics.Application.Ports` without changing registration behavior.
  Build, architecture 23/23, and focused SPEC-008/SPEC-014 boundary tests pass.
  The prior SPEC-014 load artifact's source fingerprint must be refreshed by
  an actual harness run before a final repository-wide quality claim.

## T005 - SPEC-018 quality and operations

- SPEC-015 inherits privacy-safe errors/correlation, PII-minimized
  logs/traces/evidence, synthetic-only non-production data, WCAG 2.2 AA,
  manual keyboard/NVDA evidence, and the approved browser matrix.
- Receipt/history reads participate in bounded read-load evidence; SPEC-015
  retains its stricter receipt target of at most 300 ms p95.
- Demo retention follows DEC-07: per-run Testing disposal, guarded Development
  reset, and seven-day cleanup for Git-ignored local logs/exports/credential
  artifacts. Production retention remains unapproved and fail closed.

## Dependency result

The direct graph `SPEC-003/SPEC-008/SPEC-014/SPEC-018 -> SPEC-015` is valid
and acyclic. Ownership is consistent: SPEC-014 writes the durable registration
aggregate; SPEC-015 exposes bounded, authorized read projections and the
canonical STU-06/STU-07 pages; SPEC-018 owns cross-cutting release evidence.
No queue, reporting database, duplicate receipt table, drop/correction
workflow, public receipt link, or distributed component is introduced.
