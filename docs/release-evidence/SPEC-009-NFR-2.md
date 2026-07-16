# SPEC-009 NFR-2 Deterministic Simulation Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T15:00:00Z

## Requirement and result

NFR-2 requires the same policy version and input to produce the same result.
The executable gate validates the exact demo policy, including the 2.0 GPA and
96 earned credits Project I boundary, then executes 100 repeated simulations
for one fixed input.

Every result is byte-identical after JSON serialization. Rule results retain
ordinal rule-code order, pass/fail state, required and current values,
explanation, source reference, and official-versus-synthetic classification.
Simulation does not invoke the publication store and creates no durable state.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec009.NFR_2EvidenceTests" -p:TreatWarningsAsErrors=true
```

- Determinism test: 1 passed
- 1 passed
- 0 failed
- Repetitions: 100 repeated simulations

Quality test normalized-LF SHA-256: `49F781A1DB101E0B30EF53E3604E69BF7E82A3A249B25C39098A7025618822B2`

**Result: PASS.**
