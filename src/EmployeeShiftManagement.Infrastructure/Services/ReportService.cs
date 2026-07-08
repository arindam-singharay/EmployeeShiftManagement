using ClosedXML.Excel;
using EmployeeShiftManagement.Application.Contracts;
using EmployeeShiftManagement.Application.Models;
using EmployeeShiftManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Services;

public class ReportService(EmployeeShiftDbContext dbContext) : IReportService
{
    public async Task<ShiftReportResult> BuildAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Shifts
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.ShiftDefinition)
            .Where(x => x.ShiftDate >= from && x.ShiftDate <= to)
            .OrderBy(x => x.ShiftDate)
            .ThenBy(x => x.Employee!.FullName)
            .Select(x => new ShiftReportRow(
                x.ShiftDate,
                x.Employee != null ? x.Employee.EmployeeCode : string.Empty,
                x.Employee != null ? x.Employee.FullName : string.Empty,
                x.Employee != null ? x.Employee.Department : string.Empty,
                x.ShiftDefinition != null ? x.ShiftDefinition.Name : string.Empty,
                x.ShiftDefinition != null ? x.ShiftDefinition.StartTime : TimeOnly.MinValue,
                x.ShiftDefinition != null ? x.ShiftDefinition.EndTime : TimeOnly.MinValue,
                x.ShiftDefinition != null
                    ? ((x.ShiftDefinition.EndTime <= x.ShiftDefinition.StartTime
                        ? x.ShiftDate.ToDateTime(x.ShiftDefinition.EndTime).AddDays(1)
                        : x.ShiftDate.ToDateTime(x.ShiftDefinition.EndTime))
                      - x.ShiftDate.ToDateTime(x.ShiftDefinition.StartTime)).TotalHours
                    : 0))
            .ToListAsync(cancellationToken);

        return new ShiftReportResult(
            from,
            to,
            rows,
            rows.Sum(x => x.WorkedHours));
    }

    public Task<byte[]> ExportExcelAsync(ShiftReportResult report, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Shift Report");

        worksheet.Cell(1, 1).Value = "Date";
        worksheet.Cell(1, 2).Value = "Employee Code";
        worksheet.Cell(1, 3).Value = "Employee Name";
        worksheet.Cell(1, 4).Value = "Department";
        worksheet.Cell(1, 5).Value = "Shift";
        worksheet.Cell(1, 6).Value = "Start";
        worksheet.Cell(1, 7).Value = "End";
        worksheet.Cell(1, 8).Value = "Hours";

        var rowIndex = 2;
        foreach (var row in report.Rows)
        {
            worksheet.Cell(rowIndex, 1).Value = row.ShiftDate.ToString("yyyy-MM-dd");
            worksheet.Cell(rowIndex, 2).Value = row.EmployeeCode;
            worksheet.Cell(rowIndex, 3).Value = row.EmployeeName;
            worksheet.Cell(rowIndex, 4).Value = row.Department;
            worksheet.Cell(rowIndex, 5).Value = row.ShiftName;
            worksheet.Cell(rowIndex, 6).Value = row.StartTime.ToString("HH:mm");
            worksheet.Cell(rowIndex, 7).Value = row.EndTime.ToString("HH:mm");
            worksheet.Cell(rowIndex, 8).Value = row.WorkedHours;
            rowIndex++;
        }

        worksheet.Cell(rowIndex + 1, 7).Value = "Total Hours";
        worksheet.Cell(rowIndex + 1, 8).Value = report.TotalHours;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
