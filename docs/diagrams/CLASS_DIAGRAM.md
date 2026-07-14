# Domain and Application Class Diagram

This system-wide class-level view shows the canonical use-case services,
transaction collaborators, module seams, and core registration model. Exact
fields remain in owner-spec contracts and the ERD; these panels are not an
instruction to create one project per architectural layer.

## Application orchestration

```mermaid
classDiagram
  class RegistrationEndpoint
  class RegistrationCommandFactory
  class RegistrationTransactionCoordinator
  class SqlSeatAllocator
  class RegistrationSubmissionStore
  class StudentRegistrationDbContext
  class IStudentAcademicReader
  class ISchedulingReader
  class IEligibilityEvaluator
  class IRegistrationPlanReader
  class CurrentUser
  class TimeProvider

  RegistrationEndpoint --> RegistrationCommandFactory
  RegistrationCommandFactory --> CurrentUser
  RegistrationCommandFactory --> TimeProvider
  RegistrationCommandFactory --> IStudentAcademicReader
  RegistrationCommandFactory --> ISchedulingReader
  RegistrationCommandFactory --> IEligibilityEvaluator
  RegistrationCommandFactory --> IRegistrationPlanReader
  RegistrationCommandFactory --> RegistrationTransactionCoordinator
  RegistrationTransactionCoordinator --> SqlSeatAllocator
  RegistrationTransactionCoordinator --> RegistrationSubmissionStore
  RegistrationTransactionCoordinator --> StudentRegistrationDbContext
```

```mermaid
classDiagram
  class ScheduleRecommendationEndpoint
  class OptimizationCoordinator
  class ScheduleOptimizer
  class ScheduleScorer
  class ISchedulingReader
  class RecommendationApplicationService
  class AppContextEndpoint
  class AcademicContextResolver
  class ISessionContextReader
  class IAcademicContextReader

  ScheduleRecommendationEndpoint --> OptimizationCoordinator
  OptimizationCoordinator --> ScheduleOptimizer
  ScheduleOptimizer --> ScheduleScorer
  ScheduleOptimizer --> ISchedulingReader
  ScheduleRecommendationEndpoint --> RecommendationApplicationService
  AppContextEndpoint --> AcademicContextResolver
  AppContextEndpoint --> ISessionContextReader
  AcademicContextResolver --> IAcademicContextReader
```

## System module services

```mermaid
classDiagram
  class StudentAuthenticationService
  class StudentActivationService
  class StaffAuthenticationService
  class DemoDatabaseInitializer
  class DemoIdentitySeedContributor
  class DemoStudentProfileSeedContributor
  class AdminUserLifecycleService
  class IAdminUserLifecycleCommands
  class AcademicContextResolver
  class RegistrationWindowService
  class CataloguePublicationService
  class PolicyAdministrationService
  class IPolicyEvaluator
  class OfferingPublicationService
  class ResourceAvailabilityService
  class ISchedulingReader
  class EligibilityService
  class RegistrationPlanService
  class ScheduleOptimizer
  class RegistrationTransactionCoordinator
  class RegistrationReceiptService
  class StaffWorkspaceQueries
  class AuditTransactionWriter
  class IAuditEventWriter
  class IAuditEventReader
  class IRegistrationSubmissionReader
  class IOperationalMetricsReader
  class AuditEventQueries
  class AuditExportService
  class AdminMetricsQuery
  class ObservabilityExtensions

  AdminUserLifecycleService ..|> IAdminUserLifecycleCommands
  DemoDatabaseInitializer --> DemoIdentitySeedContributor
  DemoDatabaseInitializer --> DemoStudentProfileSeedContributor
  AcademicContextResolver --> RegistrationWindowService
  EligibilityService --> IPolicyEvaluator
  EligibilityService --> ISchedulingReader
  RegistrationPlanService --> EligibilityService
  ScheduleOptimizer --> ISchedulingReader
  AuditTransactionWriter ..|> IAuditEventWriter
  RegistrationTransactionCoordinator --> IAuditEventWriter
  OfferingPublicationService --> IAuditEventWriter
  RegistrationReceiptService --> IRegistrationSubmissionReader
  StaffWorkspaceQueries --> ISchedulingReader
  AuditEventQueries --> IAuditEventReader
  AuditExportService --> AuditEventQueries
  AdminMetricsQuery --> IOperationalMetricsReader
```

