# Specification Index

There are exactly 18 specifications. All start as In Review. Implementation is
blocked until the accountable owner and required reviewers change a spec to
Approved.

| ID | File | Owner | Reviewers |
|---|---|---|---|
| 001 | [Product Charter and RBAC](SPEC-001-product-charter-rbac.md) | Product Owner | All leads |
| 002 | [AASTMT Policy Rulebook](SPEC-002-aastmt-policy-rulebook.md) | Registrar/Policy SME | PO, Data, QA |
| 003 | [UX Storyboard and Accessibility](SPEC-003-ux-storyboard-accessibility.md) | UX Lead | PO, QA, role representatives |
| 004 | [Architecture and Principles](SPEC-004-architecture-engineering-principles.md) | Architect | Backend, Security, DevOps |
| 005 | [ERD and Data Lifecycle](SPEC-005-erd-data-lifecycle.md) | Data Lead | Architect, QA, Security |
| 006 | [Domain Classes and APIs](SPEC-006-domain-class-api-contracts.md) | Technical Lead | Frontend, Backend, QA |
| 007 | [Identity and Account Lifecycle](SPEC-007-identity-account-lifecycle.md) | Security Lead | PO, Backend, QA |
| 008 | [Academic Term and Student Profile](SPEC-008-academic-term-student-profile.md) | Backend Lead | Registrar, QA |
| 009 | [Catalogue, Prerequisites, Policy Admin](SPEC-009-catalog-prerequisites-policy-admin.md) | Policy SME + Backend | Admin, QA |
| 010 | [Offerings, Groups, Resources](SPEC-010-offerings-groups-resources.md) | Backend Lead | Admin, Lecturer/TA, QA |
| 011 | [Eligibility and Discovery](SPEC-011-eligibility-subject-discovery.md) | Product Owner | Registrar, UX, QA |
| 012 | [Schedule Builder and Conflicts](SPEC-012-schedule-builder-conflicts.md) | Technical Lead | UX, Registrar, QA |
| 013 | [Schedule Recommendations](SPEC-013-schedule-recommendations.md) | Technical Lead | PO, QA, Performance |
| 014 | [Registration Capacity and Concurrency](SPEC-014-registration-capacity-concurrency.md) | Data/Backend Lead | Security, QA, DevOps |
| 015 | [Student Registration Records](SPEC-015-student-registration-records.md) | Product Owner | Registrar, UX, QA |
| 016 | [Lecturer and TA Workspace](SPEC-016-lecturer-ta-workspace.md) | Product Owner | Lecturer/TA reps, Security, QA |
| 017 | [Admin Operations, Audit, Reporting](SPEC-017-admin-operations-audit-reporting.md) | Product Owner | Admin, Security, DevOps |
| 018 | [Quality, Security, Scalability, Operations](SPEC-018-quality-security-scalability-operations.md) | QA/DevOps/Security | All leads |

## Mandatory structure

Every spec includes:

1. Title and metadata
2. Context
3. Functional requirements using RFC 2119 terms
4. Measurable non-functional requirements
5. Given/When/Then acceptance criteria
6. Edge cases
7. API contracts
8. Data models
9. Explicit out of scope

## Status lifecycle

Draft -> In Review -> Approved -> In Development -> Verification -> Released
-> Superseded.

No code is written for an unapproved spec. If implementation exposes a missing
or changed requirement, update and approve the spec before changing behavior.
