# Canonical Route Inventory

**Contract version:** `route-inventory/1.1-draft`<br>
**Source:** `.specify/route-manifest.json` version `2.1.0-draft`<br>
**Design owner:** `SPEC-003`<br>
**Amendment status:** Pending Ahmed ELbamby review

This is the exact MVP route inventory. A row marked `design-only` authorizes
design review, not route source or executable route tests. Implementation stays
with the single owner shown below. The version 1.1 draft reconciles omitted
API-authority and explicit composite contributors from the endpoint manifest;
it changes no route, page, or implementation owner.

| Route ID | Template | Canonical page | Implementation owner | Owner/contributor specs | Readiness |
|---|---|---|---|---|---|
| AUTH-01 | `/` | `RoleGatewayPage.razor` | SPEC-008 | SPEC-003, SPEC-008 | design-only |
| AUTH-02 | `/student/login` | `StudentLoginPage.razor` | SPEC-007 | SPEC-003, SPEC-007, SPEC-008 | design-only |
| AUTH-03 | `/student/activate` | `StudentActivationPage.razor` | SPEC-007 | SPEC-003, SPEC-007, SPEC-008 | design-only |
| AUTH-04 | `/staff/login` | `StaffLoginPage.razor` | SPEC-007 | SPEC-003, SPEC-007, SPEC-008 | design-only |
| AUTH-05 | `/account/recovery` | `AccountRecoveryPage.razor` | SPEC-007 | SPEC-003, SPEC-007 | design-only |
| STU-01 | `/student` | `StudentDashboardPage.razor` | SPEC-008 | SPEC-003, SPEC-007, SPEC-008, SPEC-015 | design-only |
| STU-02 | `/student/subjects` | `SubjectDiscoveryPage.razor` | SPEC-011 | SPEC-003, SPEC-011 | design-only |
| STU-03 | `/student/subjects/{offeringId}` | `SubjectDetailsPage.razor` | SPEC-011 | SPEC-003, SPEC-010, SPEC-011 | design-only |
| STU-04 | `/student/schedule` | `ScheduleBuilderPage.razor` | SPEC-012 | SPEC-003, SPEC-012, SPEC-013 | design-only |
| STU-05 | `/student/review` | `RegistrationReviewPage.razor` | SPEC-014 | SPEC-003, SPEC-012, SPEC-014 | design-only |
| STU-06 | `/student/registration/result/{id}` | `RegistrationResultPage.razor` | SPEC-015 | SPEC-003, SPEC-014, SPEC-015 | design-only |
| STU-07 | `/student/registrations` | `RegistrationHistoryPage.razor` | SPEC-015 | SPEC-003, SPEC-015 | design-only |
| STU-08 | `/student/account` | `StudentAccountPage.razor` | SPEC-007 | SPEC-003, SPEC-007 | design-only |
| ADM-01 | `/admin` | `AdminDashboardPage.razor` | SPEC-017 | SPEC-003, SPEC-007, SPEC-008, SPEC-010, SPEC-017 | design-only |
| ADM-02 | `/admin/terms` | `TermAdministrationPage.razor` | SPEC-008 | SPEC-003, SPEC-008, SPEC-017 | design-only |
| ADM-03 | `/admin/users` | `UserAdministrationPage.razor` | SPEC-007 | SPEC-003, SPEC-007, SPEC-017 | design-only |
| ADM-04 | `/admin/students` | `StudentAdministrationPage.razor` | SPEC-008 | SPEC-003, SPEC-008, SPEC-017 | design-only |
| ADM-05 | `/admin/catalogue` | `CatalogueAdministrationPage.razor` | SPEC-009 | SPEC-003, SPEC-009, SPEC-017 | design-only |
| ADM-06 | `/admin/offerings` | `OfferingAdministrationPage.razor` | SPEC-010 | SPEC-003, SPEC-010, SPEC-017 | design-only |
| ADM-07 | `/admin/resources` | `ResourceAdministrationPage.razor` | SPEC-010 | SPEC-003, SPEC-010, SPEC-017 | design-only |
| ADM-08 | `/admin/registrations` | `RegistrationAdministrationPage.razor` | SPEC-017 | SPEC-003, SPEC-014, SPEC-015, SPEC-017 | design-only |
| ADM-09 | `/admin/audit` | `AuditAdministrationPage.razor` | SPEC-017 | SPEC-003, SPEC-017 | design-only |
| STF-01 | `/staff` | `StaffDashboardPage.razor` | SPEC-016 | SPEC-003, SPEC-007, SPEC-008, SPEC-016 | design-only |
| STF-02 | `/staff/timetable` | `StaffTimetablePage.razor` | SPEC-016 | SPEC-003, SPEC-016 | design-only |
| STF-03 | `/staff/groups/{groupId}/roster` | `StaffRosterPage.razor` | SPEC-016 | SPEC-003, SPEC-016 | design-only |
| STF-04 | `/staff/availability` | `StaffAvailabilityPage.razor` | SPEC-016 | SPEC-003, SPEC-016 | design-only |
| SYS-01 | `/status/{code}` | `SystemStatusPage.razor` | SPEC-003 | SPEC-003, SPEC-006, SPEC-007, SPEC-008, SPEC-018 | design-only |

## Change control

- Route additions, removals, renames, or owner changes require an approved spec
  and a synchronized route-manifest version change.
- A route becomes `implementation-ready` only through the reviewed contributor
  baseline after exact implementation-owner and contributor contracts are pinned.
- SPEC-003 may implement only SYS-01; every other page source belongs to its
  listed implementation owner.
