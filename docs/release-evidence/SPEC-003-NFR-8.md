# SPEC-003 NFR-8 Controlled Usability Evidence

**Evidence version:** 1.1.0

**Status:** BLOCKED — REQUIRED HUMAN PARTICIPANT COHORT NOT PROVIDED

**Protocol owner and demo approval authority:** Ahmed ELbamby

**Human evidence approval:** WITHHELD

**Scope:** Non-production design-capability demo; not official AASTMT UAT

## Fail-closed result

NFR-8 is not passed. T266 remains open because no qualifying human-participant
sessions are recorded in this artifact. The 17 automated persona sessions
listed below are browser-test scenarios, not 17 unique representative humans.
They cannot be counted as participants, used to calculate the human completion
rate, or substituted for observed human usability findings.

Ahmed ELbamby is the project's sole developer and human approval authority,
but that governance role does not replace the representative participant cohort
explicitly required by NFR-8. Approval of the automated work or permission to
continue the demo therefore does not close this manual evidence gate.

## Required human cohort

At least 17 unique representative human participants must complete controlled
critical-journey sessions:

| Participant group | Minimum unique humans | Required representation |
|---|---:|---|
| Students | 8 | The student sample must include a novice participant, a keyboard-only participant, and a screen-reader participant. These may be members of the eight-person student sample. |
| Admin | 3 | Admin critical journeys. |
| Lecturer | 3 | Lecturer critical journeys. |
| Teaching Assistant | 3 | TA critical journeys. |
| **Total** | **17** | **At least 17 unique humans; automated personas do not contribute to this total.** |

For each human session, the record must identify the participant with a
privacy-safe unique code, participant group and applicable access mode,
environment and assistive technology where applicable, assigned script,
observed completion result, assistance required, observations, and linked
defects. Go-live usability completion passes only when at least 90% of the
qualifying human participants complete their critical journeys and no critical
or major core-flow usability defect remains unresolved.

## Supporting automated persona inventory

The following executable scenarios help prepare the human scripts and expose
repeatable functional/accessibility behavior. Every row is automation-only and
records no human observation or participant result.

| Automated session | Simulated perspective and mode | Critical journey | Executable evidence | Human evidence status |
|---|---|---|---|---|
| S01 | Student, novice persona | Discover eligible subject and understand reasons | `SubjectDiscoveryPageFeatureTests.Stu_02_primary_renders_server_load_reasons_and_complete_group_bundle` | NOT PROVIDED |
| S02 | Student, keyboard-only persona | Search/filter and reach the next result page | `Stu_02_search_filters_and_page_are_sent_to_the_server` plus STU-02 accessibility suite | NOT PROVIDED |
| S03 | Student, screen-reader/NVDA persona | Understand unavailable prerequisite/policy explanation | `Stu_02_unavailable_offering_preserves_required_current_policy_and_source` plus axe/landmark run | NOT PROVIDED |
| S04 | Student, recovery-oriented persona | Recover from empty/service-error without stale success | `Stu_02_empty_reset_and_service_retry_do_not_reuse_cached_success` | NOT PROVIDED |
| S05 | Student, schedule-conflict persona | Identify all overlaps in calendar and chronological list | `ScheduleBuilderPageFeatureTests.Stu_04_loading_then_success_uses_server_18_18_and_equivalent_views` | NOT PROVIDED |
| S06 | Student, high-zoom persona | Resolve schedule at 400% effective zoom | STU-04 accessibility and responsive suite | NOT PROVIDED |
| S07 | Student, submission-review persona | Review valid plan and prevent duplicate action | STU-05 primary browser and component pending-action suite | NOT PROVIDED |
| S08 | Student, blocking-error persona | Understand hard conflict and disabled submission reasons | `RegistrationReviewPageFeatureTests.Stu_05_hard_conflict_v1_lists_every_blocker_and_never_posts` | NOT PROVIDED |
| A01 | Admin operations persona | Read timestamped metrics and staleness | `AdminDashboardPageFeatureTests` | NOT PROVIDED |
| A02 | Admin catalogue persona | Validate/publish and recover from stale version | `CatalogueAdministrationPageFeatureTests` | NOT PROVIDED |
| A03 | Admin registration/audit persona | Inspect scoped registration/audit data | `RegistrationAdministrationPageFeatureTests` and `AuditAdministrationPageFeatureTests` | NOT PROVIDED |
| L01 | Lecturer persona | Reach only authorized assignments | `StaffDashboardPageFeatureTests` Lecturer fixture | NOT PROVIDED |
| L02 | Lecturer persona | Read equivalent timetable | `StaffTimetablePageFeatureTests` | NOT PROVIDED |
| L03 | Lecturer persona | Open only an assigned roster | `StaffRosterPageFeatureTests` Lecturer fixture | NOT PROVIDED |
| T01 | Teaching Assistant persona | Reach only TA assignments | `StaffDashboardPageFeatureTests` TA fixture | NOT PROVIDED |
| T02 | Teaching Assistant persona | Read only assigned roster | `StaffRosterPageFeatureTests` TA fixture | NOT PROVIDED |
| T03 | Teaching Assistant persona | Edit availability and recover from stale/deadline state | `StaffAvailabilityPageFeatureTests` | NOT PROVIDED |

The automated environment is Windows 11 Pro 10.0.26200 x64. Exact Chrome,
Edge, Firefox, and Playwright WebKit builds are recorded in
`tests/StudentRegistration.E2ETests/browser-matrix.json`; Playwright WebKit is
not Safari. Fixtures are synthetic and versioned, and no institutional student
data is used.

## Missing observations and activation condition

No qualifying human session, first-run completion result, assistance record,
completion percentage, or human-discovered defect disposition is currently
recorded. The completion percentage is therefore **not calculable**, not zero
and not passed.

T266, NFR-8, AC-12, SC-3, and the SPEC-003 release gate remain blocked until
the complete minimum cohort above is observed and recorded, the completion
rate is at least 90%, every critical or major core-flow usability defect is
resolved, and Ahmed ELbamby approves the resulting human evidence. Automated
browser, axe, keyboard, focus, responsive, and persona evidence may support
that decision but cannot satisfy the missing-human condition.
