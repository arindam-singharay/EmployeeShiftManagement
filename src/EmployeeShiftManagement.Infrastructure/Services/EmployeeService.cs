using EmployeeShiftManagement.Application.Contracts;
using EmployeeShiftManagement.Application.Models;
using EmployeeShiftManagement.Domain.Entities;
using EmployeeShiftManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Services;

public class EmployeeService(EmployeeShiftDbContext dbContext) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .OrderBy(x => x.FullName)
            .Select(x => new EmployeeModel(x.Id, x.EmployeeCode, x.FullName, x.Department, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeModel> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employeeCode = request.EmployeeCode.Trim();
        var fullName = request.FullName.Trim();
        var department = request.Department.Trim();

        if (string.IsNullOrWhiteSpace(employeeCode) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(department))
        {
            throw new InvalidOperationException("Employee code, full name, and department are required.");
        }

        var duplicateCode = await dbContext.Employees
            .AsNoTracking()
            .AnyAsync(x => x.EmployeeCode == employeeCode, cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException("Employee code already exists.");
        }

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FullName = fullName,
            Department = department,
            IsActive = true
        };

        dbContext.Employees.Add(employee);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }

        return new EmployeeModel(
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            employee.IsActive);
    }

    public async Task<EmployeeModel?> UpdateAsync(UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        var employeeCode = request.EmployeeCode.Trim();
        var fullName = request.FullName.Trim();
        var department = request.Department.Trim();

        if (string.IsNullOrWhiteSpace(employeeCode) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(department))
        {
            throw new InvalidOperationException("Employee code, full name, and department are required.");
        }

        var duplicateCode = await dbContext.Employees
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.EmployeeCode == employeeCode, cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException("Employee code already exists.");
        }

        employee.EmployeeCode = employeeCode;
        employee.FullName = fullName;
        employee.Department = department;
        employee.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionHelper.ToUserFriendlyException(ex);
        }

        return new EmployeeModel(
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            employee.IsActive);
    }
}
