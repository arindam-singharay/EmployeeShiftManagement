namespace EmployeeShiftManagement.Application.Models;

public record ShiftModel(
    Guid Id,
    Guid ShiftDefinitionId,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly ShiftDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ShiftName,
    string Notes,
    double WorkedHours);
