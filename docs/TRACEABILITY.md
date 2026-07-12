# Requirement Traceability

| User requirement | Primary specification(s) | Verification |
|---|---|---|
| Storyboard/frontend pages | SPEC-003 | 27 Page Design Records, design system, component/contract/E2E/visual/accessibility tests |
| ERD | SPEC-005 | Model review, migration and constraint tests |
| Class diagram | SPEC-006 | Architecture and dependency tests |
| Engineering principles | SPEC-004, SPEC-018 | ADR and architecture-test review |
| Simple scalable implementation | SPEC-004, SPEC-018 | load, deployment, and boundary tests |
| Modular without overengineering | SPEC-004 | rejected-complexity list and dependency rules |
| .NET Core / Blazor WASM | SPEC-004, SPEC-006 | build and E2E test |
| Code First SQL Server | SPEC-005 | migrations on real SQL Server |
| EF Core and LINQ | SPEC-005, SPEC-006 | integration tests and query review |
| Student login/register | SPEC-007 | identity acceptance tests |
| Shared Admin/Lecturer/TA login | SPEC-007 | role-routing and authorization tests |
| University ID + password | SPEC-007 | activation and login tests |
| Current date and academic term | SPEC-008 | fake-clock and term-boundary tests |
| User/load handling | SPEC-018 | target, 2x, and spike load tests |
| GPA/policy-based availability | SPEC-002, SPEC-009, SPEC-011 | rule-unit and decision-explanation tests |
| Staff/location/time shown | SPEC-010, SPEC-011 | API and UI tests |
| Multiple groups/capacity | SPEC-010, SPEC-014 | database and collision tests |
| Conflict detection and best timetable | SPEC-012, SPEC-013 | deterministic constraint tests |
| Red X and blocked unresolved conflict | SPEC-003, SPEC-012 | accessible E2E test |
| Race-condition safety | SPEC-003, SPEC-005, SPEC-006, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-012, SPEC-013, SPEC-014, SPEC-016, SPEC-017, SPEC-018 | duplicate-action UI, identity single-use, constraints, shared lock boundaries, idempotency, two-replica SQL concurrency/load/fault tests |
| Easy future services/features | SPEC-004 | module dependency and ADR review |
| Scalability | SPEC-018 | measurable performance/recovery gates |
| Agile phased development | SPEC-001, project plan | sprint reviews and spec status matrix |

## Traceability rule

Every implementation pull request must include:

- At least one SPEC-NNN/FR-N reference.
- Every affected SPEC-NNN/FR-N and SPEC-NNN/NFR-N reference.
- The SPEC-NNN/AC-N and SPEC-NNN/EC-N tests that prove the change.
- The exact upstream SPEC dependency/contract version consumed.
- The owned frontend route ID and page/functional/accessibility/visual evidence
  when a route is affected.
- The relevant ADR for a changed architecture decision.
- Migration, UI evidence, observability, and policy-approval notes when
  applicable.
