# ADR-001: Modular Monolith

- Status: Accepted for non-production demo (Gate A)
- Date: 2026-07-12
- Accepted: 2026-07-13
- Approval: Ahmed ELbamby (Technical Lead/Architect review perspective)
- Decision perspectives: Technical Lead, Data Lead, DevOps, Security
- Production authorization: Not granted

## Context

Registration requires strong consistency across policy revalidation, group
capacity, enrollment, and audit. The team also needs low operational
complexity and future extension seams.

## Decision

Build one ASP.NET Core/Blazor WASM application and one SQL Server, organized
into IdentityAccess, Academics, Scheduling, Registration, and
StaffAdministration modules with narrow dependencies and SQL schema ownership.

## Consequences

Positive:

- One ACID transaction protects the scarce seat invariant.
- Simple deployment, debugging, migrations, and local development.
- Module boundaries can later become service seams when evidence justifies it.

Accepted costs:

- Modules cannot deploy independently.
- A single database needs disciplined ownership and architecture tests.

## Rejected alternatives

- Microservices: distributed transactions and operations cost are unjustified.
- Unstructured monolith: fast initially but weak future change isolation.
- Full CQRS/event sourcing: exceeds current audit and scale needs.

## Revisit trigger

Independent deployment demand, optimizer-specific scaling, or measured module
contention that cannot be solved with indexing/caching within approved SLOs.
