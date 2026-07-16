# SPEC-010 Non-Production Demo Release Approval

**Decision:** APPROVED for the SPEC-010 Development and Testing demo slice  
**Decision date:** 2026-07-16  
**Sole decision authority and developer:** Ahmed ELbamby  
**Governance boundary:** Gate A non-production design-capability demo only

Ahmed ELbamby's approved SPEC-010 implementation plan records the standing
Gate A authority for this bounded slice. The perspectives below are distinct
reviews performed by Ahmed; they are not separate people, institutional
signatories, or official AASTMT authorities. This record is not institutional approval
and does not authorize production deployment or go-live.

## Review perspectives

| Perspective | Decision and admitted evidence | Residual boundary |
|---|---|---|
| Admin user | APPROVED for the bounded ADM-06 offering create/edit/validate/publish journey and ADM-07 room, read-only availability, impact revalidation, and resolution journey. | SPEC-017 monitoring, audit, reporting, and export contributors remain downstream; no Admin availability edit or override is approved. |
| Staff representative | APPROVED for staff-owned complete availability declarations, optimistic concurrency, and read-only Admin planning use. | The SPEC-016 staff workspace remains the availability mutation owner; this approval grants Admin no staff-availability mutation authority. |
| Academic scheduling / product | APPROVED for complete activity bundles, canonical Tutorial with optional Section display label, stable validation reasons, capacity boundaries, and transactional publication. | Institution-wide timetable generation, automatic reassignment, waitlists, reservations, and force-over-capacity remain excluded. |
| Data / concurrency | APPROVED for the eight Scheduling mappings, S2 incremental migration, constraints, collision indexes, rowversions, stable publication locking, rollback/replay, and child-mutation parent-version triggers. | Production migration execution, production data reconciliation, and downstream registration persistence remain separately governed. |
| QA | APPROVED for the passing Spec 010 domain, consolidated, edge, acceptance, authorization, endpoint-contract, OpenAPI, browser, quality, mapping, and migration evidence recorded by this candidate. | Evidence is source-sensitive and must be rerun after relevant drift; this does not replace the broader SPEC-018 mixed-load, recovery, or release gates. |
| Security | APPROVED for `OfferingDetailsRead`, Admin-plus-`Offerings.Manage`, antiforgery on every Admin mutation, bounded inputs, safe `ApiError` contracts, DTO isolation, and explicit absence of Admin availability mutation routes. | No penetration-test sign-off, production secrets custody, institutional privacy authorization, or production threat acceptance is granted. |
| Accessibility | APPROVED for the delivered ADM-06 and ADM-07 semantic headings, landmarks, native controls, live status/error regions, keyboard-operable actions, readable concurrency outcomes, and read-only availability presentation. | This is page-slice evidence, not institution-wide accessibility certification; later contributor content must pass its owning gate. |
| Operations | APPROVED for isolated Development/Testing execution, deterministic fixtures, the modular monolith, the shared DbContext contribution, and pinned real-SQL migration proof. | Production topology, TLS/network acceptance, monitoring, backups, recovery rehearsal, support, capacity approval, and Gate B-D remain pending. |

## Evidence admitted

- [Complete traceability](SPEC-010-traceability.md)
- [Scheduling scope review](SPEC-010-scope-review.md)
- [NFR-1 read evidence](SPEC-010-NFR-1.md)
- [NFR-2 stable-code evidence](SPEC-010-NFR-2.md)
- [NFR-3 term-time evidence](SPEC-010-NFR-3.md)
- [NFR-4 bounded-list evidence](SPEC-010-NFR-4.md)
- Spec 010 model, consolidated application, edge, acceptance, authorization,
  endpoint-contract, OpenAPI, browser, quality, EF mapping, migration, and
  real-SQL suites

## Explicit pending and non-approvals

- SPEC-011 remains the STU-03 Razor-page and student-eligibility owner.
- SPEC-016 remains the staff availability workspace and mutation owner.
- SPEC-017 remains the broader Admin monitoring, audit, reporting, and export
  contributor. Its absence does not create a duplicate writer in SPEC-010.
- SPEC-018 continuous mixed-load, spike, soak, failover, recovery, and
  production release evidence is not replaced by the bounded NFR-1 component
  sample.
- Gate B, Gate C, Gate D, production migration execution, production data,
  official institutional scheduling authority, and AASTMT go-live approval
  are not granted.

Ahmed Elbamby approves only the evidenced synthetic non-production SPEC-010
demo slice under the standing Gate A authority.

**Result: APPROVED — NON-PRODUCTION SPEC-010 DEMO ONLY.**
