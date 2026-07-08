using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Services;

internal static class DbUpdateExceptionHelper
{
    public static InvalidOperationException ToUserFriendlyException(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            // 2601 and 2627 are SQL Server duplicate key violations.
            if (sqlException.Number is 2601 or 2627)
            {
                return new InvalidOperationException("A record with the same unique value already exists.", exception);
            }

            // 547 indicates FK/check constraint violations in SQL Server.
            if (sqlException.Number == 547)
            {
                return new InvalidOperationException("The operation violates a related data constraint.", exception);
            }
        }

        return new InvalidOperationException("Unable to save changes due to invalid or conflicting data.", exception);
    }
}
