namespace EmployeeShiftManagement.Application.Models;

public record CreateEmployeeRequest(
    string EmployeeCode,
    string FullName,
    string Department);
