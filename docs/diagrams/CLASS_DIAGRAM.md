# Delivered SPEC-008 Class Diagram

This diagram documents the implemented Academic Term and Student Profile slice
inside the modular monolith. It names only delivered SPEC-008 types and their
direct collaborators. DTO field details remain in the shared Contracts project
and the feature API contract; database columns remain in the ERD and EF mapping.

## Browser, API, and application boundary

```mermaid
classDiagram
  direction LR

  class AcademicApiClient {
    <<Blazor same-origin facade>>
    +GetPublicContextAsync()
    +GetAppContextAsync()
    +GetStudentAcademicContextAsync()
    +ListAdminTermsAsync()
    +CreateAdminTermAsync()
    +UpdateAdminTermAsync()
    +PublishAdminRegistrationWindowAsync()
    +SearchAdminStudentsAsync()
    +GetAdminStudentAcademicContextAsync()
    +CorrectAdminStudentAcademicProfileAsync()
  }
  class Spec008Endpoints {
    <<HTTP endpoint facade>>
    +MapSpec008Endpoints()
  }
  class AcademicContextResolver
  class StudentAcademicProfileService
  class AdminAcademicManagementService
  class RegistrationWindowService
  class IAcademicStudentScopeReader {
    <<port>>
  }
  class IAcademicSessionContextAdapter {
    <<port>>
  }
  class AcademicSessionContextAdapter {
    <<API composition adapter>>
  }
  class SessionLifecycleService {
    <<SPEC-007 application service>>
  }
  class Contracts {
    <<dependency-neutral DTOs>>
  }

  AcademicApiClient ..> Spec008Endpoints : same-origin HTTP + JSON
  AcademicApiClient --> Contracts
  Spec008Endpoints --> Contracts
  Spec008Endpoints --> AcademicContextResolver
  Spec008Endpoints --> StudentAcademicProfileService
  Spec008Endpoints --> AdminAcademicManagementService
  Spec008Endpoints --> IAcademicStudentScopeReader
  Spec008Endpoints --> IAcademicSessionContextAdapter
  AcademicSessionContextAdapter ..|> IAcademicSessionContextAdapter
  AcademicSessionContextAdapter --> SessionLifecycleService
  AcademicSessionContextAdapter --> Contracts
  AdminAcademicManagementService --> RegistrationWindowService : term owner
  AdminAcademicManagementService --> StudentAcademicProfileService : profile owner
```

`AcademicApiClient` does transport, antiforgery-token forwarding, and bounded
response/error deserialization. It does not decide term, window, authorization,
time, GPA, hold, or concurrency outcomes. `Spec008Endpoints` applies the named
server policies and maps application outcomes to the ten HTTP contracts.
Identity-session composition stays in the API project:
`AcademicSessionContextAdapter` combines SPEC-007 session data with the
academic result through `IAcademicSessionContextAdapter`; the Academics project
does not reference the IdentityAccess implementation.

## Application services, ports, and SQL adapter

```mermaid
classDiagram
  direction LR

  class AcademicContextResolver
  class RegistrationWindowService
  class StudentAcademicProfileService
  class AdminAcademicManagementService
  class DemoStudentProfileSeedContributor
  class TimeProvider

  class IAcademicContextReader { <<port>> }
  class IAcademicStudentScopeReader { <<port>> }
  class IRegistrationWindowStore { <<port>> }
  class IStudentAcademicProfileStore { <<port>> }
  class IAdminAcademicStore { <<port>> }
  class IDemoStudentProfileSeedStore { <<port>> }
  class IAuditEventWriter { <<shared audit port>> }

  class AcademicStore {
    <<single SQL adapter>>
  }
  class StudentRegistrationDbContext {
    <<shared EF Core DbContext>>
  }
  class AcademicContextModelConfiguration {
    <<EF mapping contribution>>
  }

  AcademicContextResolver --> IAcademicContextReader
  AcademicContextResolver --> TimeProvider
  RegistrationWindowService --> IRegistrationWindowStore
  RegistrationWindowService --> TimeProvider
  StudentAcademicProfileService --> IStudentAcademicProfileStore
  StudentAcademicProfileService --> TimeProvider
  AdminAcademicManagementService --> IAdminAcademicStore
  AdminAcademicManagementService --> RegistrationWindowService
  AdminAcademicManagementService --> StudentAcademicProfileService
  DemoStudentProfileSeedContributor --> IDemoStudentProfileSeedStore

  AcademicStore ..|> IAcademicContextReader
  AcademicStore ..|> IAcademicStudentScopeReader
  AcademicStore ..|> IRegistrationWindowStore
  AcademicStore ..|> IStudentAcademicProfileStore
  AcademicStore ..|> IAdminAcademicStore
  AcademicStore ..|> IDemoStudentProfileSeedStore
  AcademicStore --> StudentRegistrationDbContext
  AcademicStore --> IAuditEventWriter
  AcademicStore --> TimeProvider
  StudentRegistrationDbContext --> AcademicContextModelConfiguration
```

