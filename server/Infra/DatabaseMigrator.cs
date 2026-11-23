using LinqToDB;
using LinqToDB.Data;

namespace Infra;

public static class DatabaseMigrator
{
    public static void Migrate(MyDatabaseConnection db, string? upToVersion = null)
    {
        using var transaction = db.BeginTransaction();
        
        db.Execute("SELECT pg_advisory_xact_lock(26)");

        db.Execute("""
                   CREATE TABLE IF NOT EXISTS schema_migrations (
                       version text PRIMARY KEY,
                       applied_at timestamptz NOT NULL DEFAULT now()
                   )
                   """);

        var applied = db.Query<string>("SELECT version FROM schema_migrations").ToHashSet();

        var assembly = typeof(DatabaseMigrator).Assembly;
        var files = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".sql"))
            .OrderBy(name => name, StringComparer.Ordinal);

        foreach (var file in files)
        {
            var version = file[..^".sql".Length];

            if (upToVersion is not null && string.CompareOrdinal(version, upToVersion) > 0)
            {
                break;
            }

            if (applied.Contains(version))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(file)!;
            using var reader = new StreamReader(stream);

            db.Execute(reader.ReadToEnd());
            db.Execute(
                "INSERT INTO schema_migrations (version) VALUES (@version)",
                new DataParameter("version", version));
        }

        transaction.Commit();
    }
}