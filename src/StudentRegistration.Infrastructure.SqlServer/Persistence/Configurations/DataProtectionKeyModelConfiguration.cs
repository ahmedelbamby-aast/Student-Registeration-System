using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class DataProtectionKeyModelConfiguration :
    IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("DataProtectionKeys");
        builder.HasKey(key => key.Id);
        builder.Property(key => key.FriendlyName).HasMaxLength(256);
        builder.Property(key => key.Xml).IsRequired();
    }
}
