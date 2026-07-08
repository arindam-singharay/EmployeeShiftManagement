namespace EmployeeShiftManagement.Application.Models;

public record UpdateShiftDefinitionRequest(
    Guid Id,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);
