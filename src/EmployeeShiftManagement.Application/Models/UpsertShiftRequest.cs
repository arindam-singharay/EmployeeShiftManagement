namespace EmployeeShiftManagement.Application.Models;

public record UpsertShiftRequest(
    Guid? ShiftId,
    Guid ShiftDefinitionId,
    Guid EmployeeId,
    DateOnly ShiftDate,
    string Notes);
