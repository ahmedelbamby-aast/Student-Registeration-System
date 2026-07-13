# Frontend Page Design and Functional-Test Matrix

Every row requires an approved Page Design Record before implementation.
Component, contract, E2E, accessibility, and visual IDs are planned test
families; executable tests are not created in this phase.

| ID | Route | Feature contract contributors (SPEC-003 designs all) | Primary composition | Minimum functional journeys |
|---|---|---|---|---|
| AUTH-01 | / | SPEC-008 | Public status, role gateway | normal, maintenance, unavailable, named navigation |
| AUTH-02 | /student/login | SPEC-007 | Auth layout, University-ID field, password field, validation summary | valid, invalid, locked, rate-limited, keyboard/error focus |
| AUTH-03 | /student/activate | SPEC-007 | Auth layout, institutional verification, password creation | valid, unknown, mismatch, claimed, expired, weak password, double submit |
| AUTH-04 | /staff/login | SPEC-007 | Staff auth layout without role picker | Admin/Lecturer/TA routing, invalid, disabled, no-role, MFA |
| AUTH-05 | /account/recovery | SPEC-007 | Recovery form and generic confirmation | sent, expired, used, rate-limited, success, enumeration resistance |
| STU-01 | /student | SPEC-008 | Student shell, academic summary, holds, registration CTA | open, upcoming, closed, no term, hold, incomplete profile |
| STU-02 | /student/subjects | SPEC-011 | Search/filter, offering list, reason panels | eligible, unavailable reasons, no results, reset, stale capacity, service error |
| STU-03 | /student/subjects/{offeringId} | SPEC-010/011 | Offering summary and group cards | open, nearly full, full, changed, unpublished, selected |
| STU-04 | /student/schedule | SPEC-012/013 | Calendar/list switcher, plan editor, conflict/alternatives panels | valid, recalculating, warning, conflict, stale, full, no solution |
| STU-05 | /student/review | SPEC-012/014 | Review summary, policy checks, blocking reasons, submit | valid, hard conflict, policy changed, window closed, double submit |
| STU-06 | /student/registration/result/{id} | SPEC-014/015 | Atomic result, receipt summary, reference | accepted, rejected/no partial result, lost-response recovery, denied |
| STU-07 | /student/registrations | SPEC-015 | Current/history selector, calendar/list, printable table | empty, current, history, archived, service error |
| STU-08 | /student/account | SPEC-007 | Institutional identity, security/session actions | read-only, validation, stale, password failure, sign-out-all confirmation |
| ADM-01 | /admin | SPEC-017 | Metrics cards, warnings, timestamp/degraded banner | live, paused refresh, stale, degraded, unauthorized |
| ADM-02 | /admin/terms | SPEC-008/017 | Term/window list and versioned editor | create, invalid dates, overlap, concurrent edit, publish |
| ADM-03 | /admin/users | SPEC-007/017 | Import preview, row errors, role editor | preview, duplicates, partial-invalid, disable, concurrent role change |
| ADM-04 | /admin/students | SPEC-008/017 | Sourced profile/transcript/hold view and correction dialog | read, invalid correction, stale, reason required, restricted |
| ADM-05 | /admin/catalogue | SPEC-009/017 | Version list, typed policy/catalogue editor, simulation | draft, invalid, cycle, simulate, publish, stale publish |
| ADM-06 | /admin/offerings | SPEC-010/017 | Offering/group editor and publication validation | missing resource, overlap, capacity mismatch, stale edit, publish |
| ADM-07 | /admin/resources | SPEC-010/017 | Room/availability/impact-alert table and grid alternative | imported, empty, unavailable, overlap, stale, keyboard text entry |
| ADM-08 | /admin/registrations | SPEC-014/017 | Read-only submission/fill/reconciliation monitor | live, stale, degraded, collision, paused-group support reference, no repair/correction action |
| ADM-09 | /admin/audit | SPEC-017 | Scoped search, immutable-event detail, export job | empty, pagination, queued, ready, failed, expired, restricted |
| STF-01 | /staff | SPEC-016 | Staff shell, role context, assignments, warnings | Lecturer, TA, dual role, no assignment, stale assignment |
| STF-02 | /staff/timetable | SPEC-016 | Calendar/list switcher and assignment details | current, history, unassigned 403, stale, equivalent views |
| STF-03 | /staff/groups/{groupId}/roster | SPEC-016 | Scoped roster table and paging | assigned, unassigned 403, empty, paged, assignment removed |
| STF-04 | /staff/availability | SPEC-016 | Text-range editor and timetable alternative | draft, saved, overlap, deadline, published warning, stale edit |
| SYS-01 | /status/{code} | SPEC-006/007 | Safe status panel and reference action | 403, 404, expired, maintenance, offline, unexpected without stack trace |

## Required test families

For every row, the combined SPEC-003 design/test-contract tasks and the
implementationOwner feature tasks MUST name:

- a component-state test file;
- an API-contract integration test file;
- a primary/failure Playwright journey file;
- an automated accessibility test file;
- a visual-regression file covering 375, 768, 1280, and 1920 CSS pixels.

SPEC-003 owns every Page Design Record and test contract. The
implementationOwner in .specify/route-manifest.json is the only spec allowed
to plan a write to the canonical Razor page. Other listed contributors supply
versioned feature contracts and tests without editing that page.
