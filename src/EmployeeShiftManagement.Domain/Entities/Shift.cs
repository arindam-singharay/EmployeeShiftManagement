namespace EmployeeShiftManagement.Domain.Entities;

public class Shift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShiftDefinitionId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Employee? Employee { get; set; }
    public ShiftDefinition? ShiftDefinition { get; set; }
}
