using EmployeeShiftManagement.Application.Contracts;
using EmployeeShiftManagement.Application.Models;
using EmployeeShiftManagement.Domain.Entities;
using EmployeeShiftManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Services;

public class ShiftService(EmployeeShiftDbContext dbContext) : IShiftService
{
    public async Task<IReadOnlyList<ShiftDefinitionModel>> GetShiftDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ShiftDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ShiftDefinitionModel(x.Id, x.Name, x.StartTime, x.EndTime, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ShiftDefinitionModel> CreateShiftDefinitionAsync(CreateShiftDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Shift name is required.");
        }

        var duplicateName = await dbContext.ShiftDefinitions
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);

        if (duplicateName)
        {
            throw new InvalidOperationException("Shift name already exists.");
        }

        var shiftDefinition = new ShiftDefinition
        {
            Name = name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true
        };

        dbContext.ShiftDefinitions.Add(shiftDefinition);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }

        return new ShiftDefinitionModel(shiftDefinition.Id, shiftDefinition.Name, shiftDefinition.StartTime, shiftDefinition.EndTime, shiftDefinition.IsActive);
    }

    public async Task<ShiftDefinitionModel?> UpdateShiftDefinitionAsync(UpdateShiftDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var shiftDefinition = await dbContext.ShiftDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (shiftDefinition is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Shift name is required.");
        }

        var duplicateName = await dbContext.ShiftDefinitions
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Name == name, cancellationToken);

        if (duplicateName)
        {
            throw new InvalidOperationException("Shift name already exists.");
        }

        shiftDefinition.Name = name;
        shiftDefinition.StartTime = request.StartTime;
        shiftDefinition.EndTime = request.EndTime;
        shiftDefinition.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }

        return new ShiftDefinitionModel(shiftDefinition.Id, shiftDefinition.Name, shiftDefinition.StartTime, shiftDefinition.EndTime, shiftDefinition.IsActive);
    }

    public async Task<IReadOnlyList<ShiftModel>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await GetByDateRangeAsync(start, end, cancellationToken);
    }

    public async Task<IReadOnlyList<ShiftModel>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shifts
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.ShiftDefinition)
            .Where(x => x.ShiftDate >= from && x.ShiftDate <= to)
            .OrderBy(x => x.ShiftDate)
            .ThenBy(x => x.ShiftDefinition!.StartTime)
            .ThenBy(x => x.Employee!.FullName)
            .Select(x => new ShiftModel(
                x.Id,
                x.ShiftDefinitionId,
                x.EmployeeId,
                x.Employee != null ? x.Employee.FullName : string.Empty,
                x.ShiftDate,
                x.ShiftDefinition != null ? x.ShiftDefinition.StartTime : TimeOnly.MinValue,
                x.ShiftDefinition != null ? x.ShiftDefinition.EndTime : TimeOnly.MinValue,
                x.ShiftDefinition != null ? x.ShiftDefinition.Name : string.Empty,
                x.Notes,
                x.ShiftDefinition != null
                    ? GetWorkedHours(x.ShiftDate, x.ShiftDefinition.StartTime, x.ShiftDefinition.EndTime)
                    : 0))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ApplyForDateRangeAsync(ApplyShiftRangeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EmployeeIds.Count == 0)
        {
            throw new InvalidOperationException("Select at least one employee.");
        }

        var shiftExists = await dbContext.ShiftDefinitions
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ShiftDefinitionId, cancellationToken);

        if (!shiftExists)
        {
            throw new InvalidOperationException("Selected shift definition does not exist.");
        }

        var distinctEmployeeIds = request.EmployeeIds.Distinct().ToList();
        var existingEmployeeIds = await dbContext.Employees
            .AsNoTracking()
            .Where(x => distinctEmployeeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (existingEmployeeIds.Count != distinctEmployeeIds.Count)
        {
            throw new InvalidOperationException("One or more selected employees no longer exist.");
        }

        var notes = (request.Notes ?? string.Empty).Trim();
        if (notes.Length > 300)
        {
            throw new InvalidOperationException("Notes cannot exceed 300 characters.");
        }

        var from = request.FromDate <= request.ToDate ? request.FromDate : request.ToDate;
        var to = request.FromDate <= request.ToDate ? request.ToDate : request.FromDate;

        var created = 0;
        var allDates = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1)
            .Select(offset => from.AddDays(offset));

        foreach (var date in allDates)
        {
            foreach (var employeeId in distinctEmployeeIds)
            {
                var existing = await dbContext.Shifts.FirstOrDefaultAsync(
                    x => x.EmployeeId == employeeId
                        && x.ShiftDate == date
                        && x.ShiftDefinitionId == request.ShiftDefinitionId,
                    cancellationToken);

                if (existing is not null)
                {
                    existing.Notes = notes;
                    continue;
                }

                dbContext.Shifts.Add(new Shift
                {
                    EmployeeId = employeeId,
                    ShiftDate = date,
                    ShiftDefinitionId = request.ShiftDefinitionId,
                    Notes = notes
                });

                created++;
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }
        return created;
    }

    public async Task<ShiftModel> UpsertAsync(UpsertShiftRequest request, CancellationToken cancellationToken = default)
    {
        var notes = (request.Notes ?? string.Empty).Trim();
        if (notes.Length > 300)
        {
            throw new InvalidOperationException("Notes cannot exceed 300 characters.");
        }

        Shift? shift = null;

        if (request.ShiftId is Guid shiftId)
        {
            shift = await dbContext.Shifts.FirstOrDefaultAsync(x => x.Id == shiftId, cancellationToken);
        }

        shift ??= await dbContext.Shifts.FirstOrDefaultAsync(
            x => x.EmployeeId == request.EmployeeId && x.ShiftDate == request.ShiftDate && x.ShiftDefinitionId == request.ShiftDefinitionId,
            cancellationToken);

        if (shift is null)
        {
            shift = new Shift();
            dbContext.Shifts.Add(shift);
        }

        var shiftDefinition = await dbContext.ShiftDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.ShiftDefinitionId, cancellationToken)
            ?? throw new InvalidOperationException("Shift definition not found.");

        var employeeExists = await dbContext.Employees
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            throw new InvalidOperationException("Employee not found.");
        }

        shift.ShiftDefinitionId = request.ShiftDefinitionId;
        shift.EmployeeId = request.EmployeeId;
        shift.ShiftDate = request.ShiftDate;
        shift.Notes = notes;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }

        var employeeName = await dbContext.Employees
            .Where(x => x.Id == request.EmployeeId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new ShiftModel(
            shift.Id,
            shift.ShiftDefinitionId,
            shift.EmployeeId,
            employeeName,
            shift.ShiftDate,
            shiftDefinition.StartTime,
            shiftDefinition.EndTime,
            shiftDefinition.Name,
            shift.Notes,
            GetWorkedHours(shift.ShiftDate, shiftDefinition.StartTime, shiftDefinition.EndTime));
    }

    private static double GetWorkedHours(DateOnly shiftDate, TimeOnly startTime, TimeOnly endTime)
    {
        var start = shiftDate.ToDateTime(startTime);
        var end = shiftDate.ToDateTime(endTime);
        if (end <= start)
        {
            end = end.AddDays(1);
        }

        return (end - start).TotalHours;
    }
}
