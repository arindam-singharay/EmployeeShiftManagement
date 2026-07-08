using EmployeeShiftManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Persistence;

public class EmployeeShiftDbContext(DbContextOptions<EmployeeShiftDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ShiftDefinition> ShiftDefinitions => Set<ShiftDefinition>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Department).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => x.EmployeeCode).IsUnique();
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Notes).HasMaxLength(300);
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ShiftDefinition)
                .WithMany()
                .HasForeignKey(x => x.ShiftDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.EmployeeId, x.ShiftDate, x.ShiftDefinitionId }).IsUnique();
        });

        modelBuilder.Entity<ShiftDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(60).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });
    }
}
