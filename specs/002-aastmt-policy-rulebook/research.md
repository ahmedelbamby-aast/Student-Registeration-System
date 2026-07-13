# Research: AASTMT Policy Rulebook

## Decisions

### Modular boundary
**Decision**: Own this capability in the Academics module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface PolicyDecisionDto {
  eligible: boolean;
  policyVersion: string;
  evaluatedAtUtc: string;
  inputSummary: Record<string, string>;
  approvedBy: string;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  results: Array<{
    reasonCode: string;
    passed: boolean;
    explanation: string;
    sourceUrl: string;
    sourceAccessedOn: string;
    overridePossible: boolean;
  }>;
}
```

Endpoint ownership is intentionally downstream: SPEC-009 owns
`POST /api/admin/policies/{policySetId}/simulate`, and SPEC-011 owns
`GET /api/student/offerings/{offeringId}/eligibility`. This rulebook supplies
the decision/provenance shape but no handler.

### Accepted demo POC policy profile
**Decision**: Use the bounded `DEMO-POC-2026.1` profile approved by Ahmed
ELbamby on 2026-07-13:

- configured registration window, eligible standing, no blocking hold, and
  completed prerequisites are mandatory;
- regular plans are 9-18 credits, with 18 as the default/recommended target
  and hard normal maximum;
- GPA below 2.0 is probation and has a 12-credit maximum;
- the first successful capacity commit wins, with no waitlist or override;
- every unresolved meeting overlap blocks submission and the travel buffer is
  zero/disabled; and
- automatic exceptions, add/drop, withdrawal, and advisor workflows are not
  part of the POC.

Repeat attempts have a typed rule category but remain deny-by-default in this
profile because Ahmed has not approved a repeat workflow for the POC. The
decision returns `REPEAT_POLICY_UNAVAILABLE`; it does not contradict or
reinterpret the sourced institutional repeat rules.

**Rationale**: These rules are sufficient to prove explainable eligibility,
capacity, and conflict concepts without implementing ambiguous institutional
workflows.

**Curriculum decision**: Seed the exact 19-course Data Science snapshot in
`docs/DEMO_CURRICULUM.md`, curated from the official AASTMT College of
Artificial Intelligence page registered in `docs/POLICY_RESEARCH.md`.
Every copied field retains its URL and access date. Course credits remain
field-level `SyntheticDemo` values even when code/title/prerequisites are
`OfficialAASTMT`. A missing row needed to make
the POC coherent may be synthetic only when explicitly labelled synthetic in
data and UI; it cannot be attributed to AASTMT.

**Authority boundary**: Public AASTMT pages provide provenance. Ahmed's
approval authorizes this non-production demo profile only and is not AASTMT
production authorization.



## Open Research

No unresolved requirement clarification remains for the bounded demo profile.
Institutional values outside it remain release prerequisites/configuration
provenance and are not guessed as production defaults.
