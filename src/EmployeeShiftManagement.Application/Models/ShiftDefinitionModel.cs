namespace EmployeeShiftManagement.Application.Models;

public record ShiftDefinitionModel(
    Guid Id,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);
