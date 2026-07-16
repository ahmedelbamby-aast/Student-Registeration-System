# SPEC-006 NFR-1 Evidence

**Release result:** PASS

Runtime execution result: PASS — the .NET 10 generator produced 27 paths,
28 operations, 55 schemas, and cookie-security metadata. Three executable
baseline tests prove repeatable generation, semantic comparison, and rejection
of status drift while ignoring JSON formatting and object-property order.
`.github/scripts/Verify-OpenApi.ps1` runs the same gate in SPEC-018-owned CI.
