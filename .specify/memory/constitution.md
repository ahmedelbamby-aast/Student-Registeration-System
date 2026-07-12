<!--
Sync Impact Report
- Version: template -> 1.0.0
- Ratified: 2026-07-12
- Last amended: 2026-07-12
- Templates requiring alignment: plan-template.md, spec-template.md, tasks-template.md
- Source of truth: docs/PROJECT_CONSTITUTION.md
-->
# Student Registration System Constitution

## Core Principles

### I. Ahmed ELbamby Owns Every Repository Contribution
Every Git commit MUST use author and committer `Ahmed ELbamby <A.Elbamby61869@student.aast.edu>`. Commit messages, trailers, repository documents, and generated metadata MUST NOT name Codex, an AI assistant, or another automated system as author, committer, co-author, or contributor. This identity gate is mandatory before every commit.

### II. Specification First and Traceable Delivery
Every feature MUST have an approved specification before implementation. Requirements MUST use stable identifiers; acceptance scenarios and planned tasks MUST trace back to those identifiers. Clarification, planning, checklist, task, and consistency-analysis gates MUST pass before implementation begins. This repository remains planning-only until the appropriate human approval changes a specification from In Review to Approved.

### III. Simple Modular Architecture
The system MUST begin as a modular monolith with clear module boundaries, explicit interfaces, dependency inversion at external boundaries, and a single deployable server. SOLID, separation of concerns, DRY, KISS, and YAGNI apply in proportion to demonstrated need. New services MUST be addable through module contracts without rewriting unrelated modules; distributed infrastructure MUST NOT be introduced without measured need.

### IV. Server-Authoritative Academic Correctness
Authentication, authorization, policy eligibility, prerequisites, schedule conflicts, capacity, registration submission, and academic-term state MUST be validated on the server. Registration writes MUST be atomic and concurrency-safe; capacity MUST never become negative or exceed its configured limit. Client calculations are advisory only and MUST be revalidated during submission.

### V. Secure, Accessible, and Usable by Default
Role-based least privilege, secure password handling, auditability, input validation, and privacy-safe logs are required. Student and staff journeys MUST meet WCAG 2.2 AA targets, support keyboard navigation, provide clear validation and recovery messages, and avoid relying on color alone. The registration workflow MUST remain understandable under time pressure.

### VI. Quality, Scalability, and Operability Are Features
Each requirement MUST have proportionate automated verification planned before implementation. The design MUST support horizontal API scaling through stateless requests, indexed and paginated queries, bounded payloads, optimistic or database-enforced concurrency, and measurable service-level objectives. Structured logging, health checks, metrics, and trace correlation MUST be planned for critical flows.

## Technology and Design Constraints

- Target stack: .NET 10, ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ, and SQL Server using Code First migrations.
- The Blazor client MUST NOT connect directly to SQL Server; all durable access crosses authenticated server APIs.
- Identity, registration, academics, scheduling, policy, staff administration, and audit concerns MUST have explicit module ownership.
- External AASTMT policy statements MUST retain provenance and approval state. Unverified policy values MUST fail closed or remain configurable; they MUST NOT be silently presented as authoritative.
- Current date and academic term MUST come from server-controlled time and term configuration, not a browser clock.
- Planning artifacts MAY be created now. Application source code, database migrations, deployment resources, and implementation tests MUST NOT be created until the relevant approval gate passes.

## Development Workflow and Gates

For every feature, the required order is: constitution check, specification, clarification, plan, research, data model and contracts, requirement checklist, tasks, consistency analysis, human approval, then implementation. Each feature MUST declare dependencies, assumptions, out-of-scope boundaries, measurable success criteria, and requirement-to-acceptance-to-task traceability.

A feature passes automated readiness only when all required artifacts exist, no unresolved clarification marker remains, every requirement is traceable, dependency references are valid and acyclic, and checklists contain no unresolved planning item. Automated readiness does not imply human approval. Cross-cutting specs apply to dependent features through explicit links, not copied and divergent rules.

## Governance

This constitution governs all repository work. Amendments require a documented rationale, impact review across templates and specifications, semantic version increment, and Ahmed ELbamby's approval. A compliance review is required during planning analysis and before every merge. Exceptions MUST be explicit, time-bounded, owned, and recorded; convenience alone is not an exception.

**Version**: 1.0.0 | **Ratified**: 2026-07-12 | **Last Amended**: 2026-07-12
