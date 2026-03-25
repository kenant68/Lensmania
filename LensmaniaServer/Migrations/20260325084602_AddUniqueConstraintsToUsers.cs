using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LensmaniaServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM ""Users""
        GROUP BY ""Email""
        HAVING COUNT(*) > 1
    ) THEN
        RAISE NOTICE 'Duplicates detected for Users.Email. Deduplicating before creating unique index IX_Users_Email.';
        WITH d AS (
            SELECT ""Id"",
                   ROW_NUMBER() OVER (PARTITION BY ""Email"" ORDER BY ""Id"") AS rn
            FROM ""Users""
        )
        DELETE FROM ""Users"" u
        USING d
        WHERE u.""Id"" = d.""Id""
          AND d.rn > 1;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'public'
          AND indexname = 'IX_Users_Email'
    ) THEN
        CREATE UNIQUE INDEX ""IX_Users_Email"" ON ""Users"" (""Email"");
    END IF;
END$$;
");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM ""Users""
        GROUP BY ""Username""
        HAVING COUNT(*) > 1
    ) THEN
        RAISE NOTICE 'Duplicates detected for Users.Username. Deduplicating before creating unique index IX_Users_Username.';
        WITH d AS (
            SELECT ""Id"",
                   ROW_NUMBER() OVER (PARTITION BY ""Username"" ORDER BY ""Id"") AS rn
            FROM ""Users""
        )
        DELETE FROM ""Users"" u
        USING d
        WHERE u.""Id"" = d.""Id""
          AND d.rn > 1;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'public'
          AND indexname = 'IX_Users_Username'
    ) THEN
        CREATE UNIQUE INDEX ""IX_Users_Username"" ON ""Users"" (""Username"");
    END IF;
END$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Users_Email"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Users_Username"";");
        }
    }
}
