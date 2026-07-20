using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Application.Ports;

public sealed record CatalogueProgramQuery(
    string? Query,
    bool? Active,
    int Page,
    int PageSize,
    string? Sort);

public sealed record CatalogueVersionQuery(
    string? Scope,
    string? State,
    int Page,
    int PageSize,
    string? Sort);

public sealed record CatalogueImportValidationCommand(
    Guid ImportId,
    string ExpectedImportRowVersion,
    string ExpectedDraftRowVersion,
    string ActorReference);

public sealed record CatalogueImportPublishCommand(
    Guid ImportId,
    string ExpectedDraftRowVersion,
    string PreviewToken,
    Guid ClientRequestId,
    string ActorReference);

public interface ICatalogueAdministrationStore
{
    Task<Page<ProgramAdminDto>> ListProgramsAsync(
        CatalogueProgramQuery query,
        CancellationToken cancellationToken);

    Task<Page<CatalogueVersionSummaryDto>> ListVersionsAsync(
        CatalogueVersionQuery query,
        CancellationToken cancellationToken);

    Task<CatalogueDraftDto?> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken);

    Task<CatalogueDraftDto> CreateDraftAsync(
        CreateCatalogueDraftRequest request,
        CancellationToken cancellationToken);

    Task<CatalogueDraftDto> UpdateDraftAsync(
        Guid draftId,
        CatalogueDraftMutationRequest request,
        CancellationToken cancellationToken);

    Task<ImportBatchDto> CreateImportAsync(
        CreateImportRequest request,
        CancellationToken cancellationToken);

    Task<ImportBatchDto?> GetImportAsync(
        Guid importId,
        CancellationToken cancellationToken);

    Task<CatalogueValidationResult> ValidateImportAsync(
        CatalogueImportValidationCommand command,
        CancellationToken cancellationToken);

    Task<CatalogueVersionSummaryDto> PublishImportAsync(
        CatalogueImportPublishCommand command,
        CancellationToken cancellationToken);

    Task<StudentRoadmapDto?> GetStudentRoadmapAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken);
}

public enum CatalogueStoreFailure
{
    Validation,
    NotFound,
    Conflict,
}

public sealed class CatalogueStoreException(
    CatalogueStoreFailure failure,
    string code,
    string message) : Exception(message)
{
    public CatalogueStoreFailure Failure { get; } = failure;

    public string Code { get; } = code;
}
