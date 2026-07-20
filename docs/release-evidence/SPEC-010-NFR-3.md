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

Quality test normalized-LF SHA-256: `76796C753ED1810C00F9E25A85F4BB2A884D2CC71C53F8274E7D789D9644CDB8`

**Result: PASS.**
