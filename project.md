# Employee Shift Management System - Duplicate Key Exception Fix

## Issue Description

**Exception Type:** `Microsoft.EntityFrameworkCore.DbUpdateException`

**Error Message:** 
```
Cannot insert duplicate key row in object 'dbo.Employees' with unique index 'IX_Employees_EmployeeCode'. 
The duplicate key value is (EAL001).
```

**Severity:** High - Prevents employee creation and updates when duplicate employee codes are used

---

## Root Cause Analysis

### Problem
The application attempts to create or update employees without validating whether the `EmployeeCode` already exists in the database. The database enforces a unique constraint on the `EmployeeCode` column through the index `IX_Employees_EmployeeCode`, but the application layer lacks corresponding validation logic.

### Affected Code
**File:** `src/EmployeeShiftManagement.Infrastructure/Services/EmployeeService.cs`

**Methods:**
1. `CreateAsync(CreateEmployeeRequest request, ...)` - Line 19-38
2. `UpdateAsync(UpdateEmployeeRequest request, ...)` - Line 40-54

### Database Schema
```csharp
// From EmployeeShiftDbContext.cs
entity.HasIndex(x => x.EmployeeCode).IsUnique();
```

---

## Impact

### Current Behavior
- User attempts to create/update an employee with a duplicate employee code
- Application throws unhandled `DbUpdateException` 
- Generic error message displayed to user
- Poor user experience with technical error details exposed

### Expected Behavior
- Application validates employee code uniqueness before database operation
- User receives clear, actionable error message
- Exception handling provides meaningful feedback
- Prevents unnecessary database calls

---

## Proposed Solution

### 1. Add Duplicate Validation in CreateAsync
Check if the employee code exists before attempting to insert:

```csharp
public async Task<EmployeeModel> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
{
	// Validate employee code uniqueness
	var exists = await dbContext.Employees
		.AnyAsync(e => e.EmployeeCode == request.EmployeeCode.Trim(), cancellationToken);

	if (exists)
	{
		throw new InvalidOperationException($"Employee code '{request.EmployeeCode}' already exists.");
	}

	var employee = new Employee
	{
		EmployeeCode = request.EmployeeCode.Trim(),
		FullName = request.FullName.Trim(),
		Department = request.Department.Trim(),
		IsActive = true
	};

	dbContext.Employees.Add(employee);
	await dbContext.SaveChangesAsync(cancellationToken);

	return new EmployeeModel(
		employee.Id,
		employee.EmployeeCode,
		employee.FullName,
		employee.Department,
		employee.IsActive);
}
```

### 2. Add Duplicate Validation in UpdateAsync
Check if the new employee code conflicts with other employees (excluding current):

```csharp
public async Task<EmployeeModel?> UpdateAsync(UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
{
	var employee = await dbContext.Employees.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
	if (employee is null)
	{
		return null;
	}

	// Validate employee code uniqueness (excluding current employee)
	var codeExists = await dbContext.Employees
		.AnyAsync(e => e.EmployeeCode == request.EmployeeCode.Trim() && e.Id != request.Id, cancellationToken);

	if (codeExists)
	{
		throw new InvalidOperationException($"Employee code '{request.EmployeeCode}' already exists.");
	}

	employee.EmployeeCode = request.EmployeeCode.Trim();
	employee.FullName = request.FullName.Trim();
	employee.Department = request.Department.Trim();
	employee.IsActive = request.IsActive;

	await dbContext.SaveChangesAsync(cancellationToken);

	return new EmployeeModel(
		employee.Id,
		employee.EmployeeCode,
		employee.FullName,
		employee.Department,
		employee.IsActive);
}
```

### 3. UI Error Handling Enhancement (Optional)
Update the Blazor component to catch and display validation exceptions:

```csharp
// In Employees.razor.cs
try
{
	await EmployeeService.CreateAsync(new CreateEmployeeRequest(...));
	message = $"Employee {form.FullName} created successfully.";
}
catch (InvalidOperationException ex)
{
	message = $"Error: {ex.Message}";
	isError = true;
}
catch (Exception ex)
{
	message = "An unexpected error occurred. Please try again.";
	isError = true;
}
```

---

## Alternative Solutions

### Option A: Use Result Pattern
Instead of throwing exceptions, return a `Result<EmployeeModel>` type with success/failure status and validation messages.

### Option B: Fluent Validation
Implement FluentValidation library for comprehensive validation pipeline.

### Option C: Custom Business Exception
Create a `DuplicateEmployeeCodeException` for more specific exception handling.

---

## Testing Recommendations

### Unit Tests
- Test `CreateAsync` with duplicate employee code
- Test `UpdateAsync` with duplicate employee code
- Test `UpdateAsync` with same employee code (should succeed)
- Test successful creation and update scenarios

### Integration Tests
- Verify database constraint is enforced
- Test concurrent creation attempts with same code
- Validate error messages are user-friendly

---

## Files to Modify

1. `src/EmployeeShiftManagement.Infrastructure/Services/EmployeeService.cs`
   - Add duplicate validation in `CreateAsync`
   - Add duplicate validation in `UpdateAsync`

2. `src/EmployeeShiftManagement.Web/Components/Pages/Employees.razor` (Optional)
   - Add try-catch block in `SaveEmployeeAsync`
   - Display validation error messages

---

## Technical Details

**Stack Trace Context:**
```
Thread ID: 38044
[1] External Code
[2] EmployeeService.CreateAsync (Line 30 - SaveChangesAsync)
[3] Employees.SaveEmployeeAsync (Lines 147-150)
```

**Duplicate Employee Code:** `EAL001`

**Database Table:** `dbo.Employees`

**Unique Index:** `IX_Employees_EmployeeCode`

---

## References

- [EF Core Handling Concurrency Conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [SQL Server Unique Indexes](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/create-unique-indexes)
- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

## Status
- [x] Issue Identified
- [x] Root Cause Analyzed
- [x] Solution Proposed
- [ ] Implementation
- [ ] Testing
- [ ] Code Review
- [ ] Deployment

---

**Created:** 2025
**Last Updated:** 2025
**Priority:** High
**Category:** Bug Fix / Data Validation
