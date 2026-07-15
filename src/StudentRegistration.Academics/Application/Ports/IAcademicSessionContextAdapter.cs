using StudentRegistration.Academics.Application;
using StudentRegistration.Contracts;

namespace StudentRegistration.Academics.Application.Ports;

public interface IAcademicSessionContextAdapter
{
    Task<AppContextDto?> ComposeAsync(
        Guid applicationUserId,
        string? activeRole,
        AcademicContextResult academicContext,
        CancellationToken cancellationToken = default);
}
