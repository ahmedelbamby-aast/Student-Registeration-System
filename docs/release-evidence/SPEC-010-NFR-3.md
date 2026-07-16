# SPEC-010 NFR-3 Term-Timezone Display Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T18:00:00Z  
**Boundary:** Non-production Spec 010 component evidence

## Executable gate

Two direct cases use the canonical `AcademicContextOptions` IANA timezone
authority and `OfferingService.GetStudentDetailAsync` scheduling projection:

| Term timezone | Meeting projection |
|---|---|
| `Africa/Cairo` | Sunday 09:00-10:30 |
| `Europe/London` | Monday 13:15-14:45 |

For both cases, the service must preserve the meeting `DayOfWeek`, `TimeOnly`
start, and `TimeOnly` end values exactly. `AcademicContextOptions` separately
proves the configured IANA authority; the Scheduling projection carries the
already term-local fields and performs no browser-timezone conversion. The
canonical API value remains `Tutorial`; the presentation label may be
`Section`. `CourseOfferingDto` does not add a timezone field.
The browser timezone is not consulted by this Scheduling projection.

This evidence proves the approved local-day/local-time representation and the
IANA term-timezone authority. It does not claim that a browser clock or device
timezone may override the term configuration.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec010.NFR_3EvidenceTests" -p:TreatWarningsAsErrors=true
```

- 3 passed
- 0 failed
- Configured timezones: 2

Quality test normalized-LF SHA-256: `38D935DEFC2EC4D668F4F0B3778959B4B522373378ECD52B46BCEC147872E62B`

**Result: PASS.**
