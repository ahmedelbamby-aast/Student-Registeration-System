# Governed Permission Definitions

**Schema:** `permission-definition/1.0`
**Artifact owner:** SPEC-001
**Executable policy owner: SPEC-007**

These tokens are stable capability names. They define permitted operations and
maximum data scope; they do not grant access by themselves. SPEC-007 must
enforce them from authenticated server claims and resource scope.

## Permission definitions

| Permission token | Allowed role or principal | Data scope | Allowed operations | Explicit denial |
|---|---|---|---|---|
| `Context.Read` | Student, Admin, Lecturer, TeachingAssistant | server-composed current context | Read current identity, role, term, and service context | Route or request input cannot widen the scope |
| `AcademicProfile.ReadOwn` | Student | self | Read own academic profile, standing, holds, and transcript projection | No other student's record |
| `Catalogue.ReadAvailable` | Student | self plus published catalogue | Search and inspect server-evaluated availability | No unpublished catalogue or client-authored eligibility |
| `RegistrationPlan.ManageOwn` | Student | self | Read and change own current-term draft | No other student's draft or policy bypass |
| `Registration.SubmitOwn` | Student | self | Submit an idempotent current-term registration | No capacity, prerequisite, credit, hold, or conflict bypass |
| `RegistrationRecords.ReadOwn` | Student | self | Read own receipts, timetable, and history | No other student's records |
| `IdentityAccess.Manage` | Admin | institutional-admin | Provision supported staff accounts and replace effective staff roles with reason and concurrency tokens | No student-role grant, self-escalation, final-Admin removal, or plaintext credential read |
| `AcademicTerms.Manage` | Admin | institutional-admin | Configure and publish terms and registration windows | No invalid or overlapping published state |
| `CataloguePolicy.Manage` | Admin | institutional-admin | Manage versioned curricula, prerequisites, and approved policy configuration | No silent unapproved policy claim |
| `Offerings.Manage` | Admin | institutional-admin | Manage and publish offerings, activity groups, assignments, rooms, times, and capacities | No invalid staffing, room, time, or capacity publication |
| `RegistrationRecords.Read` | Admin | named StudentId plus TermId | Read an explicitly selected student's term registration list or detail with audit | No unrestricted dump or Lecturer/TA inheritance |
| `Audit.Read` | Admin | permission-filtered institutional audit | Search and inspect allowed audit events | No restricted security fields without their owning permission |
| `Reports.Export` | Admin | permission-filtered bounded report | Request and retrieve expiring audited exports | No unbounded, cross-permission, or permanent export |
| `TeachingAssignments.ReadOwn` | Lecturer, TeachingAssistant | self | Read own assignments and timetable | No unrelated staff assignment |
| `AssignedRosters.Read` | Lecturer, TeachingAssistant | assigned-groups | Read minimum roster fields for an effective assigned group | No unrelated group, expired assignment, or broad student profile |
| `Availability.ManageOwn` | Lecturer, TeachingAssistant | self | Read and change own planning availability with version checks | Admin cannot correct or override staff availability in the POC |
| `Registration.Reconcile` | service identity only | detected counter invariant | Repair a counter after verified enrollment truth and append operations audit | No grant to Student, Admin, Lecturer, TeachingAssistant, or public endpoint |

## Shared enforcement rules

- Authentication, role membership, permission, resource ownership, and data
  scope are revalidated by the API on every protected request.
- Client routes, hidden controls, submitted role values, and cached browser
  state never grant or widen a permission.
- Multiple effective roles form a set of allowed contexts, not a union into an
  all-powerful client session. The selected context must already exist in the
  server-derived role set.
- A missing policy, unsupported role, stale scope, or failed contributor denies
  the protected operation safely.
