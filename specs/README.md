# Specification Index

All 18 features have completed the Spec Kit planning workflow through consistency analysis. No implementation has started. Detailed original requirements are preserved in each feature's requirements.md.

| Spec | Feature package | Owner | Dependencies | Gate status |
|---|---|---|---|---|
| SPEC-001 | [Product Charter and RBAC](001-product-charter-rbac/spec.md) | Product Owner | None | Automated PASS; human approval pending |
| SPEC-002 | [AASTMT Policy Rulebook](002-aastmt-policy-rulebook/spec.md) | Registrar/Policy SME | SPEC-001 | Automated PASS; human approval pending |
| SPEC-003 | [UX Storyboard and Accessibility](003-ux-storyboard-accessibility/spec.md) | UX Lead | SPEC-001, SPEC-002 | Automated PASS; human approval pending |
| SPEC-004 | [Architecture and Engineering Principles](004-architecture-engineering-principles/spec.md) | Technical Lead/Architect | SPEC-001, SPEC-003 | Automated PASS; human approval pending |
| SPEC-005 | [ERD and Data Lifecycle](005-erd-data-lifecycle/spec.md) | Data/Backend Lead | SPEC-002, SPEC-004 | Automated PASS; human approval pending |
| SPEC-006 | [Domain Classes and API Contracts](006-domain-class-api-contracts/spec.md) | Technical Lead | SPEC-004, SPEC-005 | Automated PASS; human approval pending |
| SPEC-018 | [Quality Security Scalability and Operations](018-quality-security-scalability-operations/spec.md) | QA/DevOps/Security Leads | SPEC-001, SPEC-004, SPEC-005, SPEC-006 | Automated PASS; human approval pending |
| SPEC-007 | [Identity and Account Lifecycle](007-identity-account-lifecycle/spec.md) | Security Lead | SPEC-004, SPEC-005, SPEC-006, SPEC-018 | Automated PASS; human approval pending |
| SPEC-008 | [Academic Term and Student Profile](008-academic-term-student-profile/spec.md) | Backend Lead | SPEC-002, SPEC-005, SPEC-007, SPEC-018 | Automated PASS; human approval pending |
| SPEC-009 | [Catalogue Prerequisites and Policy Administration](009-catalog-prerequisites-policy-admin/spec.md) | Registrar/Policy SME and Backend Lead | SPEC-002, SPEC-005, SPEC-006, SPEC-008, SPEC-018 | Automated PASS; human approval pending |
| SPEC-010 | [Offerings Groups and Resources](010-offerings-groups-resources/spec.md) | Backend Lead | SPEC-005, SPEC-006, SPEC-009, SPEC-018 | Automated PASS; human approval pending |
| SPEC-011 | [Eligibility and Subject Discovery](011-eligibility-subject-discovery/spec.md) | Product Owner | SPEC-002, SPEC-008, SPEC-009, SPEC-010, SPEC-018 | Automated PASS; human approval pending |
| SPEC-012 | [Schedule Builder and Conflicts](012-schedule-builder-conflicts/spec.md) | Technical Lead | SPEC-003, SPEC-010, SPEC-011, SPEC-018 | Automated PASS; human approval pending |
| SPEC-013 | [Schedule Recommendations](013-schedule-recommendations/spec.md) | Technical Lead | SPEC-012, SPEC-018 | Automated PASS; human approval pending |
| SPEC-014 | [Registration Capacity and Concurrency](014-registration-capacity-concurrency/spec.md) | Data/Backend Lead | SPEC-007, SPEC-010, SPEC-011, SPEC-012, SPEC-013, SPEC-018 | Automated PASS; human approval pending |
| SPEC-015 | [Student Registration Records](015-student-registration-records/spec.md) | Product Owner | SPEC-008, SPEC-014, SPEC-018 | Automated PASS; human approval pending |
| SPEC-016 | [Lecturer and Teaching Assistant Workspace](016-lecturer-ta-workspace/spec.md) | Product Owner | SPEC-007, SPEC-010, SPEC-015, SPEC-018 | Automated PASS; human approval pending |
| SPEC-017 | [Admin Operations Audit and Reporting](017-admin-operations-audit-reporting/spec.md) | Product Owner | SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-014, SPEC-015, SPEC-016, SPEC-018 | Automated PASS; human approval pending |

Run .specify/scripts/powershell/Test-AllSpecs.ps1 to reproduce the gates.
