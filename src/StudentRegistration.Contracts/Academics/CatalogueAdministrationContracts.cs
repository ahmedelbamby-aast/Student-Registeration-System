using System.Text.Json;

namespace StudentRegistration.Contracts.Academics;

public sealed record CatalogueFieldProvenanceDto(
    string SourceReference,
    DateOnly AccessedOn,
    string SourceKind,
    IReadOnlyList<string> SyntheticFields);

public sealed record CourseAdminDto(
    Guid Id,
    string Code,
    string Title,
    decimal Credits,
    bool Active,
    CatalogueFieldProvenanceDto Provenance,
    string RowVersion);

public sealed record ProgramAdminDto(
    Guid Id,
    string Code,
    string DisplayName,
    bool Active,
    CatalogueFieldProvenanceDto Provenance);

public sealed record CurriculumCourseAdminDto(
    string ProgramCode,
    string CourseCode,
    int Level,
    int? TermSequence,
    bool Required,
    CatalogueFieldProvenanceDto Provenance);

public sealed record CatalogueDraftDto(
    Guid Id,
    string Scope,
    Guid? BasedOnVersionId,
    string State,
    string CanonicalContentHash,
    string RowVersion,
    IReadOnlyList<ProgramAdminDto> Programs,
    IReadOnlyList<CourseAdminDto> Courses,
    IReadOnlyList<CurriculumCourseAdminDto> Curricula);

public sealed record CatalogueDraftOperation(
    string Kind,
    ProgramAdminDto? Program = null,
    CourseAdminDto? Course = null,
    CurriculumCourseAdminDto? CurriculumCourse = null,
    string? ProgramCode = null,
    string? CourseCode = null,
    IReadOnlyList<string>? RequiredCourseCodes = null,
    CatalogueFieldProvenanceDto? Provenance = null);

public sealed record CatalogueDraftMutationRequest(
    string ExpectedDraftRowVersion,
    string Reason,
    string Source,
    IReadOnlyList<CatalogueDraftOperation> Operations);

public sealed record CatalogueVersionSummaryDto(
    Guid Id,
    string Scope,
    string Version,
    string State,
    string Source,
    DateTime PublishedAtUtc);

public sealed record ImportRowErrorDto(
    int? Row,
    string? Field,
    string Code,
    string Message);

public sealed record ImportBatchDto(
    Guid Id,
    Guid DraftId,
    string State,
    string Source,
    DateOnly AccessedOn,
    string ContentHash,
    int SyntheticFieldCount,
    string RowVersion,
    IReadOnlyList<ImportRowErrorDto> Errors,
    int TotalErrorCount,
    bool HasMoreErrors,
    Guid? PublishedVersionId);

public sealed record CreateImportRequest(
    Guid DraftId,
    string Source,
    DateOnly AccessedOn,
    string ContentHash,
    IReadOnlyList<string> SyntheticFields,
    Guid ClientRequestId);

public sealed record CatalogueValidationErrorDto(
    int? Row,
    string? Field,
    string Code,
    string Message);

public sealed record CatalogueValidationRequest(
    string ExpectedImportRowVersion,
    string ExpectedDraftRowVersion);

public sealed record CatalogueValidationResult(
    bool Valid,
    IReadOnlyList<CatalogueValidationErrorDto> Errors,
    string? PreviewToken,
    string ExpectedDraftRowVersion,
    IReadOnlyDictionary<string, string> DependencyVersions);

public sealed record PublishVersionRequest(
    string ExpectedDraftRowVersion,
    string PreviewToken,
    Guid ClientRequestId);

public sealed record PolicyRuleAdminDto(
    Guid? Id,
    string Code,
    string ValueType,
    JsonElement Value,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string SourceReference,
    string SourceKind);

public sealed record PolicySetAdminDto(
    Guid Id,
    string Scope,
    string Version,
    string State,
    string RowVersion,
    IReadOnlyList<PolicyRuleAdminDto> Rules);

public sealed record PolicySetOperation(
    string Kind,
    PolicyRuleAdminDto? Rule = null,
    Guid? RuleId = null);

public sealed record CreatePolicySetRequest(
    string Scope,
    string Version,
    Guid TermId,
    Guid? ProgramId,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string Reason,
    IReadOnlyList<PolicySetOperation> Operations,
    Guid ClientRequestId);

public sealed record PolicySetMutationRequest(
    string ExpectedPolicySetRowVersion,
    string Reason,
    IReadOnlyList<PolicySetOperation> Operations);

public sealed record PolicyValidationRequest(
    string ExpectedPolicySetRowVersion);

public sealed record PolicyPublishRequest(
    string ExpectedPolicySetRowVersion,
    string PreviewToken,
    Guid ClientRequestId);

public sealed record PolicySimulationRequest(
    Guid PolicySetId,
    string StudentContextFixtureId,
    IReadOnlyList<string> RequestedCourseCodes);

public sealed record PolicyRuleSimulationResultDto(
    string RuleCode,
    bool Passed,
    string? RequiredValue,
    string? CurrentValue,
    string SourceReference,
    string SourceKind,
    string Explanation);

public sealed record PolicySimulationResult(
    bool Eligible,
    string PolicyVersion,
    IReadOnlyList<PolicyRuleSimulationResultDto> RuleResults);
