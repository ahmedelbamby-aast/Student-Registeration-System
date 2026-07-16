# SPEC-009 Scope Review

**Review date:** 2026-07-16  
**Decision authority:** Ahmed ELbamby  
**Boundary:** Gate A non-production demo

## Verified exclusions

| ID | Verified exclusion | Delivered-source result |
|---|---|---|
| OS-1 | No live scraping | `CataloguePublicationService` uses the frozen curated snapshot and contains no HTTP client, crawler, browser automation, or runtime fetch. |
| OS-1 | No complete or official AASTMT curriculum claim | The page and evidence call the data a curated subset and keep locally assigned credit/status values explicitly synthetic. |
| OS-2 | No advisor override | The typed rule allow-list contains no advisor, overload, waiver, or override rule. |
| OS-2 | No exception workflow | Policy administration accepts fixed typed values, never arbitrary expressions, scripts, or exception routing. |
| OS-3 | No silent auto-correction | Missing references, duplicates, invalid credits, unknown rule types, and cycles remain stable validation errors. The source is not rewritten to make invalid data publishable. |
| OS-4 | No historical deletion | Published catalogue and policy records are immutable and superseded; referenced records are deactivated rather than deleted. |

## Architecture and ownership boundary

The implementation remains one modular monolith:

```text
Client -> Contracts -> Academics endpoints
Academics endpoints -> Academics application/domain
SQL infrastructure -> Academics mapping contribution -> shared DbContext
```

There is no second DbContext, generic repository, event bus, message broker,
distributed transaction, microservice, live catalogue connector, or policy
script engine. `CatalogueModelConfiguration.cs` is the SPEC-009 persistence
contribution. The combined `S2CatalogueScheduling` migration remains owned by
SPEC-010.

## Authority boundary

This review approves only the synthetic Development and Testing demo slice. It
does not authorize a production catalogue source, official curriculum
completeness, institutional policy publication, SIS integration, exception
workflow, production migration execution, or AASTMT go-live.

**Result: PASS.**
