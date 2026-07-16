# SPEC-009 NFR-4 Audit and Provenance Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T15:00:00Z

## Requirement

Every published change must retain actor, reason, source, and server timestamp.
Catalogue and policy values must also remain understandable through source,
access date, value classification, and explicit synthetic field evidence.

## Executable evidence

The focused suite creates a bound preview with an injected fixed UTC clock,
confirms publication, and inspects the exact store command. It verifies:

- actor: the authenticated actor reference;
- reason: the reviewed publication reason;
- source: the governed command source;
- server timestamp: the injected authoritative UTC instant;
- correlation reference and dependency versions;
- catalogue source URL and 2026-07-13 access date;
- `official-source` classification plus each synthetic field name;
- typed policy value classification and per-rule source classification.

The evidence is aggregate-only and contains no student identity, password,
credential, cookie, security stamp, or production record.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec009.NFR_4EvidenceTests" -p:TreatWarningsAsErrors=true
```

- Audit/provenance test: 1 passed
- 1 passed
- 0 failed

Quality test normalized-LF SHA-256: `CA97F89C19930C192C048AD16E6B057A66659E9C281F7FE5941D65FC8F754D73`

**Result: PASS.**
