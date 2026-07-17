# SPEC-015 Complete Traceability Evidence

**Recorded:** 2026-07-17
**Scope:** bounded non-production Student Registration Records demo
**Result:** PASS

Release is rejected if any matrix row loses its named implementation,
executable test, or passing evidence document. Production deployment and
official AASTMT authorization are not approved by this matrix.

## Functional requirements

| ID | Delivery | Passing evidence |
|---|---|---|
| FR-1 | `RegistrationReceipt.cs`; canonical projection in `RegistrationReceiptModelConfiguration.cs` | `RegistrationReceiptModelTests.cs`; `RegistrationReceiptModelConfigurationTests.cs`; `AC-1Tests.cs` |
| FR-2 | `RegistrationReceiptService.cs`; `RegistrationRecordContracts.cs` | `AC-1Tests.cs`; `Endpoint02ContractTests.cs`; `RegistrationResultPageFeatureTests.cs` |
| FR-3 | `RegistrationReceiptService.cs` rejection projection | `AC-2Tests.cs`; `Endpoint02And05BehaviorTests.cs`; `RegistrationResultPageFeatureTests.cs` |
| FR-4 | `RegistrationRecordQueries.cs`; `SqlRegistrationRecordReader.cs`; STU-07 | `Endpoint01And04BehaviorTests.cs`; `Endpoint03BehaviorTests.cs`; `RegistrationHistoryPageFeatureTests.cs` |
| FR-5 | exact read policies and Admin audit in `SqlRegistrationRecordReader.cs` | `AC-3Tests.cs`; `NFR-2EvidenceTests.cs`; five endpoint contract tests |
| FR-6 | shared calendar/list semantic model on STU-07 | `AC-5Tests.cs`; `NFR-3EvidenceTests.cs`; `RegistrationRecordsPageAccessibilityTests.cs` |
| FR-7 | immutable receipt/decision snapshots projected from SPEC-014 | `AC-4Tests.cs`; `DecisionSnapshotModelTests.cs`; `NFR-4EvidenceTests.cs` |
| FR-8 | `RegistrationRecordActionPolicy.cs`; read-only endpoint surface | `AC-5Tests.cs`; `RegistrationRecordsPageContractTests.cs`; `SPEC-015-scope-review.md` |

## Non-functional and success criteria

| ID | Passing evidence |
|---|---|
| NFR-1 | `NFR-1EvidenceTests.cs`; `SPEC-015-NFR-1.md` — 9.314 ms p95, limit 300 ms |
| NFR-2 | `NFR-2EvidenceTests.cs`; `SPEC-015-NFR-2.md` |
| NFR-3 | `NFR-3EvidenceTests.cs`; `SPEC-015-NFR-3.md` |
| NFR-4 | `NFR-4EvidenceTests.cs`; `SPEC-015-NFR-4.md` |
| SC-1 | `AC-1Tests.cs`; `RegistrationReceiptModelTests.cs`; unique canonical reference/receipt constraints |
| SC-2 | `AC-2Tests.cs`; `RegistrationSubmissionModelTests.cs`; explicit no-partial rejection |
| SC-3 | `AC-4Tests.cs`; `NFR-4EvidenceTests.cs`; eight archived snapshots survive later mutable edits |

## Acceptance and edge cases

| ID | Direct executable evidence |
|---|---|
| AC-1 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-1Tests.cs` |
| AC-2 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-2Tests.cs` |
| AC-3 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-3Tests.cs`; `NFR-2EvidenceTests.cs` |
| AC-4 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-4Tests.cs`; `NFR-4EvidenceTests.cs` |
| AC-5 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-5Tests.cs`; route accessibility/visual/browser tests |
| AC-6 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-6Tests.cs`; all four SPEC-015 NFR evidence files |
| EC-1 | `tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-1Tests.cs` |
| EC-2 | `tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-2Tests.cs` |
| EC-3 | `tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-3Tests.cs` |
| EC-4 | `tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-4Tests.cs` |

## Routes and API endpoints

| Boundary | Delivery | Passing evidence |
|---|---|---|
| STU-06 `/student/registration/result/{id}` | `RegistrationResultPage.razor` | `RegistrationResultPageFeatureTests.cs`; component, route-contract, accessibility, and visual suites |
| STU-07 `/student/registrations` | `RegistrationHistoryPage.razor` | `RegistrationHistoryPageFeatureTests.cs`; component, route-contract, accessibility, and visual suites |
| GET `/api/student/registrations` | `Spec015Endpoints.cs` | `Endpoint01ContractTests.cs`; `Endpoint01And04BehaviorTests.cs` |
| GET `/api/student/registrations/{submissionId}` | `Spec015Endpoints.cs` | `Endpoint02ContractTests.cs`; `Endpoint02And05BehaviorTests.cs` |
| GET `/api/student/registrations/current/timetable` | `Spec015Endpoints.cs` | `Endpoint03ContractTests.cs`; `Endpoint03BehaviorTests.cs` |
| GET `/api/admin/students/{studentId}/terms/{termId}/registrations` | `Spec015Endpoints.cs` | `Endpoint04ContractTests.cs`; `Endpoint01And04BehaviorTests.cs`; `NFR-2EvidenceTests.cs` |
| GET `/api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}` | `Spec015Endpoints.cs` | `Endpoint05ContractTests.cs`; `Endpoint02And05BehaviorTests.cs`; `NFR-2EvidenceTests.cs` |

## Scope and release rule

OS-1, OS-2, OS-3, and OS-4 are proved absent in
`SPEC-015-scope-review.md`. `TraceabilityEvidenceTests` verifies the complete
FR-1..FR-8, NFR-1..NFR-4, SC-1..SC-3, AC-1..AC-6, EC-1..EC-4, route, endpoint,
scope, and approval inventory without placeholders.
