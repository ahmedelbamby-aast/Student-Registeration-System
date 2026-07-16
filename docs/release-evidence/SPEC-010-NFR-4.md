# SPEC-010 NFR-4 Bounded Admin List Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T18:00:00Z  
**Boundary:** Non-production Spec 010 component evidence

## Executable gate

The focused suite calls `OfferingService.ListAdminOfferingsAsync` with a
capturing passive store. The accepted request uses:

- term filter set;
- state filter `published`;
- query filter submitted as ` csc ` and normalized to trimmed `csc`;
- page 2;
- page size 100;
- default stable sort `courseCode,id`; and
- total count 250.

The returned page contains exactly 100 summary roots and echoes the applied
page, page size, total count, and stable sort. The captured port request proves
that term, state, and the trimmed query filters cross the application boundary.

Separate cases prove page size 0 and 101 return `PAGE_SIZE_INVALID`, page 0
returns `PAGE_INVALID`, trimmed query lengths outside 3 through 50 return
`VALIDATION_ERROR`, and an unsupported sort returns `VALIDATION_ERROR`. No
invalid request reaches the store. This prevents silent capping, unbounded
Admin reads, and store-defined sort injection.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec010.NFR_4EvidenceTests" -p:TreatWarningsAsErrors=true
```

- 6 passed
- 0 failed
- Maximum accepted page size: 100

Quality test normalized-LF SHA-256: `5F69EF352B7256EE666FE26A816BF8722F5635349A4307556B6B0E75A4D11BB7`

**Result: PASS.**
