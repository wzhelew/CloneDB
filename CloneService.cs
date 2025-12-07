using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace CloneDBManager
{
    public record TableCloneOption(string Name, bool CopyData);

    public enum DataCopyMethod
    {
        BulkCopy,
        BulkInsert
    }

    public static class CloneService
    {
        public static async Task<IReadOnlyList<string>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(EnsureLocalInfileEnabled(connectionString));
            await connection.OpenAsync(cancellationToken);

            var tables = new List<string>();
            const string sql = "SELECT TABLE_NAME FROM information_schema.tables WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE' ORDER BY TABLE_NAME";
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
            DataCopyMethod copyMethod = DataCopyMethod.BulkCopy,
            CancellationToken cancellationToken = default)
        {
            await using var source = new MySqlConnection(EnsureLocalInfileEnabled(sourceConnectionString));
            await using var destination = new MySqlConnection(EnsureLocalInfileEnabled(destinationConnectionString));
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);

            var originalForeignKeyState = await GetForeignKeyChecksAsync(destination, cancellationToken);
            await SetForeignKeyChecksAsync(destination, 0, cancellationToken);

            try
            {
                foreach (var table in tables)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!await IsBaseTableAsync(source, table.Name, cancellationToken))
                    {
                        log?.Invoke($"Skipping '{table.Name}' because it is a view; views are cloned separately.");
                        continue;
                    }

                    log?.Invoke($"Cloning structure for table '{table.Name}'...");
                    await CloneTableAsync(source, destination, table.Name, cancellationToken);

                    if (table.CopyData)
                    {
                        log?.Invoke($"Copying data for '{table.Name}'...");
                        await CopyDataAsync(source, destination, table.Name, copyMethod, cancellationToken);
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

            var createStatement = reader.GetString(1);
            await reader.CloseAsync();

            await using var dropCmd = new MySqlCommand($"DROP TABLE IF EXISTS `{tableName}`;", destination);
            await dropCmd.ExecuteNonQueryAsync(cancellationToken);

            await using var createDestCmd = new MySqlCommand(createStatement, destination);
            await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        private static async Task CopyDataAsync(MySqlConnection source, MySqlConnection destination, string tableName, DataCopyMethod method, CancellationToken cancellationToken)
        {
            await using var selectCmd = new MySqlCommand($"SELECT * FROM `{tableName}`;", source);
            await using var reader = await selectCmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            if (!reader.HasRows)
            {
                return;
            }

            switch (method)
            {
                case DataCopyMethod.BulkInsert:
                    await CopyDataWithBulkInsertAsync(reader, destination, tableName, cancellationToken);
                    break;
                case DataCopyMethod.BulkCopy:
                default:
                    await CopyDataWithBulkCopyAsync(reader, destination, tableName, cancellationToken);
                    break;
            }
        }

        private static async Task CopyDataWithBulkCopyAsync(DbDataReader reader, MySqlConnection destination, string tableName, CancellationToken cancellationToken)
        {
            var bulkCopy = new MySqlBulkCopy(destination)
            {
                DestinationTableName = WrapName(tableName)
            };

            await bulkCopy.WriteToServerAsync(reader, cancellationToken);
        }

        private static async Task CopyDataWithBulkInsertAsync(DbDataReader reader, MySqlConnection destination, string tableName, CancellationToken cancellationToken)
        {
            const int batchSize = 500;

            var schema = reader.GetColumnSchema();
            if (schema.Count == 0)
            {
                return;
            }

            var columnNames = schema.Select(col => WrapName(col.ColumnName)).ToArray();
            var insertPrefix = $"INSERT INTO {WrapName(tableName)} ({string.Join(", ", columnNames)}) VALUES ";

            var valueRows = new List<string>(batchSize);
            var parameters = new List<MySqlParameter>(batchSize * columnNames.Length);

            try
            {
                await bulkCopy.WriteToServerAsync(reader, cancellationToken);
            }
            finally
            {
                var bulkCopyObj = (object)bulkCopy;
                if (bulkCopyObj is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (bulkCopyObj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private static async Task CopyDataWithBulkInsertAsync(DbDataReader reader, MySqlConnection destination, string tableName, CancellationToken cancellationToken)
        {
            const int batchSize = 500;

            var schema = reader.GetColumnSchema();
            if (schema.Count == 0)
            {
                var placeholders = new string[columnNames.Length];
                for (var i = 0; i < columnNames.Length; i++)
                {
                    var paramName = $"@p{parameters.Count}";
                    placeholders[i] = paramName;

                    var value = await reader.IsDBNullAsync(i, cancellationToken)
                        ? DBNull.Value
                        : reader.GetValue(i);

                    parameters.Add(new MySqlParameter(paramName, value));
                }

                valueRows.Add($"({string.Join(", ", placeholders)})");

                if (valueRows.Count >= batchSize)
                {
                    await FlushBatchAsync(destination, insertPrefix, valueRows, parameters, cancellationToken);
                }
            }

            await FlushBatchAsync(destination, insertPrefix, valueRows, parameters, cancellationToken);
        }

        private static async Task FlushBatchAsync(
            MySqlConnection destination,
            string insertPrefix,
            List<string> valueRows,
            List<MySqlParameter> parameters,
            CancellationToken cancellationToken)
        {
            if (valueRows.Count == 0)
            {
                return;
            }

            var sql = insertPrefix + string.Join(", ", valueRows) + ";";

            using (var insertCmd = new MySqlCommand(sql, destination))
            {
                insertCmd.Parameters.AddRange(parameters.ToArray());
                await insertCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            valueRows.Clear();
            parameters.Clear();
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

            var definitions = new List<(string Name, string CreateSql)>();
            foreach (var viewName in views)
            {
                await using var createCmd = new MySqlCommand($"SHOW CREATE VIEW `{viewName}`;", source);
                await using var createReader = await createCmd.ExecuteReaderAsync(cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                var createStatement = createReader.GetString(1);
                await createReader.CloseAsync();

                definitions.Add((viewName, createStatement));
            }

            foreach (var (name, _) in definitions)
            {
                await using var dropCmd = new MySqlCommand($"DROP VIEW IF EXISTS `{name}`;", destination);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var pending = new List<(string Name, string CreateSql)>(definitions);
            while (pending.Count > 0)
            {
                var createdThisPass = false;
                Exception? lastError = null;

                foreach (var view in pending.ToList())
                {
                    try
                    {
                        await using var createDestCmd = new MySqlCommand(view.CreateSql, destination);
                        await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
                        pending.Remove(view);
                        createdThisPass = true;
                    }
                    catch (MySqlException ex) when (ex.Number == 1146 || ex.Number == 1356)
                    {
                        lastError = ex;
                    }
                }

                if (!createdThisPass)
                {
                    throw lastError ?? new InvalidOperationException("Unable to create views due to unresolved dependencies.");
                }
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

            var sourceSchema = await GetCurrentDatabaseAsync(source, cancellationToken);
            var destinationSchema = await GetCurrentDatabaseAsync(destination, cancellationToken);

            foreach (var trigger in triggers)
            {
                await using var createCmd = new MySqlCommand($"SHOW CREATE TRIGGER `{trigger}`;", source);
                await using var createReader = await createCmd.ExecuteReaderAsync(cancellationToken);
                if (!await createReader.ReadAsync(cancellationToken))
                {
                    continue;
                }

                var createStatement = createReader.GetString(2);
                await createReader.CloseAsync();

                if (!string.IsNullOrEmpty(sourceSchema) && !string.IsNullOrEmpty(destinationSchema) && !sourceSchema.Equals(destinationSchema, StringComparison.OrdinalIgnoreCase))
                {
                    createStatement = createStatement.Replace($"`{sourceSchema}`.", $"`{destinationSchema}`.");
                }

                await using var dropCmd = new MySqlCommand($"DROP TRIGGER IF EXISTS `{trigger}`;", destination);
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);

                await using var createDestCmd = new MySqlCommand(createStatement, destination);
                await createDestCmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static async Task<string> GetCurrentDatabaseAsync(MySqlConnection connection, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(connection.Database))
            {
                return connection.Database;
            }

            await using var cmd = new MySqlCommand("SELECT DATABASE();", connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return Convert.ToString(result) ?? string.Empty;
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

                var createStatement = createReader.GetString(2);
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

        private static async Task<bool> IsBaseTableAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
        {
            const string sql = "SELECT TABLE_TYPE FROM information_schema.tables WHERE table_schema = DATABASE() AND TABLE_NAME = @tableName LIMIT 1;";
            await using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tableName", tableName);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return string.Equals(result as string, "BASE TABLE", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task SetForeignKeyChecksAsync(MySqlConnection connection, uint value, CancellationToken cancellationToken)
        {
            await using var cmd = new MySqlCommand($"SET FOREIGN_KEY_CHECKS={value};", connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        private static string WrapName(string name) => $"`{name}`";

        private static string EnsureLocalInfileEnabled(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                AllowLoadLocalInfile = true
            };

            return builder.ToString();
        }
    }
}
