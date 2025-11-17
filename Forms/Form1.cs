using System;
using System.Collections.Generic;
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
                    txtSourceUser.Text,
                    txtSourcePassword.Text,
                    txtSourceDatabase.Text);

                var destinationConnectionString = BuildConnectionString(
                    txtDestinationHost.Text,
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

        private static string BuildConnectionString(string host, string user, string password, string database)
        {
            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = host,
                UserID = user,
                Password = password,
                Database = database,
                SslMode = MySqlConnector.MySqlSslMode.None
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
            grpSource.Enabled = enabled;
            grpDestination.Enabled = enabled;
            chkTriggers.Enabled = enabled;
            chkRoutines.Enabled = enabled;
            chkViews.Enabled = enabled;
        }
    }
}
