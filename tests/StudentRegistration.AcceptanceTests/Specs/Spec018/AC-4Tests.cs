namespace StudentRegistration.AcceptanceTests.Specs.Spec018;

public sealed class AC_4Tests
{
    private const string ActivationGate =
        "Activation condition: SPEC-003 route records must advance from design-only after implementation-owner specs SPEC-007 through SPEC-017 deliver the critical routes, and SPEC-018 T067 must run automated WCAG checks plus signed keyboard and NVDA/Windows manual evidence.";

    [Fact(Skip = ActivationGate)]
    public void Critical_routes_pass_automated_keyboard_and_signed_screen_reader_gates()
    {
        // Given implemented critical student/staff routes in staging.
        // When automated, keyboard, and representative NVDA journeys run.
        // Then no serious or critical/major barrier remains and signed evidence is complete.
        throw new NotImplementedException(
            "Design records and planned fixtures are not executable accessibility evidence.");
    }
}
