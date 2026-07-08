namespace EmployeeShiftManagement.Application.Models;

public record ShiftReportRow(
    DateOnly ShiftDate,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    double WorkedHours);
