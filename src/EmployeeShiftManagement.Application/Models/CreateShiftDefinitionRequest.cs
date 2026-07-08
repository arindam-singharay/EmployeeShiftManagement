namespace EmployeeShiftManagement.Application.Models;

public record CreateShiftDefinitionRequest(
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime);
