using Infra;
using LinqToDB;
using LinqToDB.Data;

namespace Api.IntegrationTests;

public class MigrationTests
{
    [Fact]
    public void EmptyDatabase_GetsTheFullSchema()
    {
        var schema = TestDatabase.CreateSchema();
        try
        {
            using var db = TestDatabase.CreateConnection(schema);

            DatabaseMigrator.Migrate(db);

            var tables = db.Query<string>(
                    "SELECT table_name FROM information_schema.tables " +
                    "WHERE table_schema = current_schema() AND table_name <> 'schema_migrations'")
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(
                new[]
                {
                    "calendar_days", "employments", "holiday_accrual_days", "holiday_accrual_months",
                    "holiday_balance_adjustments", "holiday_rule_periods", "holiday_rules", "users", "work_schedule_days", "work_schedules"
                },
                tables);

            var versions = db.Query<string>("SELECT version FROM schema_migrations").ToList();
            Assert.Contains("001_initial_schema", versions);
            Assert.Contains("002_calender_days", versions);
            Assert.Contains("003_holiday_rules", versions);
            Assert.Contains("004_holiday_accrulas", versions);
            Assert.Contains("005_holiday_balance_adjustments", versions);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public void Migrate_CanRunTwice_WithoutRepeatingMigrations()
    {
        var schema = TestDatabase.CreateSchema();
        try
        {
            using var db = TestDatabase.CreateConnection(schema);

            DatabaseMigrator.Migrate(db);
            var first = db.Execute<int>("SELECT COUNT(*) FROM schema_migrations");
            DatabaseMigrator.Migrate(db);
            var second = db.Execute<int>("SELECT COUNT(*) FROM schema_migrations");

            Assert.Equal(first, second);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }

    [Fact]
    public void Upgrade_FromEarlierVersion_KeepsExistingData()
    {
        var schema = TestDatabase.CreateSchema();
        try
        {
            using var db = TestDatabase.CreateConnection(schema);
            
            DatabaseMigrator.Migrate(db, "001_initial_schema");
            var userId = Guid.NewGuid();
            db.Insert(new User { Id = userId, Email = "keep@example.test", PasswordHash = "hash" });
            
            DatabaseMigrator.Migrate(db);

            Assert.Equal("keep@example.test", db.Users.Single(user => user.Id == userId).Email);
        }
        finally
        {
            TestDatabase.DropSchema(schema);
        }
    }
}