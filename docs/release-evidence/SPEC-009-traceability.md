# SPEC-009 Complete Traceability Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T15:00:00Z  
**Boundary:** Approved Gate A non-production demo

## Functional requirements

| ID | Delivered implementation | Primary executable evidence |
|---|---|---|
| FR-1 | Catalogue draft, 19-course snapshot, field provenance, immutable version model | `CataloguePublicationService`, model tests, ADM-05 browser tests |
| FR-2 | Import target/source/access/hash/state/errors/synthetic manifest and validation | `ImportBatch`, endpoint 05-08 contracts, AC-1 |
| FR-3 | Duplicate, credit, missing-reference and cycle detection | catalogue validation tests, EC-1, AC-1/AC-2 |
| FR-4 | Typed effective-dated policy aggregate and approved demo rule set | `PolicySet`, `PolicyRule`, `PolicyAdministrationService` |
| FR-5 | Deterministic simulation with 18/19, 12/13, GPA and earned-credit explanations | AC-3, NFR-2 evidence, ADM-05 simulation |
| FR-6 | Immutable published/superseded versions and server-controlled lifecycles | domain tests, mapping tests, AC-4 |
| FR-7 | Admin plus `CataloguePolicy.Manage` and antiforgery | authorization tests and 14 endpoint contracts |
| FR-8 | Expected version and actor/scope/content/dependency/expiry-bound preview | `PublicationConfirmationService`, AC-5/AC-6 |
| FR-9 | Scope lock, revalidation, publication and audit atomicity | race tests, EC-5, NFR-3 evidence |
| FR-10 | Payload-bound replay and mismatch rejection | publication race tests and endpoint 05/08/10/14 contracts |

## Non-functional requirements

| ID | Gate | Evidence |
|---|---|---|
| NFR-1 | 10,000 rows within 30 seconds | `NFR-1EvidenceTests.cs`; `SPEC-009-NFR-1.md` |
| NFR-2 | Same version/input deterministic | `NFR-2EvidenceTests.cs`; `SPEC-009-NFR-2.md` |
| NFR-3 | Transactional publication | `NFR-3EvidenceTests.cs`; `SPEC-009-NFR-3.md` |
| NFR-4 | Actor, reason, source, timestamp and provenance | `NFR-4EvidenceTests.cs`; `SPEC-009-NFR-4.md` |

## Acceptance criteria

| ID | Delivered outcome | Evidence |
|---|---|---|
| AC-1 | Missing prerequisite blocks all publication | `AC-1Tests.cs` |
| AC-2 | Cycle path is complete and actionable | `AC-2Tests.cs`, ADM-05 cycle journey |
| AC-3 | DS413 and credit boundaries include explanations/source | `AC-3Tests.cs`, NFR-2 |
| AC-4 | Authorized immutable publication; unauthorized denied | `AC-4Tests.cs`, endpoint authorization |
| AC-5 | Concurrent confirmation has one winner | `AC-5Tests.cs`, NFR-3 |
| AC-6 | Edit invalidates preview; rejection replay is stable | `AC-6Tests.cs` |
| AC-7 | All four feature quality gates pass | four NFR suites and records |

## Edge cases

| ID | Delivered behavior | Evidence |
|---|---|---|
| EC-1 | Case/spacing duplicate code normalizes and rejects | `EdgeCases/EC-1Tests.cs` |
| EC-2 | Historical reference deactivates/supersedes, never deletes | `EdgeCases/EC-2Tests.cs` |
| EC-3 | Concurrent policy publication returns stable stale outcome | `EdgeCases/EC-3Tests.cs` |
| EC-4 | Unknown typed rule is rejected | `EdgeCases/EC-4Tests.cs` |
| EC-5 | Audit fault rolls back publication | `EdgeCases/EC-5Tests.cs`, NFR-3 |

## Success criteria

| ID | Delivered measure | Evidence |
|---|---|---|
| SC-1 | Invalid or cyclic relationships cannot publish | `SC-1OutcomeTests.cs` |
| SC-2 | Published catalogue/policy versions are immutable and auditable | `SC-2OutcomeTests.cs` |
| SC-3 | Admin can preview and explain all validation failures | `SC-3OutcomeTests.cs`, ADM-05 |

