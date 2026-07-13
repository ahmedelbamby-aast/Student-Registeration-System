# Route Contributor Baseline

**Baseline version:** `route-contributors/1.0`  
**Reviewed:** 2026-07-13 by Ahmed ELbamby  
**Result:** shared SPEC-003 infrastructure may proceed; every route remains design-only

All owner specifications have Gate A planning approval, but their runtime
contributions have not yet been delivered and validated at immutable contract
versions. `not-pinned` is an explicit blocked value, not a wildcard or approval
of “latest”. Page Design Records may be reviewed; canonical route source and
route-specific contract, component, accessibility, visual, and E2E work remain
blocked.

| Route ID | Implementation owner | Owner/contributor specs | Readiness | Exact contract pins | Missing evidence |
|---|---|---|---|---|---|
| AUTH-01 | SPEC-008 | SPEC-003, SPEC-008 | design-only | `not-pinned` | SPEC-008 implemented public-context/gateway contract |
| AUTH-02 | SPEC-007 | SPEC-003, SPEC-007 | design-only | `not-pinned` | SPEC-007 implemented student-login contract |
| AUTH-03 | SPEC-007 | SPEC-003, SPEC-007 | design-only | `not-pinned` | SPEC-007 implemented activation contract |
| AUTH-04 | SPEC-007 | SPEC-003, SPEC-007 | design-only | `not-pinned` | SPEC-007 implemented shared staff-login contract |
| AUTH-05 | SPEC-007 | SPEC-003, SPEC-007 | design-only | `not-pinned` | SPEC-007 implemented recovery contract |
| STU-01 | SPEC-008 | SPEC-003, SPEC-008 | design-only | `not-pinned` | SPEC-008 implemented student/context contract |
| STU-02 | SPEC-011 | SPEC-003, SPEC-011 | design-only | `not-pinned` | SPEC-011 implemented discovery/eligibility contract |
| STU-03 | SPEC-011 | SPEC-003, SPEC-010, SPEC-011 | design-only | `not-pinned` | SPEC-010 group plus SPEC-011 eligibility/details contracts |
| STU-04 | SPEC-012 | SPEC-003, SPEC-012, SPEC-013 | design-only | `not-pinned` | SPEC-012 plan/conflict plus SPEC-013 recommendation contracts |
| STU-05 | SPEC-014 | SPEC-003, SPEC-012, SPEC-014 | design-only | `not-pinned` | SPEC-012 validated plan plus SPEC-014 submission contracts |
| STU-06 | SPEC-015 | SPEC-003, SPEC-014, SPEC-015 | design-only | `not-pinned` | SPEC-014 idempotent result plus SPEC-015 receipt contracts |
| STU-07 | SPEC-015 | SPEC-003, SPEC-015 | design-only | `not-pinned` | SPEC-015 history/timetable contract |
| STU-08 | SPEC-007 | SPEC-003, SPEC-007 | design-only | `not-pinned` | SPEC-007 session/account contract |
| ADM-01 | SPEC-017 | SPEC-003, SPEC-017 | design-only | `not-pinned` | SPEC-017 operational metrics contract |
| ADM-02 | SPEC-008 | SPEC-003, SPEC-008, SPEC-017 | design-only | `not-pinned` | SPEC-008 term plus SPEC-017 administration contracts |
| ADM-03 | SPEC-007 | SPEC-003, SPEC-007, SPEC-017 | design-only | `not-pinned` | SPEC-007 identity plus SPEC-017 administration contracts |
| ADM-04 | SPEC-008 | SPEC-003, SPEC-008, SPEC-017 | design-only | `not-pinned` | SPEC-008 profile plus SPEC-017 administration contracts |
| ADM-05 | SPEC-009 | SPEC-003, SPEC-009, SPEC-017 | design-only | `not-pinned` | SPEC-009 catalogue/policy plus SPEC-017 administration contracts |
| ADM-06 | SPEC-010 | SPEC-003, SPEC-010, SPEC-017 | design-only | `not-pinned` | SPEC-010 offering plus SPEC-017 administration contracts |
| ADM-07 | SPEC-010 | SPEC-003, SPEC-010, SPEC-017 | design-only | `not-pinned` | SPEC-010 resources plus SPEC-017 administration contracts |
| ADM-08 | SPEC-017 | SPEC-003, SPEC-014, SPEC-017 | design-only | `not-pinned` | SPEC-014 result plus SPEC-017 operations contracts |
| ADM-09 | SPEC-017 | SPEC-003, SPEC-017 | design-only | `not-pinned` | SPEC-017 audit/export contracts |
| STF-01 | SPEC-016 | SPEC-003, SPEC-016 | design-only | `not-pinned` | SPEC-016 staff assignment/dashboard contract |
| STF-02 | SPEC-016 | SPEC-003, SPEC-016 | design-only | `not-pinned` | SPEC-016 timetable contract |
| STF-03 | SPEC-016 | SPEC-003, SPEC-016 | design-only | `not-pinned` | SPEC-016 scoped roster contract |
| STF-04 | SPEC-016 | SPEC-003, SPEC-016 | design-only | `not-pinned` | SPEC-016 availability contract |
| SYS-01 | SPEC-003 | SPEC-003, SPEC-006, SPEC-007 | design-only | `not-pinned` | SPEC-006 error envelope and SPEC-007 session/status contributions |

## Promotion rule

A row may become `implementation-ready` only when Ahmed ELbamby records each
implementation-owner and contributor contract by exact immutable version or
commit, confirms its implementation tests pass, and verifies that the Page
Design Record still matches. A changed or missing contributor immediately
returns the row to `design-only`.
