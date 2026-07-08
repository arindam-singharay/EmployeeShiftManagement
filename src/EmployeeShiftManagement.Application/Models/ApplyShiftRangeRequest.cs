namespace EmployeeShiftManagement.Application.Models;

public record ApplyShiftRangeRequest(
    Guid ShiftDefinitionId,
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<Guid> EmployeeIds,
    string Notes);
