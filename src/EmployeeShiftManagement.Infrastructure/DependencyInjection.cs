using EmployeeShiftManagement.Application.Contracts;
using EmployeeShiftManagement.Infrastructure.Persistence;
using EmployeeShiftManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeShiftManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=EmployeeShiftManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<EmployeeShiftDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
