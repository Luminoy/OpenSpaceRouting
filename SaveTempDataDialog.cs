using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Runtime.InteropServices;
using System.IO;

namespace OpenSpaceRouting
{
    public partial class SaveTempDataDialog : Form
    {
        public string m_default_directory = null;
        public Form1 fm1 = null;

        public SaveTempDataDialog()
        {
            InitializeComponent();
        }

        public SaveTempDataDialog(string default_dir)
        {
            InitializeComponent();
            m_default_directory = default_dir;
            textBox_Accu.Text = m_default_directory + "accumulation";
            textBox_ParentX.Text = m_default_directory + "parent_x";
            textBox_ParentY.Text = m_default_directory + "parent_y";
        }

        private void SaveTempDataDialog_Load(object sender, EventArgs e)
        {
            fm1 = (Form1)this.Owner;
        }

        private void button_accu_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfdlg = new SaveFileDialog();
            sfdlg.InitialDirectory = m_default_directory;
            sfdlg.Filter = "栅格数据|*";
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_Accu.Text = sfdlg.FileName;

            }
            
        }

        private void button_accu_save_Click(object sender, EventArgs e)
        {
            if (fm1.cost != null)
            {
                int index = textBox_Accu.Text.LastIndexOf('\\');
                string filePath = textBox_Accu.Text.Substring(0, index);
                string fileName = textBox_Accu.Text.Substring(index + 1);
                fm1.CopyRasterGridFiles(fm1.default_dir + "\\" + fm1.target_layer_name, textBox_Accu.Text);
                if (System.IO.Directory.Exists(textBox_Accu.Text))
                {
                    fm1.WriteArray2RasterFile(ref fm1.cost, rstPixelType.PT_FLOAT, filePath, fileName);
                }
            }
            else
            {
                MessageBox.Show("输入数据为空！！", "保存数据错误!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void button_parent_x_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfdlg = new SaveFileDialog();
            sfdlg.InitialDirectory = m_default_directory;
            sfdlg.Filter = "栅格数据|*";
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_ParentX.Text = sfdlg.FileName;
                
            }
            
        }

        private void button_save_x_Click(object sender, EventArgs e)
        {
            if (fm1.parentX != null)
            {
                int index = textBox_ParentX.Text.LastIndexOf('\\');
                string filePath = textBox_ParentX.Text.Substring(0, index);
                string fileName = textBox_ParentX.Text.Substring(index + 1);
                fm1.CopyRasterGridFiles(fm1.default_dir + "\\" + fm1.target_layer_name, textBox_ParentX.Text);
                if (System.IO.Directory.Exists(textBox_ParentX.Text))
                {
                    fm1.WriteArray2RasterFile(ref fm1.parentX, rstPixelType.PT_FLOAT, filePath, fileName);
                }
            }
            else
            {
                MessageBox.Show("输入数据为空！！", "保存数据错误!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void button_parent_y_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfdlg = new SaveFileDialog();
            sfdlg.Filter = "栅格数据|*";
            sfdlg.InitialDirectory = m_default_directory;
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_ParentY.Text = sfdlg.FileName;
            }
            
        }


        private void button_save_y_Click(object sender, EventArgs e)
        {
            if (fm1.parentY != null)
            {
                int index = textBox_ParentY.Text.LastIndexOf('\\');
                string filePath = textBox_ParentY.Text.Substring(0, index);
                string fileName = textBox_ParentY.Text.Substring(index + 1);
                fm1.CopyRasterGridFiles(fm1.default_dir + "\\" + fm1.target_layer_name, textBox_ParentY.Text);
                if (System.IO.Directory.Exists(textBox_ParentY.Text))
                {
                    fm1.WriteArray2RasterFile(ref fm1.parentY, rstPixelType.PT_FLOAT, filePath, fileName);
                }
            }
            else
            {
                MessageBox.Show("输入数据为空！！", "保存数据错误!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            
        }

        static bool restore_accu = false;
        private void butto_restore_accu_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox_Accu.Text))
            {
                MessageBox.Show(textBox_Accu.Text+"不存在！！", "数据打开错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int index = textBox_Accu.Text.LastIndexOf('\\');
            string filepath = textBox_Accu.Text.Substring(0, index);
            string filename = textBox_Accu.Text.Substring(index + 1);

            System.Array open_area_1;
            IRasterLayer pRLayer = fm1.OpenRasterFile(filepath, filename);
            fm1.ReadPixelValues2Array(pRLayer, out open_area_1);
            double t = 0;
            for (int i = 0; i < MyCostFunction.array_height; i++)
            {
                for (int j = 0; j < MyCostFunction.array_width; j++)
                {

                    t = Convert.ToDouble(open_area_1.GetValue(j, i));
                    if (t <= 0)
                    {
                        fm1.cost[i, j] = float.MinValue;
                    }
                    else
                    {
                        fm1.cost[i, j] = t;
                    }
                }
            }
            //Marshal.FinalReleaseComObject(open_area_1);
            open_area_1 = null;
            restore_accu = true;
        }
        static bool restore_parent_x = false;
        private void button_restore_x_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox_ParentX.Text))
            {
                MessageBox.Show(textBox_ParentX.Text + "不存在！！", "数据打开错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int index = textBox_ParentX.Text.LastIndexOf('\\');
            string filepath = textBox_ParentX.Text.Substring(0, index);
            string filename = textBox_ParentX.Text.Substring(index + 1);
            System.Array open_area_1;
            IRasterLayer pRLayer = fm1.OpenRasterFile(filepath, filename);
            fm1.ReadPixelValues2Array(pRLayer, out open_area_1);


            for (int i = 0; i < MyCostFunction.array_height; i++)
            {
                for (int j = 0; j < MyCostFunction.array_width; j++)
                {

                    double t = Convert.ToDouble(open_area_1.GetValue(j, i));
                    if (t <= 0)
                    {
                        fm1.parentX[i, j] = -1;
                    }
                    else
                    {
                        fm1.parentX[i, j] = (int)t;
                    }
                }
            }
            //Marshal.FinalReleaseComObject(open_area_1);
            open_area_1 = null;
            restore_parent_x = true;
        }

        static bool restore_parent_y = false;
        private void button_restore_y_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox_ParentY.Text))
            {
                MessageBox.Show(textBox_ParentY.Text + "不存在！！", "数据打开错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int index = textBox_ParentY.Text.LastIndexOf('\\');
            string filepath = textBox_ParentY.Text.Substring(0, index);
            string filename = textBox_ParentY.Text.Substring(index + 1);
            System.Array open_area_1;
            IRasterLayer pRLayer = fm1.OpenRasterFile(filepath, filename);
            fm1.ReadPixelValues2Array(pRLayer, out open_area_1);

            for (int i = 0; i < MyCostFunction.array_height; i++)
            {
                for (int j = 0; j < MyCostFunction.array_width; j++)
                {

                    double t = Convert.ToDouble(open_area_1.GetValue(j, i));
                    if (t <= 0)
                    {
                        fm1.parentY[i, j] = -1;
                    }
                    else
                    {
                        fm1.parentY[i, j] = (int)t;
                    }
                }
            }
            //Marshal.FinalReleaseComObject(open_area_1);
            open_area_1 = null;
            restore_parent_y = true;
        }

        private void SaveTempDataDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (restore_accu && restore_parent_x && restore_parent_y)
            {
                fm1.bool_recalculation = false;
                //MessageBox.Show("YES");
            }
        }

    }
}
