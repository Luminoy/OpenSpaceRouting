using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenSpaceRouting
{
    public partial class UpdateNodesDialog : Form
    {
        Form1 fm1 = null;
        public UpdateNodesDialog()
        {
            InitializeComponent();
        }

        private void UpdateNodesDialog_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = true;
            checkBox2.Checked = false;
            checkBox3.Checked = false;

            numericUpDown1.Value = 1;
            numericUpDown2.Value = 5;
            fm1 = (Form1)this.Owner;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.circle_search_flag = checkBox3.Checked;
            Form1.order_search_flag = checkBox1.Checked;
            Form1.inverse_search_flag = checkBox2.Checked;
            Form1.circulation_start_num = Convert.ToInt32(numericUpDown1.Value);
            Form1.circulation_end_num = Convert.ToInt32(numericUpDown2.Value);
        }

    }
}
