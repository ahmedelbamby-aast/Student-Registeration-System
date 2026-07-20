# Student Registration System

The 18 feature packages have completed automated Spec Kit planning gates. See
[the specification index](specs/README.md) and
[the readiness audit](docs/SPECKIT_AUDIT.md). Ahmed ELbamby approved Gate A for
all 18 non-production demo specifications on 13 July 2026; approved-slice
implementation may begin.

The completed demo choices and production-only boundaries are tracked in the
[decision register](docs/OPEN_DECISIONS.md).

Planning repository for an AASTMT College of Artificial Intelligence student
registration web application.

> Status: Gate A approved for demo implementation. Gate B-D and production
> deployment/release remain separate approvals.

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
- [Demo curriculum](docs/DEMO_CURRICULUM.md)
- [Brand asset provenance](docs/BRAND_ASSETS.md)
- [ERD](docs/diagrams/ERD.md)
- [Class diagram](docs/diagrams/CLASS_DIAGRAM.md)
- [Development environment](docs/DEVELOPMENT_ENVIRONMENT.md)
- [Validation evidence](docs/VALIDATION.md)
- [Specification index](specs/README.md)
- [Requirement traceability](docs/TRACEABILITY.md)

## Run the local demo

With .NET SDK `10.0.301`, Docker Desktop, and PowerShell 7 installed, one
command builds the solution, starts SQL Server, applies migrations, seeds the
synthetic Development database, and hosts the API plus Blazor client:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -TrustHttpsCertificate
```

See the [manual role testing guide](docs/MANUAL_ROLE_TESTING.md) for every
login and route. Press `Ctrl+C` to stop the web app, then run
`.\ops\scripts\Stop-LocalDemo.ps1` to stop SQL while preserving its volume.

## Delivery baseline

- 18 specifications
- 27 route-level screen templates
- 9 two-week sprints, including Sprint 0 discovery/design
- Gate A: product, policy, UX, architecture, ERD, and class contracts —
  **completed 13 July 2026**
- Gate B: secure walking skeleton and publishable master data
- Gate C: end-to-end registration beta with proven seat safety
- Gate D: UAT, security, accessibility, load, recovery, and release approval

## Approval boundary

Ahmed ELbamby completed the Product, Policy, UX, Architecture, Data, QA,
Security, and Operations review perspectives for the demo. Official AASTMT
production policy, real-data retention, hosting, Safari support, and go-live
remain outside this approval.

## Repository state

At this checkpoint the repository contains planning, specifications, diagrams,
and ADRs. Source and test projects are authorized to begin in Sprint 1 under
the approved dependency and test-first task order.
