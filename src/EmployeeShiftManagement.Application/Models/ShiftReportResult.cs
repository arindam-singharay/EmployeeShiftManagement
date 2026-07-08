namespace EmployeeShiftManagement.Application.Models;

public record ShiftReportResult(
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<ShiftReportRow> Rows,
    double TotalHours);
