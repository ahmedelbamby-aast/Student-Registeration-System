# SPEC-010 Complete Traceability Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T18:00:00Z  
**Boundary:** Approved Gate A non-production demo

This matrix traces the complete approved Spec 010 inventory to its owning
implementation and primary executable evidence. It does not convert
downstream contributor ownership into a Spec 010 completion claim.

## Functional requirements

| ID | Delivered implementation | Primary executable evidence |
|---|---|---|
| FR-1 | `OfferingService` creates term/course offerings with initial groups. | `OfferingLifecycleTests.cs`; AC-1 |
| FR-2 | Complete Lecture/Lecturer plus Tutorial-or-Laboratory/TA bundle and student detail projection. | `OfferingLifecycleTests.cs`; AC-1; SC-2 |
| FR-3 | `OfferingPublicationValidator` checks room/staff conflicts, availability, capacity, slots, bundle, and dependency versions. | `ResourcePublicationRaceTests.cs`; AC-2; EC-1 |
| FR-4 | `SectionGroupCapacityService` authors selectable/full/lifecycle results. | `GroupCapacityRaceTests.cs`; AC-3 |
| FR-5 | Capacity mutations preserve capacity at or above enrolled count. | `GroupCapacityRaceTests.cs`; AC-4; SC-3 |
| FR-6 | `ResourceAvailabilityService` preserves staff ownership, read-only Admin planning input, audited resource changes, and impact alerts. | `ResourceAvailabilityTests.cs`; AC-9 |
| FR-7 | `OfferingPublicationService` validates and publishes inside one transaction with audit. | `OfferingPublicationTransactionTests.cs`; AC-4 |
| FR-8 | Publication locks offering/groups/rooms/staff-term roots in stable order and revalidates current versions. | `ResourcePublicationRaceTests.cs`; AC-5; EC-5 |
| FR-9 | Capacity edit and allocation share the SectionGroup serialized boundary. | `GroupCapacityRaceTests.cs`; AC-6; EC-2 |
| FR-10 | Child schedule/resource changes advance the owning group or staff-term concurrency root. | AC-8; scheduling mapping/migration tests |

## Non-functional requirements

| ID | Gate | Evidence |
|---|---|---|
| NFR-1 | Offering/group read p95 at or below 300 ms at the bounded component sample; SPEC-018 mixed load remains separate. | `NFR-1EvidenceTests.cs`; `SPEC-010-NFR-1.md` |
| NFR-2 | Stable actionable publication reason codes. | `NFR-2EvidenceTests.cs`; `SPEC-010-NFR-2.md` |
| NFR-3 | Term-timezone authority with unambiguous local meeting day/time. | `NFR-3EvidenceTests.cs`; `SPEC-010-NFR-3.md` |
| NFR-4 | Bounded, paged, filtered Admin offering list. | `NFR-4EvidenceTests.cs`; `SPEC-010-NFR-4.md` |

## Acceptance criteria

| ID | Delivered outcome | Evidence |
|---|---|---|
| AC-1 | Valid complete group publishes and exposes both activities. | `AC-1Tests.cs` |
| AC-2 | Overlapping room use blocks publication with conflict detail. | `AC-2Tests.cs` |
| AC-3 | Full group is nonselectable. | `AC-3Tests.cs` |
| AC-4 | Publish/capacity changes are transactional, versioned, and audited. | `AC-4Tests.cs` |
| AC-5 | Concurrent overlapping publication has one winner and one resource conflict. | `AC-5Tests.cs` |
| AC-6 | Capacity reduction/allocation serialize without invariant breach. | `AC-6Tests.cs` |
| AC-7 | Read latency, stable codes, timezone display, and bounded lists have focused gates. | `AC-7Tests.cs`; four NFR suites |
| AC-8 | Child mutation advances group version; availability/publication serialize. | `AC-8Tests.cs` |
| AC-9 | Staff owns availability; Admin view/import is read-only; changes create durable alerts. | `AC-9Tests.cs` |

## Edge cases

| ID | Delivered behavior | Evidence |
|---|---|---|
| EC-1 | One invalid slot blocks the complete multi-slot group. | `EdgeCases/EC-1Tests.cs` |
| EC-2 | Capacity/enrollment race preserves both capacity invariants. | `EdgeCases/EC-2Tests.cs` |
| EC-3 | Post-publication unavailability creates an impact alert without silent movement. | `EdgeCases/EC-3Tests.cs` |
| EC-4 | Overnight meeting slot is rejected for the MVP. | `EdgeCases/EC-4Tests.cs` |
| EC-5 | Multi-resource lock order is stable and deadlock retry reruns the complete transaction. | `EdgeCases/EC-5Tests.cs` |

