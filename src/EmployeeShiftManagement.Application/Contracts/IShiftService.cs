using EmployeeShiftManagement.Application.Models;

namespace EmployeeShiftManagement.Application.Contracts;

public interface IShiftService
{
    Task<IReadOnlyList<ShiftDefinitionModel>> GetShiftDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<ShiftDefinitionModel> CreateShiftDefinitionAsync(CreateShiftDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<ShiftDefinitionModel?> UpdateShiftDefinitionAsync(UpdateShiftDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShiftModel>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShiftModel>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<int> ApplyForDateRangeAsync(ApplyShiftRangeRequest request, CancellationToken cancellationToken = default);
    Task<ShiftModel> UpsertAsync(UpsertShiftRequest request, CancellationToken cancellationToken = default);
}
