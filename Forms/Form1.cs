using System;
using System.Collections.Generic;
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
                    AppendLog);
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

        private static string BuildConnectionString(string host, string port, string user, string password, string database)
        {
            var parsedPort = uint.TryParse(port, out var numericPort) && numericPort > 0 ? numericPort : 3306u;
            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = host,
                Port = parsedPort,
                UserID = user,
                Password = password,
                Database = database,
                SslMode = MySqlConnector.MySqlSslMode.None,
                AllowUserVariables = true
            };

            return builder.ToString();
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
            btnLoadTables.Enabled = enabled;
            btnLoadTemplate.Enabled = enabled;
            btnSaveTemplate.Enabled = enabled;
            grpSource.Enabled = enabled;
            grpDestination.Enabled = enabled;
            chkTriggers.Enabled = enabled;
            chkRoutines.Enabled = enabled;
            chkViews.Enabled = enabled;
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
    }
}
