# SPEC-011 NFR-3 Determinism Evidence

## Requirement and fixture

This is the canonical evidence for SPEC-011 NFR-3, release alias NFR-004,
and T047. The executable test runs 100 evaluations with the same student,
term, catalogue, policy, offering, group, current plan, and fixed UTC instant.

The fixed version set includes:

- catalogue `CATALOGUE-2026.1`;
- policy `DEMO-POC-2026.1`; and
- current plan `plan/15`.

## Assertion

Each full result is serialized with the same property order and compared
byte-for-byte as text. The assertion covers decision values, reason order,
group order, source/version fields, and the evaluation instant.

**Result: PASS.**
