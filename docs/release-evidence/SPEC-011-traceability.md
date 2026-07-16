# SPEC-011 Traceability Matrix

## Functional requirements

| ID | Primary implementation | Automated evidence |
|---|---|---|
| FR-1 | `EligibilityService` governed server evaluation | EligibilityDecisionTests, AC-1, AC-4 |
| FR-2 | `OfferingSearchQuery` default eligible/available filter | AC-1, AC-3 |
| FR-3 | `OfferingSearchQuery` allow-listed search and filters | OfferingSearchTests, AC-4 |
| FR-4 | `EligibilityReason` and unavailable presentation | AC-2, SC-1 |
| FR-5 | `OfferingEligibility` and `GroupSummary` | model/projection contract tests |
| FR-6 | governed reasons, provenance, values, support | EligibilityReasonModelTests, AC-2, EC-1 |
| FR-7 | server eligibility precedes filtering | EligibilityDecisionTests, SC-3 |
| FR-8 | bounded canonical page and stable ID tie-break | Endpoint 01 contract, OfferingSearchTests |

## Non-functional requirements

| ID | Release alias | Task and canonical evidence |
|---|---|---|
| NFR-1 | NFR-001 | T045 / `docs/release-evidence/SPEC-011-NFR-1.md` |
| NFR-2 | NFR-006 | T046 / `docs/release-evidence/SPEC-011-NFR-2.md` |
| NFR-3 | NFR-004 | T047 / `docs/release-evidence/SPEC-011-NFR-3.md` |
| NFR-4 | NFR-005 | T048 / `docs/release-evidence/SPEC-011-NFR-4.md` |

## Acceptance criteria

| ID | Automated evidence |
|---|---|
| AC-1 | `AC-1Tests.cs`, STU-02 browser primary journey |
| AC-2 | `AC-2Tests.cs`, reason model tests |
| AC-3 | `AC-3Tests.cs`, STU-03 full/stale browser journey |
| AC-4 | `AC-4Tests.cs`, Endpoint 01 and search tests |
| AC-5 | `AC-5Tests.cs`, T045-T048 quality tests |

## Edge cases

| ID | Automated evidence |
|---|---|
| EC-1 | `EdgeCases/EC-1Tests.cs` |
| EC-2 | `EdgeCases/EC-2Tests.cs`, STU-03 refresh journey |
| EC-3 | `EdgeCases/EC-3Tests.cs`, NFR-006 literal and SQL translation tests |
| EC-4 | `EdgeCases/EC-4Tests.cs`, STU-02 empty/reset journey |

## Success criteria

| ID | Automated evidence |
|---|---|
| SC-1 | `SC-1OutcomeTests.cs` |
| SC-2 | `SC-2OutcomeTests.cs`, NFR-001 |
| SC-3 | `SC-3OutcomeTests.cs` |

## Routes

| Route | Owner page | Evidence |
|---|---|---|
| STU-02 | `SubjectDiscoveryPage.razor` | SubjectDiscoveryPageFeatureTests |
| STU-03 | `SubjectDetailsPage.razor` | SubjectDetailsPageFeatureTests |

## Endpoints

| Endpoint | Route | Evidence |
|---|---|---|
| Endpoint 01 | `GET /api/student/terms/{termId}/offerings` | Endpoint01ContractTests |
| Endpoint 02 | `GET /api/student/offerings/{offeringId}/eligibility` | Endpoint02ContractTests |

## Owned projections

- `OfferingEligibility`
- `EligibilityReason`
- `GroupSummary`

## Release tasks

T045, T046, T047, T048, T049, T050, and T051 are represented by focused
quality tests and the canonical `docs/release-evidence/SPEC-011-*` records.
Task checkboxes remain governed by the parent evidence audit.

**Result: PASS — complete SPEC-011 inventory and evidence mapping.**
