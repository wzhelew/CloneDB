using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloneDBManager;

namespace CloneDBManager.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cmbCopyMethod.DataSource = Enum.GetValues(typeof(DataCopyMethod));
        }

        private async void btnLoadTables_Click(object sender, EventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString(
                    txtSourceHost.Text,
                    txtSourcePort.Text,
                    txtSourceUser.Text,
                    txtSourcePassword.Text,
                    txtSourceDatabase.Text);

                var tables = await CloneService.GetTablesAsync(connectionString);
                dgvTables.Rows.Clear();

                foreach (var table in tables)
                {
                    dgvTables.Rows.Add(table, true);
                }

                AppendLog($"Loaded {tables.Count} tables from source database.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load tables: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnClone_Click(object sender, EventArgs e)
        {
            ToggleUi(false);
            try
            {
                var sourceConnectionString = BuildConnectionString(
                    txtSourceHost.Text,
                    txtSourcePort.Text,
                    txtSourceUser.Text,
                    txtSourcePassword.Text,
                    txtSourceDatabase.Text);

                var destinationConnectionString = BuildConnectionString(
                    txtDestinationHost.Text,
                    txtDestinationPort.Text,
                    txtDestinationUser.Text,
                    txtDestinationPassword.Text,
                    txtDestinationDatabase.Text);

                var tables = ReadTableSelections();

                if (!tables.Any())
                {
                    MessageBox.Show("Select at least one table to clone.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendLog("Starting clone operation...");
                await CloneService.CloneDatabaseAsync(
                    sourceConnectionString,
                    destinationConnectionString,
                    tables,
                    chkTriggers.Checked,
                    chkRoutines.Checked,
                    chkViews.Checked,
                    AppendLog,
                    GetSelectedCopyMethod(),
                    chkCreateDatabase.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Clone failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private List<TableCloneOption> ReadTableSelections()
        {
            var results = new List<TableCloneOption>();
            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                if (row.Cells[0].Value is null)
                {
                    continue;
                }

                var tableName = row.Cells[0].Value.ToString() ?? string.Empty;
                var copyData = row.Cells[1].Value is bool isChecked && isChecked;
                results.Add(new TableCloneOption(tableName, copyData));
            }

            return results;
        }

        private static MySqlConnector.MySqlConnectionStringBuilder BuildConnectionBuilder(
            string host,
            string port,
            string user,
            string password,
            string database)
        {
            var parsedPort = uint.TryParse(port, out var numericPort) && numericPort > 0 ? numericPort : 3306u;

            return new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = host,
                Port = parsedPort,
                UserID = user,
                Password = password,
                Database = database,
                SslMode = MySqlConnector.MySqlSslMode.None,
                AllowUserVariables = true,
                AllowLoadLocalInfile = true
            };
        }

        private static string BuildConnectionString(string host, string port, string user, string password, string database)
        {
            return BuildConnectionBuilder(host, port, user, password, database).ToString();
        }

        private async Task RunSqlDumpAsync(MySqlConnector.MySqlConnectionStringBuilder connectionBuilder, string filePath)
        {
            if (string.IsNullOrWhiteSpace(connectionBuilder.Database))
            {
                throw new InvalidOperationException("Database name is required for SQL dump.");
            }

            var result = await RunSqlDumpInternalAsync(connectionBuilder, filePath, includeEvents: true);

            if (result.ExitCode != 0 && IsUnknownEventsOption(result.Errors))
            {
                AppendLog("mysqldump does not support --events; retrying without events.");
                result = await RunSqlDumpInternalAsync(connectionBuilder, filePath, includeEvents: false);
            }

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"mysqldump exited with code {result.ExitCode}: {result.Errors}");
            }

            if (!string.IsNullOrWhiteSpace(result.Errors))
            {
                AppendLog(result.Errors);
            }
        }

        private static async Task<(int ExitCode, string Errors)> RunSqlDumpInternalAsync(
            MySqlConnector.MySqlConnectionStringBuilder connectionBuilder,
            string filePath,
            bool includeEvents)
        {
            var processStartInfo = new ProcessStartInfo("mysqldump")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            processStartInfo.ArgumentList.Add("--host");
            processStartInfo.ArgumentList.Add(connectionBuilder.Server);
            processStartInfo.ArgumentList.Add("--port");
            processStartInfo.ArgumentList.Add(connectionBuilder.Port.ToString());
            processStartInfo.ArgumentList.Add("--user");
            processStartInfo.ArgumentList.Add(connectionBuilder.UserID);
            if (!string.IsNullOrEmpty(connectionBuilder.Password))
            {
                processStartInfo.ArgumentList.Add($"--password={connectionBuilder.Password}");
            }

            processStartInfo.ArgumentList.Add("--routines");
            processStartInfo.ArgumentList.Add("--triggers");
            if (includeEvents)
            {
                processStartInfo.ArgumentList.Add("--events");
            }

            processStartInfo.ArgumentList.Add("--single-transaction");
            processStartInfo.ArgumentList.Add("--add-drop-database");
            processStartInfo.ArgumentList.Add("--databases");
            processStartInfo.ArgumentList.Add(connectionBuilder.Database);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            var readOutputTask = process.StandardOutput.BaseStream.CopyToAsync(fileStream);
            var readErrorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(readOutputTask, process.WaitForExitAsync());
            var errors = await readErrorTask;

            return (process.ExitCode, errors);
        }

        private static bool IsUnknownEventsOption(string errors)
        {
            return errors.IndexOf("--events", StringComparison.OrdinalIgnoreCase) >= 0
                   && errors.IndexOf("unknown", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task RunSqlRestoreAsync(MySqlConnector.MySqlConnectionStringBuilder connectionBuilder, string filePath)
        {
            if (string.IsNullOrWhiteSpace(connectionBuilder.Database))
            {
                throw new InvalidOperationException("Database name is required for SQL restore.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"SQL dump file not found: {filePath}");
            }

            var processStartInfo = new ProcessStartInfo("mysql")
            {
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            processStartInfo.ArgumentList.Add("--host");
            processStartInfo.ArgumentList.Add(connectionBuilder.Server);
            processStartInfo.ArgumentList.Add("--port");
            processStartInfo.ArgumentList.Add(connectionBuilder.Port.ToString());
            processStartInfo.ArgumentList.Add("--user");
            processStartInfo.ArgumentList.Add(connectionBuilder.UserID);
            if (!string.IsNullOrEmpty(connectionBuilder.Password))
            {
                processStartInfo.ArgumentList.Add($"--password={connectionBuilder.Password}");
            }

            await using var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            var escapedDatabaseName = connectionBuilder.Database.Replace("`", "``");

            await using (var writer = new StreamWriter(process.StandardInput.BaseStream, leaveOpen: true))
            {
                await writer.WriteLineAsync($"CREATE DATABASE IF NOT EXISTS `{escapedDatabaseName}`;");
                await writer.WriteLineAsync($"USE `{escapedDatabaseName}`;");
                await writer.FlushAsync();
            }

            var writeTask = inputStream.CopyToAsync(process.StandardInput.BaseStream);
            var readOutputTask = process.StandardOutput.ReadToEndAsync();
            var readErrorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(writeTask, process.StandardInput.FlushAsync());
            process.StandardInput.Close();

            await process.WaitForExitAsync();
            var errors = await readErrorTask;
            var output = await readOutputTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"mysql exited with code {process.ExitCode}: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(errors))
            {
                AppendLog(errors);
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                AppendLog(output);
            }
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void ToggleUi(bool enabled)
        {
            btnClone.Enabled = enabled;
            btnSqlDump.Enabled = enabled;
            btnSqlRestore.Enabled = enabled;
            btnLoadTables.Enabled = enabled;
            btnLoadTemplate.Enabled = enabled;
            btnSaveTemplate.Enabled = enabled;
            grpSource.Enabled = enabled;
            grpDestination.Enabled = enabled;
            chkTriggers.Enabled = enabled;
            chkRoutines.Enabled = enabled;
            chkViews.Enabled = enabled;
            cmbCopyMethod.Enabled = enabled;
            chkCreateDatabase.Enabled = enabled;
        }

        private DataCopyMethod GetSelectedCopyMethod()
        {
            return cmbCopyMethod.SelectedItem is DataCopyMethod method
                ? method
                : DataCopyMethod.BulkCopy;
        }

        private string GetTemplateFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "table_template.ini");
        }

        private void btnSaveTemplate_Click(object sender, EventArgs e)
        {
            var selections = ReadTableSelections();
            var templatePath = GetTemplateFilePath();

            using var writer = new StreamWriter(templatePath, false);
            writer.WriteLine("[Tables]");
            foreach (var selection in selections)
            {
                writer.WriteLine($"{selection.Name}={(selection.CopyData ? 1 : 0)}");
            }

            AppendLog($"Template saved to {templatePath}.");
        }

        private void btnLoadTemplate_Click(object sender, EventArgs e)
        {
            var templatePath = GetTemplateFilePath();
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Template file not found: {templatePath}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var template = File.ReadAllLines(templatePath)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
                .Select(line => line.Split('='))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim() == "1", StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                if (row.Cells[0].Value is null)
                {
                    continue;
                }

                var tableName = row.Cells[0].Value.ToString() ?? string.Empty;
                if (template.TryGetValue(tableName, out var copyData))
                {
                    row.Cells[1].Value = copyData;
                }
            }

            AppendLog($"Template loaded from {templatePath}.");
        }

        private async void btnSqlDump_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                FileName = $"{txtSourceDatabase.Text}_dump.sql",
                Filter = "SQL Files (*.sql)|*.sql|All files (*.*)|*.*",
                DefaultExt = "sql"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ToggleUi(false);
            try
            {
                var connectionBuilder = BuildConnectionBuilder(
                    txtSourceHost.Text,
                    txtSourcePort.Text,
                    txtSourceUser.Text,
                    txtSourcePassword.Text,
                    txtSourceDatabase.Text);

                AppendLog($"Creating SQL dump for '{connectionBuilder.Database}'...");
                await RunSqlDumpAsync(connectionBuilder, dialog.FileName);
                AppendLog($"SQL dump saved to {dialog.FileName}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQL dump failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private async void btnSqlRestore_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql|All files (*.*)|*.*",
                DefaultExt = "sql"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ToggleUi(false);
            try
            {
                var connectionBuilder = BuildConnectionBuilder(
                    txtDestinationHost.Text,
                    txtDestinationPort.Text,
                    txtDestinationUser.Text,
                    txtDestinationPassword.Text,
                    txtDestinationDatabase.Text);

                AppendLog($"Restoring database '{connectionBuilder.Database}' from dump...");
                await RunSqlRestoreAsync(connectionBuilder, dialog.FileName);
                AppendLog($"SQL dump '{dialog.FileName}' restored to '{connectionBuilder.Database}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQL restore failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }
    }
}
