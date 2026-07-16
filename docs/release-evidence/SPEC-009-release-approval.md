# SPEC-009 Non-Production Demo Release Approval

**Decision:** APPROVED for the SPEC-009 Development and Testing demo slice  
**Decision date:** 2026-07-16  
**Sole decision authority and developer:** Ahmed ELbamby  
**Governance boundary:** Gate A non-production design-capability demo only

Ahmed's perspectives below are project reviews, not separate people or
institutional signatories. This record is not institutional approval and does
not authorize production or official AASTMT publication.

## Review perspectives

| Perspective | Decision and evidence | Residual boundary |
|---|---|---|
| Registrar / policy SME | APPROVED for the curated 19-course subset, explicit official/synthetic provenance, DS413 evidence, and 18/12-credit demo boundaries. | No complete official curriculum, production Registrar authority, advisor override, waiver, or exception workflow. |
| Product | APPROVED for the traced draft, import, validation, simulation, immutable publication, and stale/replay journeys. | Offerings, real registration and broader admin operations remain later-spec work. |
| Admin user | APPROVED for ADM-05’s bounded version view, attached provenance, import form, typed policy fields, cycle links, simulation and confirmation dialog. | SPEC-017 audit/report contributions remain outside this page owner slice. |
| Data / concurrency | APPROVED for nine owned mappings, same-version graph keys, normalized uniqueness, rowversion, immutable history and one-winner/fault evidence. | The combined `S2CatalogueScheduling` migration remains SPEC-010-owned. |
| QA | APPROVED for the executable model, acceptance, contract, SQL, browser and four NFR evidence suites with no required failures. | Evidence is source-sensitive and must be rerun after relevant drift. |
| Security | APPROVED for Admin-plus-`CataloguePolicy.Manage`, antiforgery, bounded input, DTO isolation, safe errors, actor-bound previews and deterministic stale/idempotency outcomes. | No penetration-test sign-off, production secret custody, or institutional security authorization is granted. |
| Accessibility | APPROVED for semantic headings/tables/lists, focusable validation summary, live regions, native controls, modal focus restoration and responsive one-column fallback. | The two existing SPEC-018 manual/runtime accessibility checks remain separate product-wide gates; no institution-wide certification is claimed. |
| Operations | APPROVED for isolated Development/Testing execution, pinned real-SQL mapping proof, deterministic fixtures and modular-monolith scope. | Production topology, external network/TLS, backup/restore, monitoring, support and capacity approval remain outside Gate A. |

## Evidence admitted

- [Traceability](SPEC-009-traceability.md)
- [Scope review](SPEC-009-scope-review.md)
- [NFR-1](SPEC-009-NFR-1.md)
- [NFR-2](SPEC-009-NFR-2.md)
- [NFR-3](SPEC-009-NFR-3.md)
- [NFR-4](SPEC-009-NFR-4.md)
- SPEC-009 endpoint, acceptance, integration, real-SQL, authorization and
  browser suites

## Explicit non-approvals

- Gate B, Gate C and Gate D are not closed by this record.
- Production catalogue/policy source and production migration execution are
  not approved.
- `S2CatalogueScheduling` remains SPEC-010-owned.
- Live scraping, complete/current official AASTMT curriculum claims, arbitrary
  policy scripting, advisor/waiver/exception workflows and historical deletion
  remain excluded.
- This is not legal, privacy, institutional Registrar, executive, security or
  official AASTMT go-live approval.

Ahmed Elbamby approves only the evidenced synthetic non-production SPEC-009
demo slice under the standing Gate A authority.

**Result: APPROVED — NON-PRODUCTION SPEC-009 DEMO ONLY.**
