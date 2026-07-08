namespace EmployeeShiftManagement.Application.Models;

public record UpdateEmployeeRequest(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Department,
    bool IsActive);
