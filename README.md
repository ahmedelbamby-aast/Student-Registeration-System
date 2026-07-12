# Student Registration System

The 18 feature packages have completed automated Spec Kit planning gates. See
[the specification index](specs/README.md) and
[the readiness audit](docs/SPECKIT_AUDIT.md). Human approval remains pending;
no implementation has started.

Human choices that must be resolved before affected specs can be approved are
tracked in [open decisions](docs/OPEN_DECISIONS.md).

Planning repository for an AASTMT College of Artificial Intelligence student
registration web application.

> Status: planning baseline in review. No application code should be created
> until the relevant specification is approved.

## Outcome

The proposed system is a modular monolith built with .NET 10 LTS, ASP.NET Core,
Blazor WebAssembly, EF Core 10, LINQ, SQL Server, and Code First migrations.
It has separate student and staff sign-in experiences, server-authoritative
academic rules, explainable subject eligibility, conflict-safe schedule
building, and atomic capacity allocation.

The design deliberately keeps the first release small:

- One ASP.NET Core host, one Blazor WebAssembly client, and one SQL Server.
- Business modules with narrow boundaries, not microservices.
- Explicit C# policy rules plus effective-dated configuration, not a generic
  rules language.
- A deterministic per-student schedule search over already-published groups,
  not an institution-wide timetable generator.
- SQL transactions, constraints, idempotency, and conditional updates for
  seat safety.

## Start here

- [Project constitution](docs/PROJECT_CONSTITUTION.md)
- [Project plan](docs/PROJECT_PLAN.md)
- [Screen storyboard](docs/STORYBOARD.md)
- [Architecture and engineering principles](docs/ARCHITECTURE.md)
- [AASTMT policy research](docs/POLICY_RESEARCH.md)
- [ERD](docs/diagrams/ERD.md)
- [Class diagram](docs/diagrams/CLASS_DIAGRAM.md)
- [Development environment](docs/DEVELOPMENT_ENVIRONMENT.md)
- [Validation evidence](docs/VALIDATION.md)
- [Specification index](specs/README.md)
- [Requirement traceability](docs/TRACEABILITY.md)

## Delivery baseline

- 18 specifications
- 27 route-level screen templates
- 9 two-week sprints, including Sprint 0 discovery/design
- Gate A: approve product, policy, UX, architecture, ERD, and class contracts
- Gate B: secure walking skeleton and publishable master data
- Gate C: end-to-end registration beta with proven seat safety
- Gate D: UAT, security, accessibility, load, recovery, and release approval

## Approval required before implementation

The Product Owner, AASTMT Registrar/Policy SME, UX Lead, Technical Lead, Data
Lead, QA Lead, Security Reviewer, and DevOps owner should review the specs
assigned to them. In particular, the College/Deanery must confirm policy
questions marked POLICY-Q before production rules are implemented.

## Repository state

The repository intentionally contains planning, specifications, diagrams, and
ADRs only. Source and test projects are created in Sprint 1 after Gate A.
