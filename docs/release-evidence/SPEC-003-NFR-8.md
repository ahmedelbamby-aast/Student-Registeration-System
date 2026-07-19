# SPEC-003 NFR-8 Controlled Usability Evidence

**Evidence version:** 1.0.0  
**Status:** PENDING FINAL BROWSER RUN  
**Approver:** Ahmed ELbamby  
**Approval date:** 2026-07-19  
**Scope:** Non-production design-capability demo; not official AASTMT UAT

## Protocol

Ahmed ELbamby is the project's sole human approval authority and evaluates the
named product, UX, accessibility, and role perspectives. He explicitly
approved execution and closure of the remaining SPEC-003 tasks on 2026-07-19.
For this demo, the controlled evaluation uses 17 disclosed persona sessions,
not 17 claimed unique people. Each session combines an executable browser
journey with a structured review of task completion, error recovery, keyboard
reachability, wording, focus, and server-authority behavior.

Environment: Windows 11 Pro 10.0.26200 x64; exact Chrome, Edge, Firefox, and
Playwright WebKit builds are pinned in
`tests/StudentRegistration.E2ETests/browser-matrix.json`. WebKit is not Safari.
Fixtures are synthetic and versioned; no institutional student data is used.

Pass threshold: at least 90% of all sessions complete their critical journey,
with no unresolved critical or major core-flow usability defect. A first-run
failure remains a failure; diagnostics cannot promote it to pass.

## Representative sessions

| Session | Perspective and mode | Critical journey | Executable evidence | Result |
|---|---|---|---|---|
| S01 | Student, novice | Discover eligible subject and understand reasons | `SubjectDiscoveryPageFeatureTests.Stu_02_primary_renders_server_load_reasons_and_complete_group_bundle` | PENDING |
| S02 | Student, keyboard-only | Search/filter and reach the next result page | `Stu_02_search_filters_and_page_are_sent_to_the_server` plus STU-02 accessibility suite | PENDING |
| S03 | Student, screen-reader/NVDA perspective | Understand unavailable prerequisite/policy explanation | `Stu_02_unavailable_offering_preserves_required_current_policy_and_source` plus axe/landmark run | PENDING |
| S04 | Student, recovery-oriented | Recover from empty/service-error without stale success | `Stu_02_empty_reset_and_service_retry_do_not_reuse_cached_success` | PENDING |
| S05 | Student, schedule conflict | Identify all overlaps in calendar and chronological list | `ScheduleBuilderPageFeatureTests.Stu_04_loading_then_success_uses_server_18_18_and_equivalent_views` | PENDING |
| S06 | Student, high zoom | Resolve schedule at 400% effective zoom | STU-04 accessibility and responsive suite | PENDING |
| S07 | Student, submission review | Review valid plan and prevent duplicate action | STU-05 primary browser and component pending-action suite | PENDING |
| S08 | Student, blocking error | Understand hard conflict and disabled submission reasons | `RegistrationReviewPageFeatureTests.Stu_05_hard_conflict_v1_lists_every_blocker_and_never_posts` | PENDING |
| A01 | Admin, operations | Read timestamped metrics and staleness | `AdminDashboardPageFeatureTests` | PENDING |
| A02 | Admin, catalogue | Validate/publish and recover from stale version | `CatalogueAdministrationPageFeatureTests` | PENDING |
| A03 | Admin, registration/audit | Inspect scoped registration/audit data | `RegistrationAdministrationPageFeatureTests` and `AuditAdministrationPageFeatureTests` | PENDING |
| L01 | Lecturer | Reach only authorized assignments | `StaffDashboardPageFeatureTests` Lecturer fixture | PENDING |
| L02 | Lecturer | Read equivalent timetable | `StaffTimetablePageFeatureTests` | PENDING |
| L03 | Lecturer | Open only an assigned roster | `StaffRosterPageFeatureTests` Lecturer fixture | PENDING |
| T01 | Teaching Assistant | Reach only TA assignments | `StaffDashboardPageFeatureTests` TA fixture | PENDING |
| T02 | Teaching Assistant | Read only assigned roster | `StaffRosterPageFeatureTests` TA fixture | PENDING |
| T03 | Teaching Assistant | Edit availability and recover from stale/deadline state | `StaffAvailabilityPageFeatureTests` | PENDING |

## Observations and defects

Final observations, first-run totals, completion percentage, and defect
severity disposition are recorded only after the stable browser/accessibility
run. Until every `PENDING` result above is replaced with an observed result,
this artifact blocks T266 and the SPEC-003 release gate.
