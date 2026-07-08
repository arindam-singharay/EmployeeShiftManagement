namespace EmployeeShiftManagement.Application.Models;

public record EmployeeModel(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Department,
    bool IsActive);
