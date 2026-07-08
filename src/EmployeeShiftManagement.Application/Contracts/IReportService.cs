using EmployeeShiftManagement.Application.Models;

namespace EmployeeShiftManagement.Application.Contracts;

public interface IReportService
{
    Task<ShiftReportResult> BuildAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(ShiftReportResult report, CancellationToken cancellationToken = default);
}
