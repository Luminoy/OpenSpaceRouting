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
    public partial class Lineofsight : Form
    {
        public Form1 fm1 = null;
        public string default_dir = null;
        public Lineofsight()
        {
            InitializeComponent();
        }

        public Lineofsight(string str)
        {
            InitializeComponent();
            default_dir = str;
        }

        public Lineofsight(string str,int x1,int y1,int x2,int y2)
        {
            InitializeComponent();
            default_dir = str;
            textBox1.Text = x1.ToString();
            textBox2.Text = y1.ToString();
            textBox3.Text = x2.ToString();
            textBox4.Text = y2.ToString();
        }


        private void Lineofsight_Load(object sender, EventArgs e)
        {
            fm1 = (Form1)this.Owner;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            double x1 = Convert.ToInt32(textBox1.Text);
            double y1 = Convert.ToInt32(textBox2.Text);
            double x2 = Convert.ToInt32(textBox3.Text);
            double y2 = Convert.ToInt32(textBox4.Text);

            if (x1 < 0 || x1 > fm1.rpRows - 1)
            {
                MessageBox.Show(" x1 数值越界！！","Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (y1 < 0 || y1 > fm1.rpColumns - 1)
            {
                MessageBox.Show(" y1 数值越界！！","Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (x2 < 0 || x2 > fm1.rpRows - 1)
            {
                MessageBox.Show(" x2 数值越界！！","Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (y2 < 0 || y2 > fm1.rpColumns - 1)
            {
                MessageBox.Show(" y2 数值越界！！","Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool los = false;
            double new_cost = 0;

            MyCostFunction.DDA_Line_2((int)x1, (int)y1, (int)x2, (int)y2, ref fm1.impedance, out los, out new_cost);
            if (los)
            {
                MessageBox.Show("(" + x1 + "," + y1 + ") 与 (" + x2 + "," + y2 + ") 之间可通视。\n耗费值为：" + new_cost.ToString() + "。");
            }
            else
            {
                MessageBox.Show("(" + x1 + "," + y1 + ") 与 (" + x2 + "," + y2 + ") 之间不可通视。");
            }
        }

    }
}
