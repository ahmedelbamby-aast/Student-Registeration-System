using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Infrastructure.SqlServer.DataProtection;

public static class SqlDataProtectionKeyRepository
{
    public static IDataProtectionBuilder PersistKeysToSqlServer(
        this IDataProtectionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.PersistKeysToDbContext<StudentRegistrationDbContext>();
    }
}
