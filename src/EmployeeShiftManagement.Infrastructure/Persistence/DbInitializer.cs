using EmployeeShiftManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeShiftManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(EmployeeShiftDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (dbContext.Database.IsSqlServer())
        {
            await dbContext.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'[dbo].[ShiftDefinitions]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ShiftDefinitions]
                    (
                        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                        [Name] nvarchar(60) NOT NULL,
                        [StartTime] time NOT NULL,
                        [EndTime] time NOT NULL,
                        [IsActive] bit NOT NULL CONSTRAINT [DF_ShiftDefinitions_IsActive] DEFAULT 1
                    );
                END

                -- Handle legacy Shifts table migration for ShiftName column only
                IF OBJECT_ID(N'[dbo].[Shifts]', N'U') IS NOT NULL
                BEGIN
                    -- Add ShiftName column if migrating from old schema with ShiftType enum
                    IF COL_LENGTH('dbo.Shifts', 'ShiftName') IS NULL
                    BEGIN
                        -- Only add if there was a legacy ShiftType column
                        IF COL_LENGTH('dbo.Shifts', 'ShiftType') IS NOT NULL
                        BEGIN
                            ALTER TABLE [dbo].[Shifts]
                            ADD [ShiftName] nvarchar(60) NULL;

                            EXEC(N'
                                UPDATE [dbo].[Shifts]
                                SET [ShiftName] = CASE [ShiftType]
                                    WHEN 1 THEN N''Morning''
                                    WHEN 2 THEN N''Evening''
                                    WHEN 3 THEN N''Night''
                                    WHEN 4 THEN N''Off''
                                    ELSE N''General''
                                END;');
                        END
                    END

                    -- Ensure ShiftDefinitionId column exists and is properly linked
                    IF COL_LENGTH('dbo.Shifts', 'ShiftDefinitionId') IS NOT NULL
                    BEGIN
                        -- If ShiftDefinitionId exists but constraint doesn't, add it
                        IF NOT EXISTS
                        (
                            SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Shifts_ShiftDefinitions_ShiftDefinitionId'
                        )
                        BEGIN
                            ALTER TABLE [dbo].[Shifts] WITH CHECK
                            ADD CONSTRAINT [FK_Shifts_ShiftDefinitions_ShiftDefinitionId]
                            FOREIGN KEY([ShiftDefinitionId]) REFERENCES [dbo].[ShiftDefinitions]([Id]);
                        END
                    END
                END

                -- Handle legacy singular Shift table (if it exists)
                IF OBJECT_ID(N'[dbo].[Shift]', N'U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('dbo.Shift', 'ShiftName') IS NULL
                    BEGIN
                        IF COL_LENGTH('dbo.Shift', 'ShiftType') IS NOT NULL
                        BEGIN
                            ALTER TABLE [dbo].[Shift]
                            ADD [ShiftName] nvarchar(60) NULL;
                            
                            EXEC(N'
                                UPDATE [dbo].[Shift]
                                SET [ShiftName] = CASE [ShiftType]
                                    WHEN 1 THEN N''Morning''
                                    WHEN 2 THEN N''Evening''
                                    WHEN 3 THEN N''Night''
                                    WHEN 4 THEN N''Off''
                                    ELSE N''General''
                                END;');
                        END
                    END
                END
                """);
        }

        if (!await dbContext.ShiftDefinitions.AnyAsync())
        {
            // Seed default shift definitions
            var defaultShifts = new[]
            {
                new ShiftDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = "Morning",
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(16, 0),
                    IsActive = true
                },
                new ShiftDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = "Evening",
                    StartTime = new TimeOnly(16, 0),
                    EndTime = new TimeOnly(0, 0),
                    IsActive = true
                },
                new ShiftDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = "Night",
                    StartTime = new TimeOnly(0, 0),
                    EndTime = new TimeOnly(8, 0),
                    IsActive = true
                },
                new ShiftDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = "Off",
                    StartTime = new TimeOnly(0, 0),
                    EndTime = new TimeOnly(0, 0),
                    IsActive = true
                }
            };

            await dbContext.ShiftDefinitions.AddRangeAsync(defaultShifts);
            await dbContext.SaveChangesAsync();
        }
    }
}