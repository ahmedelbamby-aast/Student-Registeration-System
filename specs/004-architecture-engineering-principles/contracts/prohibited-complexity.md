# Prohibited Complexity Contract

```yaml
schemaVersion: 1.0
ownerSpec: SPEC-004
approvalVersion: Gate-A-2026-07-13
```

The demo starts as one modular monolith and one SQL Server database. The items
below are rejected from the approved MVP unless measured evidence establishes a
need and Ahmed ELbamby approves a separately approved ADR/spec with updated
architecture tests.

| Rejected item | Current reason | Evidence that may reopen review |
|---|---|---|
| generic repository or unit-of-work wrapper | Hides EF Core LINQ, tracking, and transaction behavior without adding a boundary | A second persistence technology with a proven shared use case |
| microservices or independently deployed modules | Adds network failure, consistency, and operating cost before independent deployment is required | A measured independent deployment or ownership requirement |
| message broker | No durable external consumer exists in MVP | The first approved durable external consumer plus delivery/idempotency evidence |
| full CQRS or event sourcing | Current audit/history requirements are met by append-only records and normal transactions | A separately approved replay or temporal-model requirement |
| dynamic rule DSL or user-authored script | Creates validation, security, and debugging complexity; typed rules are sufficient | Institutionally approved authoring and sandbox requirements |
| institution-wide solver platform | Student scheduling needs only bounded combinations of published groups | Benchmarks proving the bounded optimizer cannot meet its approved budget |
| Kubernetes, service mesh, or distributed lock | Two stateless instances can use simpler hosting and SQL is the scarce-seat ordering point | Measured operational scale that simpler hosting cannot meet |
| separate read/write databases or distributed transactions | One database preserves the required atomic registration boundary | Approved extraction with a replacement consistency design |

## Review rule

A proposal is rejected when it adds one of these components without its listed
evidence. The proposal may instead move to a separately approved ADR/spec; it
does not enter the current implementation merely because it is a common
industry pattern. Removal from this list requires the decision record,
dependency/deployment changes, measurable evidence, and corresponding
architecture-test updates in the same review.
