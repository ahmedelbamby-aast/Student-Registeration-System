# SPEC-013 Release Approval Evidence

**Spec:** SPEC-013 Schedule Recommendations  
**Date recorded:** 2026-07-17  
**Approval authority:** Ahmed ELbamby  
**Release scope:** Bounded non-production demo only

Ahmed ELbamby's Gate A approval recorded on 2026-07-13 authorizes the bounded
demo implementation. This record consolidates the applicable review
perspectives after their named executable evidence passes; it is not
production approval or official AASTMT go-live authorization.

| Perspective | Status for demo scope | Evidence basis |
|---|---|---|
| Product owner | Approved | Gate A record, fixed selected-course and 18/18 boundaries, deterministic up-to-three recommendation scope. |
| Domain owner | Approved | SPEC-010/011/012 dependency baseline, published viable groups, coherent policy/catalogue/group versions, and no guessed travel rule. |
| QA | Approved for SPEC-013 evidence | Contract, application, acceptance, integration, E2E, quality, architecture, client, operations, migration, and release verification listed in `SPEC-013-traceability.md` passed. |
| Security | Approved for demo controls | Student self-scope, opaque Data Protection token, ten-minute expiry, shared key boundary, safe codes, bounded payloads, no durable token store or ownership oracle. Production key custody remains unapproved. |
| Accessibility | Approved for contributor scope | Semantic panel, native controls, live regions, text explanations/actions, responsive contract, and STU-04 contributor E2E evidence. No institution-wide certification is claimed. |
| Data and concurrency | Approved for demo controls | One coherent snapshot, exact dependency maps, Serializable complete-plan replacement, optimistic plan rowversion, no new table, and no capacity reservation. |
| Operations | Approved for isolated demo | Existing SPEC-018 SQL-backed key/telemetry boundaries, additive allow-listed codes/route, bounded optimizer, NFR evidence, and no new service/deployment. Production topology and capacity approval remain outside Gate A. |

Final approval is valid only while every T001-T066 checkbox has passing
evidence and the final verification commands remain green.

## Final verification record

- Debug solution build: passed with zero warnings and zero errors.
- SPEC-013 focused suites: Acceptance 10/10, Contract 4/4, Application 17/17,
  Integration 34/34, E2E 5/5, and Release Quality 20/20.
- Relevant regression suites: Architecture 23/23, Operations 16/16, Migration
  1/1, Client Unit 159/159, Client Contract 49/49, and Release 8/8.
- The aggregate solution test command was stopped after its 15-minute ceiling
  while executing the pre-existing long-running LoadTests project. Bounded
  regression execution also exposed two pre-existing, out-of-scope failures:
  SPEC-007 role-claim expectation drift and a SPEC-003/SPEC-012 historical
  `12 | 18` design assertion. SPEC-013 changed neither boundary and does not
  claim those unrelated failures as passed.
