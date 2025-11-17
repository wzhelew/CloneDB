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


            var originalForeignKeyState = await GetForeignKeyChecksAsync(destination, cancellationToken);
            await SetForeignKeyChecksAsync(destination, 0, cancellationToken);

            try
            {
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
            finally
            {
                await SetForeignKeyChecksAsync(destination, originalForeignKeyState, cancellationToken);
            }
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
                await using var createReader = await createCmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                // Use the ordinal to avoid locale-dependent column name lookups (e.g., "Create View").
                var createStatement = createReader.GetString(1);
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

        private static async Task SetForeignKeyChecksAsync(MySqlConnection connection, uint value, CancellationToken cancellationToken)
        {
            await using var cmd = new MySqlCommand($"SET FOREIGN_KEY_CHECKS={value};", connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }


        private static string WrapName(string name) => $"`{name}`";
    }
}
