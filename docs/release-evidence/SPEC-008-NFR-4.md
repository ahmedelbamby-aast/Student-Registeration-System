# SPEC-008 NFR-4 Authorization and Privacy Evidence

**Artifact version:** 1.0.0

**Requirement:** SPEC-008 NFR-4

**Recorded UTC:** 2026-07-16T13:10:00Z

**Owner:** Ahmed ELbamby

## Scope

This quality gate verifies that student academic data remains restricted to
self-service and the approved minimum scope for staff. It executes the
delivered authorization policies and resource handlers, then inspects the
authorization metadata attached to the delivered SPEC-008 endpoints. The
authorization principals and resources are synthetic-only fixtures; no
production or institutional student data is used.

## Authorization matrix

| Principal and scope | Outcome | Executable reason |
|---|---|---|
| Student own resource: ALLOW | ALLOW | The authenticated Student has `AcademicProfile.ReadOwn`, and the resource owner matches the subject claim. |
| Student wrong resource: DENY | DENY | The resource owner does not match the authenticated Student subject. |
| Unauthenticated Student: DENY | DENY | The policy requires an authenticated principal. |
| Student missing AcademicProfile.ReadOwn: DENY | DENY | Student role membership cannot replace the required permission claim. |
| Named permitted Admin with AcademicProfiles.Manage: ALLOW | ALLOW | The named synthetic Admin has both the Admin role and the separately governed permission. |
| Admin missing AcademicProfiles.Manage: DENY | DENY | Admin role membership alone cannot enter full-profile scope. |
| Lecturer with Context.Read: DENY | DENY | Normal Lecturer dashboard access does not grant full-profile access. |
| TeachingAssistant with Context.Read: DENY | DENY | Normal TeachingAssistant dashboard access does not grant full-profile access. |

The test also confirms that the delivered permission catalogue gives Lecturer
and TeachingAssistant only their normal `Context.Read` permission for this
matrix, while the named permitted Admin receives the separately governed
`AcademicProfiles.Manage` permission.

## Endpoint policy metadata

| Endpoint | Method | Required policy |
|---|---|---|
| `/api/students/me/academic-context` | GET | `AcademicProfile.ReadOwn` |
| `/api/admin/students` | GET | `AcademicProfiles.Manage` |
| `/api/admin/students/{studentId}/academic-context` | GET | `AcademicProfiles.Manage` |
| `/api/admin/students/{studentId}/academic-profile` | PATCH | `AcademicProfiles.Manage` |

None of these profile endpoints carries anonymous-access metadata. Student
self-service remains distinct from Admin full-profile detail and correction.
Lecturer and TeachingAssistant access may be expanded only by later,
separately approved minimum roster projections; this evidence grants no such
projection and no general profile access.

## Privacy boundary

The evidence records aggregate authorization outcomes only. It contains
no University IDs, credentials, cookies, or security stamps, and
no full student profile fields or values. The executable fixtures use opaque
synthetic subject identifiers solely in process; logs, snapshots, and this
release record do not emit those identifiers or academic records.

## Source binding

- Quality test normalized-LF SHA-256: `3E567D8F45E0FE9D5B0AFAE83D114722B497FE5D26D37A36A3AB5EDD0FB5795E`
- RolePolicies normalized-LF SHA-256: `9CFDB94C86D2784DF932B21A7BA5748326C73BD31986D77C6A48AFB2CB16CE68`
- Spec008Endpoints normalized-LF SHA-256: `E303BEFD995F8269561509415E29697988BBC836B592F35FEEF133E21E3C6D0E`

## Executed command

Run from the repository root:

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec008.NFR_4EvidenceTests" --logger "console;verbosity=minimal" -p:TreatWarningsAsErrors=true
```

## Measured result

- Focused tests executed: 5
- 5 passed
- 0 failed
- Configuration: Release, warnings treated as errors

**Result: PASS.**
