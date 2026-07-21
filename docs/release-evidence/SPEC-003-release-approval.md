# SPEC-003 Release Approval Record

**Decision date:** 2026-07-21  
**Decision scope:** non-production design-capability demo  
**Human approval authority:** Ahmed ELbamby  
**Current decision:** APPROVED FOR NON-PRODUCTION DEMO; production withheld

Ahmed ELbamby explicitly approved implementation and evidence execution for
all remaining SPEC-003 tasks, and on 2026-07-21 approved all remaining demo
visual and manual dispositions. This authorizes the non-production demo only;
it does not turn waived human evidence into an institutional or production pass.

| Perspective | Reviewer / authority | Evidence and disposition | Decision |
|---|---|---|---|
| Product owner | Ahmed ELbamby | Approved the bounded MVP/demo scope, route set, and task closure work. | APPROVED |
| Domain owner | Ahmed ELbamby | Approved synthetic demo fixtures and server-authoritative dependency contracts; no institutional policy is inferred. | APPROVED |
| QA | Executable gates; human authority Ahmed ELbamby | Release build is warning-free; client unit 263/263, client contract 74/74, contracts 225/225, integration 649/650 with one governed environment skip, quality 266/266, and security 56/56. Ahmed approved the remaining demo-only visual and usability dispositions on 2026-07-21. | APPROVED FOR DEMO |
| Security | Server-authority and scope review | SPEC-003 owns no server endpoint; authorization, eligibility, time/term, conflict, capacity, and commit truth remain server-owned. Safe-reference and unknown-code behavior are covered. | APPROVED FOR DEMO SCOPE |
| Accessibility | Automated WCAG gates; human authority Ahmed ELbamby | Axe, keyboard, focus, contrast, target-size, zoom, and reflow evidence passes. Human NFR-8 execution is explicitly `WAIVED-DEMO`; a representative cohort including a real screen-reader participant remains required for production. | WAIVED-DEMO |
| Data / concurrency | Dependency contracts and EC-2/EC-10 gates | No database schema is owned here. Stale-plan re-fetch/revalidation and duplicate-action/idempotency behavior are covered by dependency-pinned tests. | APPROVED FOR DEMO SCOPE |
| Operations | Release/browser/performance gates | Published-host Brotli and the four-engine POC evidence are recorded. The real composed-host rerun measured p75 5,616ms for STU-02, 408ms for STU-04, and 404ms for STU-05 against 2,500ms. The remaining STU-02 overage is accepted only as Ahmed's non-production `WAIVED-DEMO`; actual Safari/macOS remains explicitly deferred and Playwright WebKit is not labelled Safari. | CONDITIONAL — DEMO WAIVER |

## Fail-closed decision

The SPEC-003 non-production demo decision is approved when each automated row
is `PASS` and the explicitly non-automatable rows carry Ahmed's exact
`WAIVED-DEMO` disposition. NFR-8/AC-12/SC-3 do not become production passes:
this record does not assert official AASTMT UAT, production go-live approval,
actual Safari support, or completion of a missing human usability cohort.
