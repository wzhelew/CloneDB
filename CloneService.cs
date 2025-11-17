using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace CloneDBManager
{
    public record TableCloneOption(string Name, bool CopyData);

    public static class CloneService
    {
        public static async Task<IReadOnlyList<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var tables = new List<string>();
            const string sql = "SELECT TABLE_NAME FROM information_schema.tables WHERE table_schema = DATABASE() ORDER BY TABLE_NAME";
            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        public static async Task CloneDatabaseAsync(
            string sourceConnectionString,
            string destinationConnectionString,
            IReadOnlyCollection<TableCloneOption> tables,
            bool copyTriggers,
            bool copyRoutines,
            bool copyViews,
            Action<string>? log,
            CancellationToken cancellationToken = default)
        {
            await using var source = new MySqlConnection(sourceConnectionString);
            await using var destination = new MySqlConnection(destinationConnectionString);
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);

            log?.Invoke("Disabling foreign key checks on destination...");
            await DisableForeignKeys(destination, cancellationToken);

            var foreignKeys = await GetForeignKeyDefinitionsAsync(source, cancellationToken);

            foreach (var table in tables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                log?.Invoke($"Cloning structure for table '{table.Name}'...");
                await CloneTableAsync(source, destination, table.Name, cancellationToken);

                if (table.CopyData)
                {
                    log?.Invoke($"Copying data for '{table.Name}'...");
                    await CopyDataAsync(source, destination, table.Name, cancellationToken);
                }
            }

            if (copyViews)
            {
                log?.Invoke("Cloning views...");
                await CloneViewsAsync(source, destination, cancellationToken);
            }

            log?.Invoke("Recreating foreign keys after data load with checks disabled...");
            await CopyForeignKeys(destination, foreignKeys, log, cancellationToken);

            log?.Invoke("Re-enabling foreign key checks on destination after foreign key creation...");
            await EnableForeignKeys(destination, cancellationToken);

            if (copyTriggers)
            {
                log?.Invoke("Cloning triggers...");
                await CloneTriggersAsync(source, destination, cancellationToken);
            }

            if (copyRoutines)
            {
                log?.Invoke("Cloning stored routines (functions/procedures)...");
                await CloneRoutinesAsync(source, destination, cancellationToken);
            }

            log?.Invoke("Cloning completed successfully.");
        }

        private static async Task CloneTableAsync(MySqlConnection source, MySqlConnection destination, string tableName, CancellationToken cancellationToken)
        {
            await using var createCmd = new MySqlCommand($"SHOW CREATE TABLE `{tableName}`;", source);
            await using var reader = await createCmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return;
            }

            var createStatement = reader.GetString("Create Table");
            await reader.CloseAsync();

            await using var dropCmd = new MySqlCommand($"DROP TABLE IF EXISTS `{tableName}`;", destination);
            await dropCmd.ExecuteNonQueryAsync(cancellationToken);

            await using var createDestCmd = new MySqlCommand(createStatement, destination);
            await createDestCmd.ExecuteNonQueryAsync(cancellationToken);

            await DropForeignKeysAsync(destination, tableName, cancellationToken);
        }

        private static async Task CopyDataAsync(MySqlConnection source, MySqlConnection destination, string tableName, CancellationToken cancellationToken)
        {
            await using var selectCmd = new MySqlCommand($"SELECT * FROM `{tableName}`;", source);
            await using var reader = await selectCmd.ExecuteReaderAsync(cancellationToken);
            if (!reader.HasRows)
            {
                return;
            }

            var columnNames = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToArray();

            var parameterNames = columnNames.Select((_, i) => $"@p{i}").ToArray();
            var insertSql = $"INSERT INTO `{tableName}` ({string.Join(", ", columnNames.Select(WrapName))}) VALUES ({string.Join(", ", parameterNames)});";

            while (await reader.ReadAsync(cancellationToken))
            {
                await using var insertCmd = new MySqlCommand(insertSql, destination);
                for (var i = 0; i < columnNames.Length; i++)
                {
                    insertCmd.Parameters.AddWithValue(parameterNames[i], reader.GetValue(i));
                }

                await insertCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task CloneViewsAsync(MySqlConnection source, MySqlConnection destination, CancellationToken cancellationToken)
        {
            const string listViewsSql = "SHOW FULL TABLES WHERE Table_type = 'VIEW';";
            await using var listCmd = new MySqlCommand(listViewsSql, source);
            await using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            var views = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                views.Add(reader.GetString(0));
            }
            await reader.CloseAsync();

            foreach (var viewName in views)
            {
                await using var createCmd = new MySqlCommand($"SHOW CREATE VIEW `{viewName}`;", source);
                await using var createReader = await createCmd.ExecuteReaderAsync(cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                var createStatement = createReader.GetString("Create View");
                await createReader.CloseAsync();

                await using var dropCmd = new MySqlCommand($"DROP VIEW IF EXISTS `{viewName}`;", destination);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);

                await using var createDestCmd = new MySqlCommand(createStatement, destination);
                await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task CloneTriggersAsync(MySqlConnection source, MySqlConnection destination, CancellationToken cancellationToken)
        {
            const string listSql = "SELECT TRIGGER_NAME FROM information_schema.triggers WHERE TRIGGER_SCHEMA = DATABASE();";
            await using var listCmd = new MySqlCommand(listSql, source);
            await using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            var triggers = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                triggers.Add(reader.GetString(0));
            }
            await reader.CloseAsync();

            foreach (var trigger in triggers)
            {
                await using var createCmd = new MySqlCommand($"SHOW CREATE TRIGGER `{trigger}`;", source);
                await using var createReader = await createCmd.ExecuteReaderAsync(cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                var createStatement = createReader.GetString("SQL Original Statement");
                await createReader.CloseAsync();

                await using var dropCmd = new MySqlCommand($"DROP TRIGGER IF EXISTS `{trigger}`;", destination);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);

                await using var createDestCmd = new MySqlCommand(createStatement, destination);
                await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task CloneRoutinesAsync(MySqlConnection source, MySqlConnection destination, CancellationToken cancellationToken)
        {
            const string listSql = "SELECT ROUTINE_NAME, ROUTINE_TYPE FROM information_schema.routines WHERE ROUTINE_SCHEMA = DATABASE();";
            await using var listCmd = new MySqlCommand(listSql, source);
            await using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            var routines = new List<(string Name, string Type)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                routines.Add((reader.GetString(0), reader.GetString(1)));
            }
            await reader.CloseAsync();

            foreach (var routine in routines)
            {
                var showCommand = routine.Type.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase)
                    ? $"SHOW CREATE FUNCTION `{routine.Name}`;"
                    : $"SHOW CREATE PROCEDURE `{routine.Name}`;";

                await using var createCmd = new MySqlCommand(showCommand, source);
                await using var createReader = await createCmd.ExecuteReaderAsync(cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                var createStatement = routine.Type.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase)
                    ? createReader.GetString("Create Function")
                    : createReader.GetString("Create Procedure");
                await createReader.CloseAsync();

                var dropSql = routine.Type.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase)
                    ? $"DROP FUNCTION IF EXISTS `{routine.Name}`;"
                    : $"DROP PROCEDURE IF EXISTS `{routine.Name}`;";

                await using var dropCmd = new MySqlCommand(dropSql, destination);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);

                await using var createDestCmd = new MySqlCommand(createStatement, destination);
                await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task<uint> GetForeignKeyChecksAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            await using var cmd = new MySqlCommand("SELECT @@FOREIGN_KEY_CHECKS;", connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToUInt32(result);
        }

        public static Task DisableForeignKeys(MySqlConnection connection, CancellationToken cancellationToken = default)
            => SetForeignKeyChecksAsync(connection, 0, cancellationToken);

        public static Task EnableForeignKeys(MySqlConnection connection, CancellationToken cancellationToken = default)
            => SetForeignKeyChecksAsync(connection, 1, cancellationToken);

        private static async Task SetForeignKeyChecksAsync(MySqlConnection connection, uint value, CancellationToken cancellationToken)
        {
            await using var cmd = new MySqlCommand($"SET FOREIGN_KEY_CHECKS={value};", connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        private static async Task DropForeignKeysAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT CONSTRAINT_NAME
FROM information_schema.key_column_usage
WHERE table_schema = DATABASE()
  AND referenced_table_name IS NOT NULL
  AND table_name = @tableName
GROUP BY CONSTRAINT_NAME";

            await using var listCmd = new MySqlCommand(sql, connection);
            listCmd.Parameters.AddWithValue("@tableName", tableName);
            await using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            var constraints = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                constraints.Add(reader.GetString(0));
            }
            await reader.CloseAsync();

            foreach (var constraint in constraints)
            {
                var dropSql = $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{constraint}`;";
                await using var dropCmd = new MySqlCommand(dropSql, connection);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task<IReadOnlyList<ForeignKeyDefinition>> GetForeignKeyDefinitionsAsync(MySqlConnection source, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT
    kcu.CONSTRAINT_NAME,
    kcu.TABLE_NAME,
    kcu.COLUMN_NAME,
    kcu.REFERENCED_TABLE_NAME,
    kcu.REFERENCED_COLUMN_NAME,
    kcu.ORDINAL_POSITION,
    rc.DELETE_RULE,
    rc.UPDATE_RULE
FROM information_schema.key_column_usage kcu
JOIN information_schema.referential_constraints rc
  ON rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
 AND rc.CONSTRAINT_SCHEMA = kcu.CONSTRAINT_SCHEMA
 AND rc.TABLE_NAME = kcu.TABLE_NAME
WHERE kcu.CONSTRAINT_SCHEMA = DATABASE()
  AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
ORDER BY kcu.TABLE_NAME, kcu.CONSTRAINT_NAME, kcu.ORDINAL_POSITION;";

            await using var cmd = new MySqlCommand(sql, source);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            var definitions = new Dictionary<(string Table, string Constraint), ForeignKeyDefinitionBuilder>(StringComparer.OrdinalIgnoreCase);

            while (await reader.ReadAsync(cancellationToken))
            {
                var key = (reader.GetString(1), reader.GetString(0));
                if (!definitions.TryGetValue(key, out var builder))
                {
                    builder = new ForeignKeyDefinitionBuilder(reader.GetString(0), reader.GetString(1), reader.GetString(3), reader.GetString(6), reader.GetString(7));
                    definitions[key] = builder;
                }

                builder.AddColumn(reader.GetInt32(5), reader.GetString(2), reader.GetString(4));
            }

            await reader.CloseAsync();

            return definitions.Values
                .Select(b => b.Build())
                .OrderBy(d => d.TableName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.ConstraintName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static async Task CopyForeignKeys(MySqlConnection destination, IReadOnlyList<ForeignKeyDefinition> definitions, Action<string>? log, CancellationToken cancellationToken = default)
        {
            foreach (var fk in definitions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dropSql = $"ALTER TABLE `{fk.TableName}` DROP FOREIGN KEY `{fk.ConstraintName}`;";
                try
                {
                    await using var dropCmd = new MySqlCommand(dropSql, destination);
                    await dropCmd.ExecuteNonQueryAsync(cancellationToken);
                }
                catch
                {
                    // ignore drop failures; constraint may not exist
                }

                var addSql = $"ALTER TABLE `{fk.TableName}` ADD CONSTRAINT `{fk.ConstraintName}` FOREIGN KEY ({string.Join(", ", fk.Columns.Select(WrapName))}) REFERENCES `{fk.ReferencedTable}` ({string.Join(", ", fk.ReferencedColumns.Select(WrapName))})";
                if (!string.Equals(fk.DeleteRule, "NO ACTION", StringComparison.OrdinalIgnoreCase))
                {
                    addSql += $" ON DELETE {fk.DeleteRule}";
                }

                if (!string.Equals(fk.UpdateRule, "NO ACTION", StringComparison.OrdinalIgnoreCase))
                {
                    addSql += $" ON UPDATE {fk.UpdateRule}";
                }

                addSql += ";";

                try
                {
                    await using var addCmd = new MySqlCommand(addSql, destination);
                    await addCmd.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Failed to create foreign key '{fk.ConstraintName}' on table '{fk.TableName}': {ex.Message}");
                }
            }
        }

        private sealed class ForeignKeyDefinitionBuilder
        {
            private readonly SortedDictionary<int, (string Column, string ReferencedColumn)> _columns = new();

            public ForeignKeyDefinitionBuilder(string constraintName, string tableName, string referencedTable, string deleteRule, string updateRule)
            {
                ConstraintName = constraintName;
                TableName = tableName;
                ReferencedTable = referencedTable;
                DeleteRule = deleteRule;
                UpdateRule = updateRule;
            }

            public string ConstraintName { get; }
            public string TableName { get; }
            public string ReferencedTable { get; }
            public string DeleteRule { get; }
            public string UpdateRule { get; }

            public void AddColumn(int ordinal, string column, string referencedColumn)
            {
                _columns[ordinal] = (column, referencedColumn);
            }

            public ForeignKeyDefinition Build()
            {
                var columns = _columns.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.Column).ToArray();
                var referenced = _columns.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.ReferencedColumn).ToArray();
                return new ForeignKeyDefinition(TableName, ConstraintName, columns, ReferencedTable, referenced, DeleteRule, UpdateRule);
            }
        }

        public sealed record ForeignKeyDefinition(
            string TableName,
            string ConstraintName,
            IReadOnlyList<string> Columns,
            string ReferencedTable,
            IReadOnlyList<string> ReferencedColumns,
            string DeleteRule,
            string UpdateRule);

        private static string WrapName(string name) => $"`{name}`";
    }
}
