# Domain and Application Class Diagram

## Main orchestration

```mermaid
classDiagram
  class RegistrationEndpoint
  class RegistrationService
  class AcademicTermResolver
  class EligibilityEvaluator
  class IEligibilityRule
  class TimetableOptimizer
  class IRegistrationCommitter
  class SqlRegistrationCommitter
  class RegistrationDbContext
  class IStudentAcademicReader
  class ISchedulingReader
  class TimeProvider
  class CurrentUser

  RegistrationEndpoint --> RegistrationService
  RegistrationService --> AcademicTermResolver
  RegistrationService --> EligibilityEvaluator
  RegistrationService --> TimetableOptimizer
  RegistrationService --> IRegistrationCommitter
  RegistrationService --> CurrentUser
  AcademicTermResolver --> TimeProvider
  EligibilityEvaluator --> IEligibilityRule
  EligibilityEvaluator --> IStudentAcademicReader
  EligibilityEvaluator --> ISchedulingReader
  TimetableOptimizer --> ISchedulingReader
  IRegistrationCommitter <|.. SqlRegistrationCommitter
  SqlRegistrationCommitter --> RegistrationDbContext
```

## Domain model

```mermaid
classDiagram
  class Student {
    +StudentId Id
    +UniversityId UniversityId
    +Gpa CurrentGpa
    +Credits EarnedCredits
    +AcademicStanding Standing
  }
  class AcademicTerm {
    +AcademicTermId Id
    +TermCode Code
    +TimeZoneId TimeZone
    +TermState State
    +IsRegistrationOpen(Instant now) bool
  }
  class CourseOffering {
    +CourseOfferingId Id
    +Course Course
    +OfferingState State
    +IReadOnlyList~SectionGroup~ Groups
  }
  class SectionGroup {
    +SectionGroupId Id
    +GroupCode Code
    +int Capacity
    +int EnrolledCount
    +GroupState State
    +HasSeat() bool
  }
  class MeetingSlot {
    +DayOfWeek Day
    +TimeOnly Start
    +TimeOnly End
    +Room Room
    +Overlaps(MeetingSlot other) bool
  }
  class RegistrationPlan {
    +RegistrationPlanId Id
    +StudentId StudentId
    +AcademicTermId TermId
    +PlanState State
    +RowVersion Version
    +SelectGroup()
    +RemoveOffering()
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
    +IReadOnlyList~RuleResult~ Results
    +PolicyVersion PolicyVersion
  }
  class RuleResult {
    +string ReasonCode
    +bool Passed
    +string Explanation
    +string Source
    +bool OverridePossible
  }

  Student "1" --> "*" RegistrationPlan
  Student "1" --> "*" Enrollment
  AcademicTerm "1" --> "*" CourseOffering
  CourseOffering "1" --> "*" SectionGroup
  SectionGroup "1" --> "*" MeetingSlot
  RegistrationPlan --> CourseOffering
  RegistrationPlan --> SectionGroup
  EligibilityDecision "1" *-- "*" RuleResult
```

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

public interface ITimetableOptimizer
{
    ScheduleOptimizationResult Optimize(
        IReadOnlyList<CourseChoice> courses,
        SchedulePreferences preferences,
        TimeSpan timeBudget);
}

public interface IRegistrationCommitter
{
    Task<CommitResult> TryCommitAsync(
        RegistrationCommitRequest request,
        CancellationToken cancellationToken);
}
```

These are design contracts, not implementation code. Names and signatures
remain subject to SPEC-006 approval.

## Dependency rule

Endpoints depend on application services. Application services depend on
domain types and narrow ports. Infrastructure implements ports. Domain code
does not reference ASP.NET Core, EF Core, SQL Server, Blazor, or infrastructure
projects.
