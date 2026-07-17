using StudentRegistration.AcceptanceTests.Specs.Spec010;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec017;

public sealed class AC_5Tests
{
    [Fact]
    public async Task Bounded_owner_command_consumes_availability_as_read_only_planning_input_without_admin_override()
    {
        // Given an authorized Admin filters a large owner-managed list, previews
        // publication, and reads staff-declared availability for planning.
        var offeringStore = new OfferingStoreFake();
        var offeringService = new OfferingService(offeringStore);
        var availabilityStore = new ResourceAvailabilityStoreFake();
        var availabilityService = new ResourceAvailabilityService(availabilityStore);
        var publicationStore = new PublicationStoreFake();
        var publicationTransaction = new PublicationTransactionFake(publicationStore);
        var publicationService = new OfferingPublicationService(
            new OfferingPublicationValidator(),
            publicationTransaction);

        // When the bounded read and confirmed feature-owner mutation execute.
        var page = await offeringService.ListAdminOfferingsAsync(
            new AdminOfferingQuery(
                TermId: null,
                State: "draft",
                Query: "csc",
                Page: 1,
                PageSize: 100,
                Sort: "courseCode,id"));
        var oversized = await offeringService.ListAdminOfferingsAsync(
            new AdminOfferingQuery(null, null, null, 1, 101, null));
        var declaredAvailability = await availabilityService.GetAdminPlanningInputAsync(
            availabilityStore.OwnerStaffId,
            availabilityStore.TermId);
        Assert.NotNull(declaredAvailability);

        // Import means copying the staff-owned version/ranges into an offering
        // validation input. The owner aggregate is not mutated.
        publicationStore.Snapshot = Spec010Scenario.Publication(
            staffVersion: declaredAvailability!.RowVersion.ToArray());
        var preview = await new OfferingPublicationValidator().ValidateAsync(
            Spec010Scenario.ValidateCommand(
                publicationStore.Snapshot,
                expectedStaffVersion: declaredAvailability.RowVersion.ToArray()),
            publicationStore);
        var publication = await publicationService.PublishAsync(
            Spec010Scenario.PublishCommand(
                publicationStore.Snapshot,
                requestId: "spec017-ac5-confirm"));

        // Then paging remains bounded/parameterized, preview and confirmation
        // remain owner-validated/audited, and availability remains read-only.
        Assert.Equal(OfferingOutcome.Found, page.Outcome);
        Assert.Equal(100, page.Page!.PageSize);
        Assert.Equal(250, page.Page.TotalCount);
        Assert.Equal("courseCode,id", page.Page.Sort);
        Assert.Equal(OfferingOutcome.ValidationError, oversized.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", oversized.ErrorCode);
        Assert.True(preview.Valid);
        Assert.Equal(OfferingPublicationOutcome.Published, publication.Outcome);
        Assert.Single(publicationTransaction.Audits);
        Assert.Empty(availabilityStore.AvailabilityWrites);
        Assert.Equal([1], availabilityStore.AvailabilityVersion);
        Assert.Equal(declaredAvailability.Ranges, (await availabilityService
            .GetAdminPlanningInputAsync(
                availabilityStore.OwnerStaffId,
                availabilityStore.TermId))!.Ranges);

        // And no Admin availability correction/override endpoint, permission,
        // editable workflow, notification, or correction-audit path exists.
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpoints,
            "MapGet(",
            "\"/api/admin/staff-availability\"",
            "Page<StaffTermAvailabilityDto>",
            "MaximumPageSize = 100");
        Assert.DoesNotContain(
            "MapPut(\"/api/admin/staff-availability",
            endpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MapPost(\"/api/admin/staff-availability",
            endpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MapPatch(\"/api/admin/staff-availability",
            endpoints,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "AdminAvailabilityOverride",
            endpoints,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "AdminAvailabilityCorrection",
            endpoints,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "AvailabilityCorrectionNotification",
            endpoints,
            StringComparison.OrdinalIgnoreCase);

        var applicationMethods = typeof(ResourceAvailabilityService)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(ResourceAvailabilityService))
            .Select(method => method.Name)
            .ToArray();
        Assert.DoesNotContain(applicationMethods, method =>
            method.Contains("AdminOverride", StringComparison.OrdinalIgnoreCase)
            || method.Contains("AdminCorrect", StringComparison.OrdinalIgnoreCase)
            || method.Contains("AdminReplace", StringComparison.OrdinalIgnoreCase));
    }
}
