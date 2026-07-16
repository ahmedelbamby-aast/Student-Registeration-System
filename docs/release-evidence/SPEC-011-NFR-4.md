# SPEC-011 NFR-4 Non-Color Status Evidence

## Requirement

This is the canonical evidence for SPEC-011 NFR-4, release alias NFR-005,
and T048.

## Evidence

STU-02 and STU-03 render eligibility with visible `Eligible` or `Unavailable`
text and a separate ✓ or ✕ icon. Rule presentation also exposes the stable
reason code and plain-language reason message. Group state uses `Selectable`
or `Unavailable` text plus the same independent icon treatment.

The focused quality test binds these claims to the Razor source and to the
Playwright feature tests that assert visible text, icons, and the
`GROUP_FULL` stale reason.

## Boundary

This is feature-level non-color status evidence for SPEC-011. Pending
SPEC-003 product-wide visual governance remains separate; this is not a product-wide WCAG certification.

**Result: PASS.**
