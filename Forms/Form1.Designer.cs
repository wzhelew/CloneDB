namespace CloneDBManager.Forms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpSource = new System.Windows.Forms.GroupBox();
            this.btnLoadTables = new System.Windows.Forms.Button();
            this.txtSourceDatabase = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSourcePassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSourceUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSourcePort = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSourceHost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpDestination = new System.Windows.Forms.GroupBox();
            this.txtDestinationDatabase = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDestinationPassword = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDestinationUser = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDestinationPort = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtDestinationHost = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvTables = new System.Windows.Forms.DataGridView();
            this.colTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCopyData = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnClone = new System.Windows.Forms.Button();
            this.chkTriggers = new System.Windows.Forms.CheckBox();
            this.chkRoutines = new System.Windows.Forms.CheckBox();
            this.chkViews = new System.Windows.Forms.CheckBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnSaveTemplate = new System.Windows.Forms.Button();
            this.btnLoadTemplate = new System.Windows.Forms.Button();
            this.btnSqlDump = new System.Windows.Forms.Button();
            this.btnSqlRestore = new System.Windows.Forms.Button();
            this.grpSource.SuspendLayout();
            this.grpDestination.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTables)).BeginInit();
            this.SuspendLayout();
            //
            // grpSource
            // 
            this.grpSource.Controls.Add(this.btnLoadTables);
            this.grpSource.Controls.Add(this.txtSourceDatabase);
            this.grpSource.Controls.Add(this.label4);
            this.grpSource.Controls.Add(this.txtSourcePassword);
            this.grpSource.Controls.Add(this.label3);
            this.grpSource.Controls.Add(this.txtSourceUser);
            this.grpSource.Controls.Add(this.label2);
            this.grpSource.Controls.Add(this.txtSourcePort);
            this.grpSource.Controls.Add(this.label10);
            this.grpSource.Controls.Add(this.txtSourceHost);
            this.grpSource.Controls.Add(this.label1);
            this.grpSource.Location = new System.Drawing.Point(12, 12);
            this.grpSource.Name = "grpSource";
            this.grpSource.Size = new System.Drawing.Size(400, 221);
            this.grpSource.TabIndex = 0;
            this.grpSource.TabStop = false;
            this.grpSource.Text = "Source (read from)";
            //
            // btnLoadTables
            //
            this.btnLoadTables.Location = new System.Drawing.Point(20, 178);
            this.btnLoadTables.Name = "btnLoadTables";
            this.btnLoadTables.Size = new System.Drawing.Size(360, 35);
            this.btnLoadTables.TabIndex = 5;
            this.btnLoadTables.Text = "Load Tables";
            this.btnLoadTables.UseVisualStyleBackColor = true;
            this.btnLoadTables.Click += new System.EventHandler(this.btnLoadTables_Click);
            // 
            // txtSourceDatabase
            // 
            this.txtSourceDatabase.Location = new System.Drawing.Point(96, 145);
            this.txtSourceDatabase.Name = "txtSourceDatabase";
            this.txtSourceDatabase.Size = new System.Drawing.Size(284, 27);
            this.txtSourceDatabase.TabIndex = 4;
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Database";
            //
            // txtSourcePassword
            //
            this.txtSourcePassword.Location = new System.Drawing.Point(96, 112);
            this.txtSourcePassword.Name = "txtSourcePassword";
            this.txtSourcePassword.PasswordChar = '*';
            this.txtSourcePassword.Size = new System.Drawing.Size(284, 27);
            this.txtSourcePassword.TabIndex = 3;
            //
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password";
            //
            // txtSourceUser
            //
            this.txtSourceUser.Location = new System.Drawing.Point(96, 79);
            this.txtSourceUser.Name = "txtSourceUser";
            this.txtSourceUser.Size = new System.Drawing.Size(284, 27);
            this.txtSourceUser.TabIndex = 2;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "User";
            //
            // txtSourcePort
            //
            this.txtSourcePort.Location = new System.Drawing.Point(96, 46);
            this.txtSourcePort.Name = "txtSourcePort";
            this.txtSourcePort.Size = new System.Drawing.Size(284, 27);
            this.txtSourcePort.TabIndex = 1;
            //
            // label10
            //
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 20);
            this.label10.TabIndex = 10;
            this.label10.Text = "Port";
            //
            // txtSourceHost
            //
            this.txtSourceHost.Location = new System.Drawing.Point(96, 13);
            this.txtSourceHost.Name = "txtSourceHost";
            this.txtSourceHost.Size = new System.Drawing.Size(284, 27);
            this.txtSourceHost.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host";
            // 
            // grpDestination
            // 
            this.grpDestination.Controls.Add(this.txtDestinationDatabase);
            this.grpDestination.Controls.Add(this.label8);
            this.grpDestination.Controls.Add(this.txtDestinationPassword);
            this.grpDestination.Controls.Add(this.label7);
            this.grpDestination.Controls.Add(this.txtDestinationUser);
            this.grpDestination.Controls.Add(this.label6);
            this.grpDestination.Controls.Add(this.txtDestinationPort);
            this.grpDestination.Controls.Add(this.label11);
            this.grpDestination.Controls.Add(this.txtDestinationHost);
            this.grpDestination.Controls.Add(this.label5);
            this.grpDestination.Location = new System.Drawing.Point(430, 12);
            this.grpDestination.Name = "grpDestination";
            this.grpDestination.Size = new System.Drawing.Size(400, 221);
            this.grpDestination.TabIndex = 1;
            this.grpDestination.TabStop = false;
            this.grpDestination.Text = "Destination (write to)";
            //
            // txtDestinationDatabase
            // 
            this.txtDestinationDatabase.Location = new System.Drawing.Point(96, 145);
            this.txtDestinationDatabase.Name = "txtDestinationDatabase";
            this.txtDestinationDatabase.Size = new System.Drawing.Size(284, 27);
            this.txtDestinationDatabase.TabIndex = 4;
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 148);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 20);
            this.label8.TabIndex = 6;
            this.label8.Text = "Database";
            //
            // txtDestinationPassword
            //
            this.txtDestinationPassword.Location = new System.Drawing.Point(96, 112);
            this.txtDestinationPassword.Name = "txtDestinationPassword";
            this.txtDestinationPassword.PasswordChar = '*';
            this.txtDestinationPassword.Size = new System.Drawing.Size(284, 27);
            this.txtDestinationPassword.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 20);
            this.label7.TabIndex = 4;
            this.label7.Text = "Password";
            //
            // txtDestinationUser
            //
            this.txtDestinationUser.Location = new System.Drawing.Point(96, 79);
            this.txtDestinationUser.Name = "txtDestinationUser";
            this.txtDestinationUser.Size = new System.Drawing.Size(284, 27);
            this.txtDestinationUser.TabIndex = 2;
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 20);
            this.label6.TabIndex = 2;
            this.label6.Text = "User";
            //
            // txtDestinationPort
            //
            this.txtDestinationPort.Location = new System.Drawing.Point(96, 46);
            this.txtDestinationPort.Name = "txtDestinationPort";
            this.txtDestinationPort.Size = new System.Drawing.Size(284, 27);
            this.txtDestinationPort.TabIndex = 1;
            //
            // label11
            //
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 49);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 20);
            this.label11.TabIndex = 10;
            this.label11.Text = "Port";
            //
            // txtDestinationHost
            //
            this.txtDestinationHost.Location = new System.Drawing.Point(96, 13);
            this.txtDestinationHost.Name = "txtDestinationHost";
            this.txtDestinationHost.Size = new System.Drawing.Size(284, 27);
            this.txtDestinationHost.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "Host";
            // 
            // dgvTables
            // 
            this.dgvTables.AllowUserToAddRows = false;
            this.dgvTables.AllowUserToDeleteRows = false;
            this.dgvTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTable,
            this.colCopyData});
            this.dgvTables.Location = new System.Drawing.Point(12, 252);
            this.dgvTables.Name = "dgvTables";
            this.dgvTables.RowHeadersVisible = false;
            this.dgvTables.RowHeadersWidth = 51;
            this.dgvTables.RowTemplate.Height = 29;
            this.dgvTables.Size = new System.Drawing.Size(818, 200);
            this.dgvTables.TabIndex = 2;
            //
            // colTable
            //
            this.colTable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTable.HeaderText = "Table";
            this.colTable.MinimumWidth = 6;
            this.colTable.Name = "colTable";
            this.colTable.ReadOnly = true;
            // 
            // colCopyData
            // 
            this.colCopyData.HeaderText = "Copy Data";
            this.colCopyData.MinimumWidth = 6;
            this.colCopyData.Name = "colCopyData";
            this.colCopyData.Width = 125;
            // 
            // btnClone
            // 
            this.btnClone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClone.Location = new System.Drawing.Point(655, 568);
            this.btnClone.Name = "btnClone";
            this.btnClone.Size = new System.Drawing.Size(175, 35);
            this.btnClone.TabIndex = 6;
            this.btnClone.Text = "Clone Database";
            this.btnClone.UseVisualStyleBackColor = true;
            this.btnClone.Click += new System.EventHandler(this.btnClone_Click);
            // 
            // chkTriggers
            // 
            this.chkTriggers.AutoSize = true;
            this.chkTriggers.Checked = true;
            this.chkTriggers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTriggers.Location = new System.Drawing.Point(12, 470);
            this.chkTriggers.Name = "chkTriggers";
            this.chkTriggers.Size = new System.Drawing.Size(156, 24);
            this.chkTriggers.TabIndex = 3;
            this.chkTriggers.Text = "Copy triggers";
            this.chkTriggers.UseVisualStyleBackColor = true;
            //
            // chkRoutines
            //
            this.chkRoutines.AutoSize = true;
            this.chkRoutines.Checked = true;
            this.chkRoutines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRoutines.Location = new System.Drawing.Point(186, 470);
            this.chkRoutines.Name = "chkRoutines";
            this.chkRoutines.Size = new System.Drawing.Size(245, 24);
            this.chkRoutines.TabIndex = 4;
            this.chkRoutines.Text = "Copy functions/procedures";
            this.chkRoutines.UseVisualStyleBackColor = true;
            // 
            // chkViews
            //
            this.chkViews.AutoSize = true;
            this.chkViews.Checked = true;
            this.chkViews.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkViews.Location = new System.Drawing.Point(454, 470);
            this.chkViews.Name = "chkViews";
            this.chkViews.Size = new System.Drawing.Size(122, 24);
            this.chkViews.TabIndex = 5;
            this.chkViews.Text = "Copy views";
            this.chkViews.UseVisualStyleBackColor = true;
            //
            // txtLog
            //
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 508);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(818, 60);
            this.txtLog.TabIndex = 7;
            // 
            // label9
            //
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 485);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 20);
            this.label9.TabIndex = 8;
            this.label9.Text = "Log";
            //
            // lblVersion
            //
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(12, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(79, 20);
            this.lblVersion.TabIndex = 9;
            this.lblVersion.Text = "Версия 1.3";
            //
            // btnSaveTemplate
            //
            this.btnSaveTemplate.Location = new System.Drawing.Point(12, 218);
            this.btnSaveTemplate.Name = "btnSaveTemplate";
            this.btnSaveTemplate.Size = new System.Drawing.Size(175, 28);
            this.btnSaveTemplate.TabIndex = 10;
            this.btnSaveTemplate.Text = "Запази шаблон";
            this.btnSaveTemplate.UseVisualStyleBackColor = true;
            this.btnSaveTemplate.Click += new System.EventHandler(this.btnSaveTemplate_Click);
            //
            // btnLoadTemplate
            //
            this.btnLoadTemplate.Location = new System.Drawing.Point(193, 218);
            this.btnLoadTemplate.Name = "btnLoadTemplate";
            this.btnLoadTemplate.Size = new System.Drawing.Size(175, 28);
            this.btnLoadTemplate.TabIndex = 11;
            this.btnLoadTemplate.Text = "Зареди шаблон";
            this.btnLoadTemplate.UseVisualStyleBackColor = true;
            this.btnLoadTemplate.Click += new System.EventHandler(this.btnLoadTemplate_Click);
            //
            // btnSqlDump
            //
            this.btnSqlDump.Location = new System.Drawing.Point(374, 218);
            this.btnSqlDump.Name = "btnSqlDump";
            this.btnSqlDump.Size = new System.Drawing.Size(175, 28);
            this.btnSqlDump.TabIndex = 12;
            this.btnSqlDump.Text = "SQLDUMP";
            this.btnSqlDump.UseVisualStyleBackColor = true;
            this.btnSqlDump.Click += new System.EventHandler(this.btnSqlDump_Click);
            //
            // btnSqlRestore
            //
            this.btnSqlRestore.Location = new System.Drawing.Point(555, 218);
            this.btnSqlRestore.Name = "btnSqlRestore";
            this.btnSqlRestore.Size = new System.Drawing.Size(175, 28);
            this.btnSqlRestore.TabIndex = 13;
            this.btnSqlRestore.Text = "SQLRESTORE";
            this.btnSqlRestore.UseVisualStyleBackColor = true;
            this.btnSqlRestore.Click += new System.EventHandler(this.btnSqlRestore_Click);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 615);
            this.Controls.Add(this.btnSqlRestore);
            this.Controls.Add(this.btnLoadTemplate);
            this.Controls.Add(this.btnSaveTemplate);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.chkViews);
            this.Controls.Add(this.chkRoutines);
            this.Controls.Add(this.chkTriggers);
            this.Controls.Add(this.btnClone);
            this.Controls.Add(this.dgvTables);
            this.Controls.Add(this.grpDestination);
            this.Controls.Add(this.grpSource);
            this.Controls.Add(this.btnSqlDump);
            this.Name = "Form1";
            this.Text = "CloneDBManager";
            this.grpSource.ResumeLayout(false);
            this.grpSource.PerformLayout();
            this.grpDestination.ResumeLayout(false);
            this.grpDestination.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTables)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpSource;
        private System.Windows.Forms.Button btnLoadTables;
        private System.Windows.Forms.TextBox txtSourceDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSourcePassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSourceUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSourcePort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSourceHost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpDestination;
        private System.Windows.Forms.TextBox txtDestinationDatabase;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDestinationPassword;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDestinationUser;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDestinationPort;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtDestinationHost;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgvTables;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTable;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCopyData;
        private System.Windows.Forms.Button btnClone;
        private System.Windows.Forms.CheckBox chkTriggers;
        private System.Windows.Forms.CheckBox chkRoutines;
        private System.Windows.Forms.CheckBox chkViews;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnSaveTemplate;
        private System.Windows.Forms.Button btnLoadTemplate;
        private System.Windows.Forms.Button btnSqlDump;
        private System.Windows.Forms.Button btnSqlRestore;
    }
}