## Core domain model

```mermaid
classDiagram
  class Student {
    +StudentId Id
    +ApplicationUserId ApplicationUserId
    +string ProgramCode
    +string Cohort
    +Gpa CurrentGpa
    +Credits EarnedCredits
    +AcademicStanding Standing
    +bool IsActive
    +string Source
    +string SourceReference
    +string DataVersion
    +Instant DataAsOfUtc
    +Instant ImportedAtUtc
    +RowVersion Version
  }
  class AcademicTerm {
    +AcademicTermId Id
    +TermCode Code
    +Guid CreationClientRequestId
    +string CreationPayloadHash
    +string DisplayName
    +LocalDate TeachingStartsOn
    +LocalDate TeachingEndsOn
    +TimeZoneId TimeZone
    +TermState State
    +RowVersion Version
  }
  class RegistrationWindow {
    +RegistrationWindowId Id
    +AcademicTermId TermId
    +WindowScopeType ScopeType
    +string ScopeValue
    +WindowState State
    +Instant OpensAtUtc
    +Instant ClosesAtUtc
    +RowVersion Version
    +Allows(Instant now, Student student) bool
  }
  class StudentTermAcademicState {
    +StudentTermAcademicStateId Id
    +StudentId StudentId
    +AcademicTermId TermId
    +Gpa GpaAtStart
    +Credits EarnedCreditsAtStart
    +AcademicStanding StandingAtStart
    +string Source
    +string SourceReference
    +string DataVersion
    +Instant DataAsOfUtc
    +RowVersion Version
  }
  class TranscriptAttempt {
    +TranscriptAttemptId Id
    +StudentId StudentId
    +AcademicTermId TermId
    +TranscriptAttemptId SupersedesAttemptId
    +string CourseCode
    +Credits Credits
    +string GradeCode
    +AttemptStatus Status
    +string Source
    +string SourceReference
    +Instant ImportedAtUtc
    +SupersedesCurrentLeaf(TranscriptAttempt prior) bool
  }
  class StudentHold {
    +StudentHoldId Id
    +StudentId StudentId
    +AcademicTermId TermId
    +string Code
    +string Message
    +bool BlocksRegistration
    +Instant EffectiveFromUtc
    +Instant EffectiveToUtc
    +string Source
    +string SourceReference
    +Instant ImportedAtUtc
  }
  class CourseOffering {
    +CourseOfferingId Id
    +CourseId CourseId
    +OfferingState State
    +RowVersion Version
  }
  class SectionGroup {
    +SectionGroupId Id
    +GroupCode Code
    +int Capacity
    +int EnrolledCount
    +GroupState State
    +bool RegistrationPaused
    +RowVersion Version
  }
  class MeetingSlot {
    +DayOfWeek Day
    +TimeOnly Start
    +TimeOnly End
    +RoomId RoomId
    +Overlaps(MeetingSlot other) bool
  }
  class StaffTermAvailability {
    +StaffTermAvailabilityId Id
    +StaffId StaffId
    +AcademicTermId TermId
    +Instant DeadlineUtc
    +RowVersion Version
    +ReplaceRanges()
  }
  class StaffAvailability {
    +DayOfWeek Day
    +TimeOnly Start
    +TimeOnly End
    +AvailabilityType Type
  }
  class RegistrationPlan {
    +RegistrationPlanId Id
    +StudentId StudentId
    +AcademicTermId TermId
    +PlanState State
    +RowVersion Version
  }
  class RegistrationSubmission {
    +RegistrationSubmissionId Id
    +Guid ClientRequestId
    +string PayloadHash
    +SubmissionState State
    +string ResultCode
  }
  class RegistrationReceipt {
    <<read projection>>
    +RegistrationSubmissionId SubmissionId
    +string Reference
    +string SnapshotJson
    +Instant IssuedAtUtc
  }
  class Enrollment {
    +EnrollmentId Id
    +StudentId StudentId
    +CourseOfferingId OfferingId
    +SectionGroupId GroupId
    +EnrollmentState State
  }
  class EligibilityDecision {
    +bool IsEligible
    +PolicyVersion PolicyVersion
    +InputSummary Inputs
  }
  class RuleResult {
    +string ReasonCode
    +bool Passed
    +string Explanation
    +string RequiredValue
    +string CurrentValue
    +string Source
    +bool OverridePossible
  }

  AcademicTerm "1" --> "*" RegistrationWindow
  AcademicTerm "1" --> "*" CourseOffering
  AcademicTerm "1" --> "*" StudentTermAcademicState
  AcademicTerm "1" --> "*" TranscriptAttempt
  AcademicTerm "1" --> "*" StudentHold
  Student "1" --> "*" StudentTermAcademicState
  Student "1" --> "*" TranscriptAttempt
  Student "1" --> "*" StudentHold
  TranscriptAttempt "0..1" --> "0..1" TranscriptAttempt : supersedes
  CourseOffering "1" *-- "*" SectionGroup
  SectionGroup "1" *-- "*" MeetingSlot
  StaffTermAvailability "1" *-- "*" StaffAvailability
  Student "1" --> "*" RegistrationPlan
  Student "1" --> "*" RegistrationSubmission
  RegistrationSubmission "1" --> "0..1" RegistrationReceipt : projects
  RegistrationSubmission "1" --> "*" Enrollment
  EligibilityDecision "1" *-- "*" RuleResult
```

