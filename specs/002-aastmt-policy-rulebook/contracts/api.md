# API Contract: AASTMT Policy Rulebook

## Feature Contract

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

Endpoint ownership:

- `POST /api/admin/policies/{policySetId}/simulate` — canonical owner SPEC-009.
- `GET /api/student/offerings/{offeringId}/eligibility` — canonical owner SPEC-011.

SPEC-002 contributes `PolicyDecisionDto`, provenance rules, and fail-closed
semantics. It owns no API handler.

`inputSummary` is a bounded, privacy-safe map of values actually used by the
decision. Historical persistence is owned by SPEC-015; SPEC-002 defines the
shape and SPEC-009/SPEC-011 project it without storing executable policy text.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Versioned updates/deletes follow SPEC-006 request-body `expectedRowVersion` and 409 `STALE_VERSION`; retryable commands follow their owner-spec idempotency contract.
- Dates use ISO 8601 and the server-configured academic term.
- Lists follow the exact SPEC-006 default-20/maximum-100 pagination and deterministic unique-ID tie-break sorting protocol.