## Fourteen endpoint contracts

| # | Route | Test and handler |
|---:|---|---|
| 01 | `GET /api/admin/programs` | `Endpoint01ContractTests.cs`; `Spec009Endpoints.cs` |
| 02 | `GET /api/admin/catalogue/versions` | `Endpoint02ContractTests.cs`; `Spec009Endpoints.cs` |
| 03 | `GET /api/admin/catalogue/drafts/{draftId}` | `Endpoint03ContractTests.cs`; `Spec009Endpoints.cs` |
| 04 | `PUT /api/admin/catalogue/drafts/{draftId}` | `Endpoint04ContractTests.cs`; `Spec009Endpoints.cs` |
| 05 | `POST /api/admin/catalogue/imports` | `Endpoint05ContractTests.cs`; `Spec009Endpoints.cs` |
| 06 | `GET /api/admin/catalogue/imports/{importId}` | `Endpoint06ContractTests.cs`; `Spec009Endpoints.cs` |
| 07 | `POST /api/admin/catalogue/imports/{importId}/validate` | `Endpoint07ContractTests.cs`; `Spec009Endpoints.cs` |
| 08 | `POST /api/admin/catalogue/imports/{importId}/publish` | `Endpoint08ContractTests.cs`; `Spec009Endpoints.cs` |
| 09 | `GET /api/admin/policies` | `Endpoint09ContractTests.cs`; `Spec009Endpoints.cs` |
| 10 | `POST /api/admin/policies` | `Endpoint10ContractTests.cs`; `Spec009Endpoints.cs` |
| 11 | `PUT /api/admin/policies/{policySetId}` | `Endpoint11ContractTests.cs`; `Spec009Endpoints.cs` |
| 12 | `POST /api/admin/policies/{policySetId}/validate` | `Endpoint12ContractTests.cs`; `Spec009Endpoints.cs` |
| 13 | `POST /api/admin/policies/{policySetId}/simulate` | `Endpoint13ContractTests.cs`; `Spec009Endpoints.cs` |
| 14 | `POST /api/admin/policies/{policySetId}/publish` | `Endpoint14ContractTests.cs`; `Spec009Endpoints.cs` |

## Entity and persistence ownership

| Entity | Delivered source and verification |
|---|---|
| `Program` | `Domain/Program.cs`; model and SQL mapping tests |
| `Course` | `Domain/Course.cs`; model, provenance and credit tests |
| `CurriculumCourse` | `Domain/CurriculumCourse.cs`; same-version FK tests |
| `CoursePrerequisite` | `Domain/CoursePrerequisite.cs`; graph/cycle tests |
| `PolicySet` | `Domain/PolicySet.cs`; lifecycle/concurrency tests |
| `PolicyRule` | `Domain/PolicyRule.cs`; typed value/source tests |
| `ImportBatch` | `Domain/ImportBatch.cs`; error/state/rowversion tests |
| `CatalogueDraft` | `Domain/CatalogueDraft.cs`; editable content/version tests |
| `CatalogueVersion` | `Domain/CatalogueVersion.cs`; immutable history tests |

`CatalogueModelConfiguration.cs` contributes the nine tables, constraints,
indexes, rowversions, provenance ownership and same-version graph keys to the
shared SPEC-004 DbContext. Real SQL materialization is proven without creating
a second migration. S2CatalogueScheduling remains SPEC-010-owned.

## Frontend

| Route | Delivered surface | Evidence |
|---|---|---|
| ADM-05 `/admin/catalogue` | `CatalogueAdministrationPage.razor`, isolated CSS and `CatalogueApiClient` | eight real-browser SPEC-009 journeys plus client contract/unit/accessibility regression |

The page keeps provenance attached to values, distinguishes validation,
simulation and publication, blocks cyclic publication, sends antiforgery on
mutations, and requires refresh/revalidation after stale confirmation.

**Result: PASS.**