The six narrow ports are implemented by one scoped `AcademicStore`. This is a
simple adapter choice, not six repositories: the ports express consumer needs
while one SQL implementation preserves the local transaction and audit
boundary. SPEC-008 adds its mapping to the existing
`StudentRegistrationDbContext`; it does not create another DbContext,
repository base class, unit-of-work abstraction, or distributed transaction.

## Delivered domain entities

```mermaid
classDiagram
  class AcademicTerm {
    +Guid Id
    +string Code
    +Guid CreationClientRequestId
    +string CreationPayloadHash
    +string DisplayName
    +DateOnly TeachingStartsOn
    +DateOnly TeachingEndsOn
    +string TimeZoneId
    +TermState State
    +byte[] Version
  }
  class RegistrationWindow {
    +Guid Id
    +Guid TermId
    +RegistrationWindowScopeType ScopeType
    +string ScopeValue
    +DateTime OpensAtUtc
    +DateTime ClosesAtUtc
    +RegistrationWindowLifecycleState State
    +byte[] Version
    +GetComputedState(serverNowUtc)
  }
  class Student {
    +Guid Id
    +Guid ApplicationUserId
    +string ProgramCode
    +string Cohort
    +decimal CurrentGpa
    +decimal EarnedCredits
    +string Standing
    +bool IsActive
    +string Source
    +string SourceReference
    +string DataVersion
    +DateTime DataAsOfUtc
    +DateTime ImportedAtUtc
    +byte[] Version
  }
  class StudentTermAcademicState {
    +Guid Id
    +Guid StudentId
    +Guid TermId
    +decimal GpaAtStart
    +decimal EarnedCreditsAtStart
    +string StandingAtStart
    +string Source
    +string SourceReference
    +string DataVersion
    +DateTime DataAsOfUtc
    +byte[] Version
  }
  class TranscriptAttempt {
    +Guid Id
    +Guid StudentId
    +Guid TermId
    +Guid SupersedesAttemptId
    +string CourseCode
    +decimal Credits
    +string GradeCode
    +TranscriptAttemptStatus Status
    +string Source
    +string SourceReference
    +DateTime ImportedAtUtc
  }
  class StudentHold {
    +Guid Id
    +Guid StudentId
    +Guid TermId
    +string Code
    +string Message
    +bool BlocksRegistration
    +DateTime EffectiveFromUtc
    +DateTime EffectiveToUtc
    +string Source
    +string SourceReference
    +DateTime ImportedAtUtc
    +IsActiveAt(instantUtc)
  }

  AcademicTerm "1" --> "0..*" RegistrationWindow
  AcademicTerm "1" --> "0..*" StudentTermAcademicState
  AcademicTerm "1" --> "0..*" TranscriptAttempt
  AcademicTerm "1" --> "0..*" StudentHold
  Student "1" --> "0..*" StudentTermAcademicState
  Student "1" --> "0..*" TranscriptAttempt
  Student "1" --> "0..*" StudentHold
  TranscriptAttempt "0..1 prior" --> "0..1 successor" TranscriptAttempt : supersedes
```

These are the exact six persisted Academics entities. `ProgramCode` and
`CourseCode` are stable sourced scalar references, not foreign keys to future
catalogue entities. `Student.ApplicationUserId` is the persistence link to the
SPEC-007 identity row, but `Student` stores only the identifier and has no
domain navigation or IdentityAccess implementation dependency. Computed
open/upcoming/closed/none window state is not a seventh entity or a persisted
lifecycle.

## Dependency direction and deliberate exclusions

```text
Blazor Client -> dependency-neutral Contracts
Blazor Client --same-origin HTTP--> HTTP endpoint facade
HTTP endpoint facade -> Academics application services and ports
API composition adapter -> Academics session-composition port + SPEC-007 service
Academics application -> Academics domain + narrow ports + TimeProvider
SQL infrastructure adapter -> Academics ports/domain + shared DbContext/audit port
```

Dependencies do not point from Domain to Application, API, Client, EF Core, SQL
Server, or IdentityAccess. There is one API host, one shared SQL database, one
`StudentRegistrationDbContext`, and local EF/SQL transactions. This delivered
SPEC-008 view intentionally contains no `CourseOffering`, `SectionGroup`,
`MeetingSlot`, optimizer, eligibility engine, seat allocator, enrollment,
registration submission, event bus, message broker, microservice, distributed
lock, second DbContext, generic repository, or extra unit of work. Those future
capabilities remain with their owning specs and are not dependencies of
SPEC-008.
