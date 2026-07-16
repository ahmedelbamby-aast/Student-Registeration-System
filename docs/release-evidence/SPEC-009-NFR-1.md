# SPEC-009 NFR-1 10,000-Row Validation Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T15:00:00Z  
**Result boundary:** Non-production Gate A quality evidence

## Requirement and executable gate

NFR-1 requires a staging-sized import validation of 10,000 rows to finish
within 30 seconds. The focused test constructs 10,000 unique normalized course
rows, alternates `official-source` and `synthetic-demo` provenance, validates
the complete set through `CataloguePublicationService`, and requires a valid
canonical SHA-256 result with no row errors.

The test uses no network, scraper, production dataset, or hidden source field.
Official-source rows retain the approved AASTMT URL and access date and list
their synthetic credit/status fields. Synthetic rows identify every locally
created field.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec009.NFR_1EvidenceTests" -p:TreatWarningsAsErrors=true
```

- Performance test: 1 passed
- 1 passed
- 0 failed
- Rows: 10,000
- Maximum duration: 30 seconds
- Configuration: Release

Quality test normalized-LF SHA-256: `A9838F34EB0336E947F1092DFFE6B62F8CEF82324803F4964AC7080E37645FE5`

The executable assertion compares the measured elapsed time directly with the
30-second gate. No prose timing estimate substitutes for the stopwatch result.

**Result: PASS.**
