# Policy Decision and Provenance Contract

**Contract:** `policy-decision/1.0`
**Shape owner:** SPEC-002
**Runtime projection owners:** SPEC-009 and SPEC-011
**Historical snapshot owner:** SPEC-015

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

## Determinism

For the same input, immutable policy version, and authoritative evaluation
instant, all fields and result order are identical. Results use stable order by
typed-rule registry order, then reasonCode, then source reference. Locale may
change UI presentation later but not persisted codes or canonical explanation
parameters.

## Input summary and privacy

`inputSummary` is bounded and privacy-safe. It contains only values actually
used, such as standing code, GPA, earned credits, selected/current credits,
blocking-hold count, prerequisite codes/statuses, group capacity values, and
meeting intervals. It MUST NOT contain a name, email, University ID, password,
credential material, raw claims, unrelated transcript details, or arbitrary
client input.

## Result rules

- `eligible` is true only when every required result passed.
- Each blocking reason has `overridePossible=false` for this demo profile.
- `sourceUrl` and `sourceAccessedOn` identify provenance; internal demo
  approvals use their repository reference as the source URL value.
- Missing source/provenance is itself a blocking decision-data failure.
- `evaluatedAtUtc` comes from server time and policy effective dates use UTC.
- Explanations are safe, actionable, and never claim unverified policy is
  official AASTMT authority.

SPEC-015 persists the exact policy version, bounded input summary, result
records, and provenance used at submission. Later rulebook versions cannot
rewrite that snapshot.