## Success criteria

| ID | Delivered measure | Evidence |
|---|---|---|
| SC-1 | Only complete, conflict-free offerings become published/visible. | `SC-1OutcomeTests.cs` |
| SC-2 | Visible groups expose capacity and every activity's staff, room, day, and time. | `SC-2OutcomeTests.cs` |
| SC-3 | Capacity never falls below enrollment. | `SC-3OutcomeTests.cs` |

## Frontend routes

| Route | Ownership and contribution | Evidence |
|---|---|---|
| STU-03 | SPEC-010 supplies the subject-detail scheduling contract. SPEC-011 owns the STU-03 Razor page. | `contracts/routes/STU-03.md`; `SubjectDetailsPageContributorTests.cs` |
| ADM-06 | SPEC-010 owns `/admin/offerings` and its scheduling page behavior. | `OfferingAdministrationPage.razor`; `OfferingAdministrationPageFeatureTests.cs` |
| ADM-07 | SPEC-010 owns `/admin/resources`, including read-only availability use and impact-alert actions. | `ResourceAdministrationPage.razor`; `ResourceAdministrationPageFeatureTests.cs` |

SPEC-017 contributor work is downstream for broader Admin monitoring, audit,
reporting, and export concerns and is not claimed as completed by SPEC-010.

## Entities and persistence

| Entity | Owned source and verification |
|---|---|
| `CourseOffering` | `Domain/CourseOffering.cs`; model and scheduling mapping tests |
| `SectionGroup` | `Domain/SectionGroup.cs`; capacity/version model and SQL tests |
| `MeetingSlot` | `Domain/MeetingSlot.cs`; slot/range model and SQL tests |
| `Room` | `Domain/Room.cs`; resource/version model and SQL tests |
| `GroupStaffAssignment` | `Domain/GroupStaffAssignment.cs`; activity-role and unique-key tests |
| `StaffTermAvailability` | `Domain/StaffTermAvailability.cs`; aggregate/version tests |
| `StaffAvailability` | `Domain/StaffAvailability.cs`; child range tests |
| `ScheduleImpactAlert` | `Domain/ScheduleImpactAlert.cs`; state/version persistence tests |

`SchedulingModelConfiguration.cs` contributes these owned entities to the
shared SPEC-004 DbContext. The `S2CatalogueScheduling` migration combines the
approved SPEC-009 catalogue contribution and SPEC-010 scheduling contribution
in dependency order.

## Fourteen endpoints

| # | Route | Contract and handler evidence |
|---:|---|---|
| 01 | `GET /api/offerings/{offeringId}` | `Endpoint01ContractTests.cs`; `Spec010Endpoints.cs` |
| 02 | `GET /api/groups/{groupId}` | `Endpoint02ContractTests.cs`; `Spec010Endpoints.cs` |
| 03 | `GET /api/admin/offerings` | `Endpoint03ContractTests.cs`; `Spec010Endpoints.cs` |
| 04 | `POST /api/admin/offerings` | `Endpoint04ContractTests.cs`; `Spec010Endpoints.cs` |
| 05 | `PUT /api/admin/groups/{groupId}` | `Endpoint05ContractTests.cs`; `Spec010Endpoints.cs` |
| 06 | `POST /api/admin/offerings/{offeringId}/validate` | `Endpoint06ContractTests.cs`; `Spec010Endpoints.cs` |
| 07 | `POST /api/admin/offerings/{offeringId}/publish` | `Endpoint07ContractTests.cs`; `Spec010Endpoints.cs` |
| 08 | `GET /api/admin/rooms` | `Endpoint08ContractTests.cs`; `Spec010Endpoints.cs` |
| 09 | `POST /api/admin/rooms` | `Endpoint09ContractTests.cs`; `Spec010Endpoints.cs` |
| 10 | `PUT /api/admin/rooms/{roomId}` | `Endpoint10ContractTests.cs`; `Spec010Endpoints.cs` |
| 11 | `GET /api/admin/staff-availability` | `Endpoint11ContractTests.cs`; mutation-absence contract; `Spec010Endpoints.cs` |
| 12 | `GET /api/admin/schedule-impact-alerts` | `Endpoint12ContractTests.cs`; `Spec010Endpoints.cs` |
| 13 | `POST /api/admin/schedule-impact-alerts/{alertId}/revalidate` | `Endpoint13ContractTests.cs`; `Spec010Endpoints.cs` |
| 14 | `POST /api/admin/schedule-impact-alerts/{alertId}/resolve` | `Endpoint14ContractTests.cs`; `Spec010Endpoints.cs` |

**Result: PASS.**
