using EmployeeShiftManagement.Application.Models;

namespace EmployeeShiftManagement.Application.Contracts;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeModel> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeModel?> UpdateAsync(UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
}
