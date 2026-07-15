namespace StudentRegistration.Contracts.Academics;

public sealed record AdminStudentLocatorDto
{
    public AdminStudentLocatorDto(
        string studentId,
        string universityId,
        string programCode,
        string cohort,
        string standing,
        string dataVersion)
    {
        StudentId = AcademicContractGuard.Required(studentId, nameof(studentId), 100);
        UniversityId = AcademicContractGuard.Required(
            universityId,
            nameof(universityId),
            50);
        ProgramCode = AcademicContractGuard.Required(
            programCode,
            nameof(programCode),
            50);
        Cohort = AcademicContractGuard.Required(cohort, nameof(cohort), 50);
        Standing = AcademicContractGuard.Required(standing, nameof(standing), 100);
        DataVersion = AcademicContractGuard.Required(
            dataVersion,
            nameof(dataVersion),
            200);
    }

    public string StudentId { get; }

    public string UniversityId { get; }

    public string ProgramCode { get; }

    public string Cohort { get; }

    public string Standing { get; }

    public string DataVersion { get; }
}
