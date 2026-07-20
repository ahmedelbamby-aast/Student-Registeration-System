# Frontend Page Design and Functional-Test Matrix

Every row requires an approved Page Design Record before implementation.
Component, contract, E2E, accessibility, and visual IDs are planned test
families; executable tests are not created in this phase.

Ahmed ELbamby approved the unified 30-route design amendment on 2026-07-20.
Every authenticated row uses the same AppShell, role-navigation, page-header,
card, form, table/list, status-panel, button, spacing, typography, focus,
responsive, and WCAG patterns. Role-specific data and actions are the only
intentional differences. Applicable capacity views use the fixed textual order
`Total / Enrolled / Held / Available`.

| ID | Route | Feature contract contributors (SPEC-003 designs all) | Primary composition | Minimum functional journeys |
|---|---|---|---|---|
| AUTH-01 | / | SPEC-008 | Public status, role gateway | normal, maintenance, unavailable, named navigation |
| AUTH-02 | /student/login | SPEC-007/008 | Auth layout, University-ID field, password field, validation summary | valid, invalid, locked, rate-limited, keyboard/error focus |
| AUTH-03 | /student/activate | SPEC-007/008 | Auth layout, generated University-ID/PIN first-use activation | valid, unknown, mismatch, activated, locked/rate-limited, double submit |
| AUTH-04 | /staff/login | SPEC-007/008 | Staff auth layout without a pre-auth role picker; post-auth authorized context selection | Admin/Lecturer/TA routing, invalid, disabled, no-role, password-only/no-second-factor |
| AUTH-05 | /account/recovery | SPEC-007 | Recovery form and generic confirmation | sent, expired, used, rate-limited, success, enumeration resistance |
| STU-01 | /student | SPEC-007/008/015 | Shared AppShell, academic summary, holds, enrollment/registration CTA, current timetable | open, upcoming, closed, no term, hold, incomplete profile, first-term auto-enrolled, term-two self-registration |
| STU-02 | /student/subjects | SPEC-011 | Search/filter, offering/group list, effective credit and reason panels | eligible, unavailable reasons, no results, reset, stale capacity, service error, normal 18, probation 12 |
| STU-03 | /student/subjects/{offeringId} | SPEC-010/011 | Offering/credit/prerequisite summary, complete group cards, capacity and approval state | open, nearly full, full, changed, unpublished, selected, pending approval/held, approved, rejected, expired/released, normal 18, probation 12 |
| STU-04 | /student/schedule | SPEC-012/013 | Calendar/list switcher, plan/load editor, conflict/alternatives and approval panels | valid, recalculating, warning, conflict, stale, full, no solution, normal 18, probation 12, overload 19-21 eligible/ineligible/pending |
| STU-05 | /student/review | SPEC-012/014 | Review summary, policy/approval checks, blocking reasons, submit | valid, hard conflict, policy changed, window closed, pending approval/held, overload pending/approved/rejected, double submit |
| STU-06 | /student/registration/result/{id} | SPEC-014/015 | Atomic result, receipt summary, reference | accepted, rejected/no partial result, lost-response recovery, denied |
| STU-07 | /student/registrations | SPEC-015 | Current/history selector, calendar/list, printable table | empty, current, history, archived, service error |
| STU-08 | /student/account | SPEC-007 | Institutional identity, security/session actions | read-only, validation, stale, password failure, sign-out-all confirmation |
| STU-09 | /student/roadmap | SPEC-008/009/011/014 | Level/term roadmap, prerequisite chains, enrollment ownership, capacity and subject approval state | loading, first-term auto-enrolled, second-term self-registration, completed, eligible, prerequisite blocked, pending approval/held, approved, rejected, expired/released, unavailable |
| ADM-01 | /admin | SPEC-007/008/010/017 | Metrics cards, warnings, timestamp/degraded banner | live, paused refresh, stale, degraded, unauthorized |
| ADM-02 | /admin/terms | SPEC-008/017 | Term/window list and versioned editor | create, invalid dates, overlap, concurrent edit, publish |
| ADM-03 | /admin/users | SPEC-007/017 | Import preview, row errors, role editor | preview, duplicates, partial-invalid, disable, concurrent role change |
| ADM-04 | /admin/students | SPEC-008/017 | Sourced profile/transcript/hold view and correction dialog | read, no results, invalid correction, stale, reason required, restricted |
| ADM-05 | /admin/catalogue | SPEC-009/017 | Version list, typed policy/catalogue editor, simulation | draft, invalid, cycle, simulate, publish, stale publish |
| ADM-06 | /admin/offerings | SPEC-010/017 | Offering/group editor and publication validation | missing resource, overlap, capacity mismatch, stale edit, publish |
| ADM-07 | /admin/resources | SPEC-010/017 | Room/availability/impact-alert table and grid alternative | imported, empty, unavailable, overlap, stale, keyboard text entry |
| ADM-08 | /admin/registrations | SPEC-014/015/017 | Read-only submission/fill/reconciliation monitor | live, no results, stale, degraded, collision, paused-group support reference, no repair/correction action |
| ADM-09 | /admin/audit | SPEC-017 | Scoped search, immutable-event detail, export job | empty, pagination, queued, ready, failed, expired, restricted |
| ADM-10 | /admin/approvals | SPEC-014/017 | Shared approval inbox/list/detail, capacity effect and reasoned decision confirmation | empty, paged, pending subject, pending overload, approved, rejected, expired/released, stale decision, unauthorized, service error |
| STF-01 | /staff | SPEC-007/008/016 | Staff shell, server-derived active role, assignments, warnings | Lecturer, TA, no assignment, stale assignment |
| STF-02 | /staff/timetable | SPEC-016 | Calendar/list switcher and assignment details | current, history, no assignments, stale, equivalent views |
| STF-03 | /staff/groups/{groupId}/roster | SPEC-016 | Scoped roster table and paging | assigned, unassigned 403, empty, paged, assignment removed |
| STF-04 | /staff/availability | SPEC-016 | Text-range editor and timetable alternative | draft, saved, overlap, deadline, published warning, stale edit |
| STF-05 | /staff/approvals | SPEC-014/016 | Role-scoped approval inbox/list/detail, capacity effect and reasoned decision confirmation | Lecturer scope, TA scope, empty, pending subject, pending overload, approved, rejected, expired/released, stale assignment/decision, forbidden |
| SYS-01 | /status/{code} | SPEC-006/007/008/018 | Safe status panel and reference action | healthy, degraded, unhealthy, 403, 404, expired, maintenance, offline, unexpected without stack trace |

## Required test families

For every row, the combined SPEC-003 design/test-contract tasks and the
implementationOwner feature tasks MUST name:

- a component-state test file;
- an API-contract integration test file;
- a primary/failure Playwright journey file;
- an automated accessibility test file;
- a visual-regression file covering 375, 768, 1280, and 1920 CSS pixels.

Critical Student registration and approval journeys additionally require a
browser test against the composed application and demo SQL data with no API
request interception. Deterministic intercepted fixtures remain required for
state coverage but do not satisfy the real-composition journey by themselves.

SPEC-003 owns every Page Design Record and test contract. The
implementationOwner in .specify/route-manifest.json is the only spec allowed
to plan a write to the canonical Razor page. Other listed contributors supply
versioned feature contracts and tests without editing that page.
