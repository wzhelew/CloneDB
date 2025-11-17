using System;
using System.Windows.Forms;

namespace CloneDBManager.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("CloneDBManager is running!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
