using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using CatalogueProgram = StudentRegistration.Academics.Domain.Program;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class CatalogueModelConfiguration :
    IEntityTypeConfiguration<CatalogueDraft>,
    IEntityTypeConfiguration<CatalogueVersion>,
    IEntityTypeConfiguration<CatalogueProgram>,
    IEntityTypeConfiguration<Course>,
    IEntityTypeConfiguration<CurriculumCourse>,
    IEntityTypeConfiguration<CoursePrerequisite>,
    IEntityTypeConfiguration<PolicySet>,
    IEntityTypeConfiguration<PolicyRule>,
    IEntityTypeConfiguration<ImportBatch>
{
    private const string Schema = "academics";

    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcConverter = new(
        value => value,
        value => value.HasValue
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value);

    private static readonly ValueConverter<IReadOnlyList<string>, string>
        StringListConverter = new(
            value => SerializeStrings(value),
            value => DeserializeStrings(value));

    private static readonly ValueComparer<IReadOnlyList<string>> StringListComparer = new(
        (left, right) =>
            left == null
                ? right == null
                : right != null && left.SequenceEqual(right),
        value => value == null
            ? 0
            : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
        value => value == null ? Array.Empty<string>() : value.ToArray());

    private static readonly ValueConverter<IReadOnlyList<ImportRowError>, string>
        ImportErrorsConverter = new(
            value => SerializeErrors(value),
            value => DeserializeErrors(value));

    private static readonly ValueComparer<IReadOnlyList<ImportRowError>>
        ImportErrorsComparer = new(
            (left, right) =>
                left == null
                    ? right == null
                    : right != null && left.SequenceEqual(right),
            value => value == null
                ? 0
                : value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            value => value == null ? Array.Empty<ImportRowError>() : value.ToArray());

    private static readonly ValueConverter<CatalogueDraftState, string>
        DraftStateConverter = new(
            value => ToDraftState(value),
            value => FromDraftState(value));

    private static readonly ValueConverter<CatalogueVersionState, string>
        VersionStateConverter = new(
            value => ToVersionState(value),
            value => FromVersionState(value));

    private static readonly ValueConverter<PolicySetState, string>
        PolicyStateConverter = new(
            value => ToPolicyState(value),
            value => FromPolicyState(value));

    private static readonly ValueConverter<PolicyValueType, string>
        PolicyValueTypeConverter = new(
            value => ToValueType(value),
            value => FromValueType(value));

    private static readonly ValueConverter<CatalogueSourceKind, string>
        SourceKindConverter = new(
            value => ToSourceKind(value),
            value => FromSourceKind(value));

    private static readonly ValueConverter<ImportBatchState, string>
        ImportStateConverter = new(
            value => ToImportState(value),
            value => FromImportState(value));

    public void Configure(EntityTypeBuilder<CatalogueDraft> builder)
    {
        builder.ToTable(
            "CatalogueDrafts",
            Schema,
            table => table.HasCheckConstraint(
                "CK_CatalogueDrafts_State",
                "[State] IN ('editing', 'validated', 'published', 'abandoned')"));
        ConfigureGuidKey(builder);
        builder.Property(item => item.ScopeCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.BasedOnVersionId);
        builder.Property(item => item.CanonicalContentHash).HasMaxLength(128).IsRequired();
        builder.Property(item => item.ContentJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(item => item.ValidationSummaryJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(item => item.State)
            .HasConversion(DraftStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureRowVersion(builder.Property(item => item.Version));
        builder.HasIndex(item => new { item.ScopeCode, item.State, item.Id });
        builder.HasOne<CatalogueVersion>()
            .WithMany()
            .HasForeignKey(item => item.BasedOnVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<CatalogueVersion> builder)
    {
        builder.ToTable(
            "CatalogueVersions",
            Schema,
            table => table.HasCheckConstraint(
                "CK_CatalogueVersions_State",
                "[State] IN ('published', 'superseded')"));
        ConfigureGuidKey(builder);
        builder.Property(item => item.SourceDraftId).IsRequired();
        builder.Property(item => item.SupersedesId);
        builder.Property(item => item.ScopeCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.VersionCode).HasMaxLength(100).IsRequired();
        builder.Property(item => item.SourceReference).HasMaxLength(500).IsRequired();
        ConfigureUtc(builder.Property(item => item.EffectiveFromUtc));
        ConfigureUtc(builder.Property(item => item.PublishedAtUtc));
        builder.Property(item => item.PublishedBy).HasMaxLength(200).IsRequired();
        builder.Property(item => item.State)
            .HasConversion(VersionStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureRowVersion(builder.Property(item => item.Version));
        builder.HasIndex(item => item.SourceDraftId).IsUnique();
        builder.HasIndex(item => new { item.ScopeCode, item.VersionCode }).IsUnique();
        builder.HasIndex(item => new { item.ScopeCode, item.State, item.PublishedAtUtc, item.Id });
        builder.HasOne<CatalogueDraft>()
            .WithOne()
            .HasForeignKey<CatalogueVersion>(item => item.SourceDraftId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CatalogueVersion>()
            .WithMany()
            .HasForeignKey(item => item.SupersedesId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<CatalogueProgram> builder)
    {
        builder.ToTable("Programs", Schema);
        ConfigureGuidKey(builder);
        builder.Property(item => item.CatalogueVersionId).IsRequired();
        builder.Property(item => item.Code).HasMaxLength(50).IsRequired();
        builder.Property(item => item.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.IsActive).IsRequired();
        ConfigureRowVersion(builder.Property(item => item.Version));
        builder.HasAlternateKey(item => new { item.CatalogueVersionId, item.Id });
        builder.HasIndex(item => new { item.CatalogueVersionId, item.Code }).IsUnique();
        builder.HasOne<CatalogueVersion>()
            .WithMany()
            .HasForeignKey(item => item.CatalogueVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        ConfigureProvenance(builder.OwnsOne(item => item.Provenance), "Provenance");
    }

    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable(
            "Courses",
            Schema,
            table => table.HasCheckConstraint("CK_Courses_Credits", "[Credits] = 3"));
        ConfigureGuidKey(builder);
        builder.Property(item => item.CatalogueVersionId).IsRequired();
        builder.Property(item => item.Code).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(300).IsRequired();
        builder.Property(item => item.Credits).HasPrecision(6, 2).IsRequired();
        builder.Property(item => item.IsActive).IsRequired();
        ConfigureRowVersion(builder.Property(item => item.Version));
        builder.HasAlternateKey(item => new { item.CatalogueVersionId, item.Id });
        builder.HasIndex(item => new { item.CatalogueVersionId, item.Code }).IsUnique();
        builder.HasOne<CatalogueVersion>()
            .WithMany()
            .HasForeignKey(item => item.CatalogueVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        ConfigureProvenance(builder.OwnsOne(item => item.Provenance), "Provenance");
    }

    public void Configure(EntityTypeBuilder<CurriculumCourse> builder)
    {
        builder.ToTable(
            "CurriculumCourses",
            Schema,
            table =>
            {
                table.HasCheckConstraint("CK_CurriculumCourses_Level", "[Level] > 0");
                table.HasCheckConstraint(
                    "CK_CurriculumCourses_RecommendedTerm",
                    "[RecommendedTerm] IS NULL OR [RecommendedTerm] > 0");
            });
        builder.HasKey(item => new
        {
            item.CatalogueVersionId,
            item.ProgramId,
            item.CourseId
        });
        builder.Property(item => item.Level).IsRequired();
        builder.Property(item => item.RecommendedTerm);
        builder.Property(item => item.IsRequired).IsRequired();
        builder.Property(item => item.CohortScope).HasMaxLength(50);
        builder.HasOne<CatalogueProgram>()
            .WithMany()
            .HasForeignKey(item => new { item.CatalogueVersionId, item.ProgramId })
            .HasPrincipalKey(item => new { item.CatalogueVersionId, item.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(item => new { item.CatalogueVersionId, item.CourseId })
            .HasPrincipalKey(item => new { item.CatalogueVersionId, item.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new
        {
            item.ProgramId,
            item.CohortScope,
            item.RecommendedTerm,
            item.CourseId
        });
        ConfigureProvenance(builder.OwnsOne(item => item.Provenance), "Provenance");
    }

    public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
    {
        builder.ToTable(
            "CoursePrerequisites",
            Schema,
            table => table.HasCheckConstraint(
                "CK_CoursePrerequisites_NotSelf",
                "[CourseId] <> [RequiredCourseId]"));
        builder.HasKey(item => new
        {
            item.CatalogueVersionId,
            item.CourseId,
            item.RequiredCourseId
        });
        builder.Property(item => item.MinimumGrade).HasMaxLength(20);
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(item => new { item.CatalogueVersionId, item.CourseId })
            .HasPrincipalKey(item => new { item.CatalogueVersionId, item.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(item => new { item.CatalogueVersionId, item.RequiredCourseId })
            .HasPrincipalKey(item => new { item.CatalogueVersionId, item.Id })
            .OnDelete(DeleteBehavior.Restrict);
        ConfigureProvenance(builder.OwnsOne(item => item.Provenance), "Provenance");
    }

    public void Configure(EntityTypeBuilder<PolicySet> builder)
    {
        builder.ToTable(
            "PolicySets",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_PolicySets_State",
                    "[State] IN ('draft', 'validated', 'published', 'superseded')");
                table.HasCheckConstraint(
                    "CK_PolicySets_EffectiveRange",
                    "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
            });
        ConfigureGuidKey(builder);
        builder.Property(item => item.VersionCode).HasMaxLength(100).IsRequired();
        builder.Property(item => item.TermId).IsRequired();
        builder.Property(item => item.ProgramId);
        builder.Property(item => item.ScopeCode).HasMaxLength(50).IsRequired();
        ConfigureUtc(builder.Property(item => item.EffectiveFromUtc));
        ConfigureNullableUtc(builder.Property(item => item.EffectiveToUtc));
        builder.Property(item => item.State)
            .HasConversion(PolicyStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureRowVersion(builder.Property(item => item.VersionToken));
        builder.HasIndex(item => new { item.ScopeCode, item.VersionCode }).IsUnique();
        builder.HasIndex(item => new { item.ScopeCode, item.State, item.EffectiveFromUtc, item.Id });
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(item => item.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CatalogueProgram>()
            .WithMany()
            .HasForeignKey(item => item.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<PolicyRule> builder)
    {
        builder.ToTable("PolicyRules", Schema);
        ConfigureGuidKey(builder);
        builder.Property(item => item.PolicySetId).IsRequired();
        builder.Property(item => item.Code).HasMaxLength(100).IsRequired();
        builder.Property(item => item.ReasonCode).HasMaxLength(100).IsRequired();
        builder.Property(item => item.ValueType)
            .HasConversion(PolicyValueTypeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(item => item.Value).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(item => item.SourceReference).HasMaxLength(500).IsRequired();
        builder.Property(item => item.SourceKind)
            .HasConversion(SourceKindConverter)
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(item => new { item.PolicySetId, item.Code }).IsUnique();
        builder.HasOne<PolicySet>()
            .WithMany()
            .HasForeignKey(item => item.PolicySetId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable(
            "ImportBatches",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ImportBatches_State",
                    "[State] IN ('uploaded', 'validating', 'invalid', 'validated', " +
                    "'publishing', 'published', 'failed')");
                table.HasCheckConstraint(
                    "CK_ImportBatches_SyntheticFieldCount",
                    "[SyntheticFieldCount] >= 0");
            });
        ConfigureGuidKey(builder);
        builder.Property(item => item.CatalogueDraftId).IsRequired();
        builder.Property(item => item.SourceReference).HasMaxLength(500).IsRequired();
        ConfigureUtc(builder.Property(item => item.AccessedAtUtc));
        builder.Property(item => item.ContentHash).HasMaxLength(128).IsRequired();
        builder.Property(item => item.SyntheticFieldCount).IsRequired();
        builder.Property(item => item.State)
            .HasConversion(ImportStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        var errors = builder.Property(item => item.Errors)
            .HasConversion(ImportErrorsConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        errors.Metadata.SetValueComparer(ImportErrorsComparer);
        builder.Property(item => item.PublishedVersionId);
        ConfigureRowVersion(builder.Property(item => item.Version));
        builder.HasIndex(item => new { item.CatalogueDraftId, item.State });
        builder.HasOne<CatalogueDraft>()
            .WithMany()
            .HasForeignKey(item => item.CatalogueDraftId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CatalogueVersion>()
            .WithMany()
            .HasForeignKey(item => item.PublishedVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureProvenance<TEntity>(
        OwnedNavigationBuilder<TEntity, CatalogueFieldProvenance> owned,
        string prefix)
        where TEntity : class
    {
        owned.Property(item => item.SourceReference)
            .HasColumnName($"{prefix}SourceReference")
            .HasMaxLength(500)
            .IsRequired();
        owned.Property(item => item.AccessedOn)
            .HasColumnName($"{prefix}AccessedOn")
            .HasColumnType("date")
            .IsRequired();
        owned.Property(item => item.SourceKind)
            .HasColumnName($"{prefix}SourceKind")
            .HasConversion(SourceKindConverter)
            .HasMaxLength(30)
            .IsRequired();
        var fields = owned.Property(item => item.SyntheticFields)
            .HasColumnName($"{prefix}SyntheticFieldsJson")
            .HasConversion(StringListConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        fields.Metadata.SetValueComparer(StringListComparer);
    }

    private static void ConfigureGuidKey<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id").ValueGeneratedNever();
    }

    private static void ConfigureRowVersion(PropertyBuilder<byte[]> property) =>
        property.IsRowVersion().IsRequired();

    private static void ConfigureUtc(PropertyBuilder<DateTime> property) =>
        property.HasConversion(UtcConverter).HasColumnType("datetime2").IsRequired();

    private static void ConfigureNullableUtc(PropertyBuilder<DateTime?> property) =>
        property.HasConversion(NullableUtcConverter).HasColumnType("datetime2");

    private static string SerializeStrings(IReadOnlyList<string> value) =>
        JsonSerializer.Serialize(value);

    private static IReadOnlyList<string> DeserializeStrings(string value) =>
        JsonSerializer.Deserialize<string[]>(value) ?? [];

    private static string SerializeErrors(IReadOnlyList<ImportRowError> value) =>
        JsonSerializer.Serialize(value);

    private static IReadOnlyList<ImportRowError> DeserializeErrors(string value) =>
        JsonSerializer.Deserialize<ImportRowError[]>(value) ?? [];

    private static string ToDraftState(CatalogueDraftState value) => value switch
    {
        CatalogueDraftState.Editing => "editing",
        CatalogueDraftState.Validated => "validated",
        CatalogueDraftState.Published => "published",
        CatalogueDraftState.Abandoned => "abandoned",
        _ => throw new InvalidOperationException("Unknown catalogue-draft state.")
    };

    private static CatalogueDraftState FromDraftState(string value) => value switch
    {
        "editing" => CatalogueDraftState.Editing,
        "validated" => CatalogueDraftState.Validated,
        "published" => CatalogueDraftState.Published,
        "abandoned" => CatalogueDraftState.Abandoned,
        _ => throw new InvalidOperationException("Unknown persisted catalogue-draft state.")
    };

    private static string ToVersionState(CatalogueVersionState value) => value switch
    {
        CatalogueVersionState.Published => "published",
        CatalogueVersionState.Superseded => "superseded",
        _ => throw new InvalidOperationException("Unknown catalogue-version state.")
    };

    private static CatalogueVersionState FromVersionState(string value) => value switch
    {
        "published" => CatalogueVersionState.Published,
        "superseded" => CatalogueVersionState.Superseded,
        _ => throw new InvalidOperationException("Unknown persisted catalogue-version state.")
    };

    private static string ToPolicyState(PolicySetState value) => value switch
    {
        PolicySetState.Draft => "draft",
        PolicySetState.Validated => "validated",
        PolicySetState.Published => "published",
        PolicySetState.Superseded => "superseded",
        _ => throw new InvalidOperationException("Unknown policy-set state.")
    };

    private static PolicySetState FromPolicyState(string value) => value switch
    {
        "draft" => PolicySetState.Draft,
        "validated" => PolicySetState.Validated,
        "published" => PolicySetState.Published,
        "superseded" => PolicySetState.Superseded,
        _ => throw new InvalidOperationException("Unknown persisted policy-set state.")
    };

    private static string ToImportState(ImportBatchState value) => value switch
    {
        ImportBatchState.Uploaded => "uploaded",
        ImportBatchState.Validating => "validating",
        ImportBatchState.Invalid => "invalid",
        ImportBatchState.Validated => "validated",
        ImportBatchState.Publishing => "publishing",
        ImportBatchState.Published => "published",
        ImportBatchState.Failed => "failed",
        _ => throw new InvalidOperationException("Unknown import-batch state.")
    };

    private static ImportBatchState FromImportState(string value) => value switch
    {
        "uploaded" => ImportBatchState.Uploaded,
        "validating" => ImportBatchState.Validating,
        "invalid" => ImportBatchState.Invalid,
        "validated" => ImportBatchState.Validated,
        "publishing" => ImportBatchState.Publishing,
        "published" => ImportBatchState.Published,
        "failed" => ImportBatchState.Failed,
        _ => throw new InvalidOperationException("Unknown persisted import-batch state.")
    };

    private static string ToValueType(PolicyValueType value) => value switch
    {
        PolicyValueType.Number => "number",
        PolicyValueType.Boolean => "boolean",
        PolicyValueType.String => "string",
        PolicyValueType.StringList => "string-list",
        _ => throw new InvalidOperationException("Unknown policy value type.")
    };

    private static PolicyValueType FromValueType(string value) => value switch
    {
        "number" => PolicyValueType.Number,
        "boolean" => PolicyValueType.Boolean,
        "string" => PolicyValueType.String,
        "string-list" => PolicyValueType.StringList,
        _ => throw new InvalidOperationException("Unknown persisted policy value type.")
    };

    private static string ToSourceKind(CatalogueSourceKind value) => value switch
    {
        CatalogueSourceKind.OfficialSource => "official-source",
        CatalogueSourceKind.SyntheticDemo => "synthetic-demo",
        _ => throw new InvalidOperationException("Unknown catalogue source kind.")
    };

    private static CatalogueSourceKind FromSourceKind(string value) => value switch
    {
        "official-source" => CatalogueSourceKind.OfficialSource,
        "synthetic-demo" => CatalogueSourceKind.SyntheticDemo,
        _ => throw new InvalidOperationException("Unknown persisted catalogue source kind.")
    };
}
