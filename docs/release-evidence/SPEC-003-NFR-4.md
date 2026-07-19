# SPEC-003 NFR-4 Pointer Target Evidence

**Result: PASS — 2026-07-19.**

The approved `interactive-minimum` token is 2.75rem (44 CSS pixels at the root
16-pixel size) and is projected into runtime CSS. Each route accessibility
suite must include pointer-target measurement or its governed inline/spacing
exception; missing evidence fails closed.

Execution evidence: 2/2 NFR-4 quality tests passed in the 22/22 focused
SPEC-003 quality run. The deployed token is `2.75rem`, exactly 44 CSS pixels at
the governed 16px root, and every route source includes a measured 44px target
or a documented pointer-target exception.