AcademicTerm creation replay is bound by globally unique
`CreationClientRequestId` plus `CreationPayloadHash`; it does not add an
idempotency entity. Transcript supersession is filtered-unique when non-null,
targets only the current leaf with the same student/course/term, and remains
acyclic because historical rows are immutable. Term/window publication and
profile corrections use expected versions.

## Shared context contracts

```mermaid
classDiagram
  class AppContextDto {
    +Instant serverTimeUtc
    +string timeZoneId
    +TermSummaryDto teachingTerm
    +TermSummaryDto registrationTerm
    +RegistrationWindowState registrationWindowState
    +RegistrationWindowSummaryDto registrationWindow
    +ServiceState serviceState
    +string displayName
    +string[] authorizedRoles
    +string activeRole
    +SessionState sessionState
    +Instant expiresAtUtc
    +string supportReferencePath
  }
  class TermSummaryDto {
    +Guid id
    +string code
    +string label
    +TermState state
    +string rowVersion
  }
  class RegistrationWindowSummaryDto {
    +Guid id
    +RegistrationWindowState state
    +Instant opensAtUtc
    +Instant closesAtUtc
    +string rowVersion
  }
  class PublicContextDto {
    +Instant serverTimeUtc
    +string timeZoneId
    +string teachingTermLabel
    +string registrationTermLabel
    +RegistrationWindowState registrationWindowState
    +ServiceState serviceState
  }

  AppContextDto --> TermSummaryDto : nullable terms
  AppContextDto --> RegistrationWindowSummaryDto : nullable matched window
```

`RegistrationWindowSummaryDto` is authenticated-context-only. The public DTO
remains exactly six fields and does not expose the matched window's ID,
interval, row version, or personal/session data.

## Core interfaces

```csharp
public interface IEligibilityEvaluator
{
    Task<EligibilityDecision> EvaluateAsync(
        StudentId studentId,
        AcademicTermId termId,
        CourseOfferingId offeringId,
        CancellationToken cancellationToken);
}

public interface IRegistrationTransactionCoordinator
{
    Task<RegistrationSubmissionResult> TrySubmitAsync(
        RegistrationSubmissionCommand command,
        CancellationToken cancellationToken);
}

// ScheduleOptimizer and AcademicContextResolver are concrete module-owned
// services with the CancellationToken-bearing methods specified by SPEC-013
// and SPEC-008; no speculative duplicate interface is introduced here.
```

These are design contracts, not public implementation types. Exact DTO fields
and HTTP outcomes are governed by SPEC-006 and the owning feature contract.

## Dependency rule

Endpoints depend on application use cases inside their business module.
Application code depends on domain types and narrow ports. The SQL Server
infrastructure project implements persistence ports and owns
`StudentRegistrationDbContext` and migrations. Domain folders do not reference
ASP.NET Core, EF Core, SQL Server, Blazor, or the infrastructure project.
