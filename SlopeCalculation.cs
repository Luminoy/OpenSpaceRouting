using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Xml;

namespace OpenSpaceRouting
{
    public partial class SlopeCalculation : Form
    {
        public string fullpath = null;
        public Form1 fm1 = null;
        public string[] layer_names = null;
        public double[,] slopes = null;
        public SlopeCalculation()
        {
            InitializeComponent();
        }

        public SlopeCalculation(Form1 fm)
        {
            InitializeComponent();
            fm1 = fm;
        }

        public SlopeCalculation(Form1 fm,string[] list)
        {
            InitializeComponent();
            fm1 = fm;
            layer_names = list;
        }

        public SlopeCalculation(Form1 fm, string[] list,ref double[,] terrain)
        {
            InitializeComponent();
            fm1 = fm;
            layer_names = list;
            slopes = terrain;
        }

        public void SaveXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            xNode.GetElementsByTagName("in_layer_name").Item(0).InnerText = textBox_in_layer.Text.Substring(textBox_in_layer.Text.LastIndexOf('\\') + 1);
            xNode.GetElementsByTagName("out_layer_name").Item(0).InnerText = textBox_out_layer.Text.Substring(textBox_out_layer.Text.LastIndexOf('\\') + 1);

            XmlDoc.Save(Xfilename);
        }

        public void ReadXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            textBox_in_layer.Text = fm1.default_dir + xNode.GetElementsByTagName("in_layer_name").Item(0).InnerText;
            textBox_out_layer.Text = fm1.default_dir + xNode.GetElementsByTagName("out_layer_name").Item(0).InnerText;

        }


        private void SlopeCalculation_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            comboBox1.Enabled = true;

            radioButton2.Checked = false;
            groupBox_input.Enabled = false;

            for (int i = 0; i < layer_names.Length; i++)
            {
                if (layer_names[i] != null)
                {
                    comboBox1.Items.Add(layer_names[i]);
                }
            }

            if (layer_names.Length != 0)
            {
                this.comboBox1.Text = layer_names[0];
            }
            else
            {
                this.btn_ok.Enabled = false;
            }
            ReadXmlFile("config.xml", "/DialogResults/FormNode[name = 'SlopeCalculation']");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox_input.Enabled = radioButton2.Checked;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = radioButton1.Checked;
        }

        private void textBox_in_layer_Leave(object sender, EventArgs e)
        {
            fullpath = textBox_in_layer.Text;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            IRasterLayer pRLayer = null;
            string input_path_name = null;

            if (textBox_out_layer.Text == string.Empty)
            {
                MessageBox.Show("输出路径不能为空！！","错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }

            if (Directory.Exists(textBox_out_layer.Text))
            {
                MessageBox.Show("文件已存在！！","错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }

            if (radioButton1.Checked == true)
            {
                string pa = fm1.default_dir;
                if (!pa.EndsWith("\\"))
                {
                    pa += "\\";
                }
                input_path_name = pa + comboBox1.Text;
                pRLayer = (IRasterLayer)fm1.GetLayerByName(comboBox1.Text);
            }

            if (radioButton2.Checked == true)
            {
                
                if (Directory.Exists(textBox_in_layer.Text))
                {
                    input_path_name = textBox_in_layer.Text;
                    int index = textBox_in_layer.Text.LastIndexOf('\\');
                    string in_path = textBox_in_layer.Text.Substring(0, index);
                    string in_name = textBox_in_layer.Text.Substring(index + 1);
                    pRLayer = fm1.OpenRasterFile(in_path, in_name);
                }
            }

            
            if (pRLayer != null)
            {
                int index = textBox_out_layer.Text.LastIndexOf('\\');
                string out_path = textBox_out_layer.Text.Substring(0, index);
                string out_name = textBox_out_layer.Text.Substring(index + 1);

                IRasterProps property = (IRasterProps)pRLayer.Raster;
                System.Array array;
                fm1.ReadPixelValues2Array(pRLayer, out array);
                double[,] dems = new double[property.Height, property.Width];
                fm1.SystemArray2DoubleArray(array, ref dems);

                slopes = slope_calculation(ref dems, property.Height, property.Width, (int)property.MeanCellSize().X, (int)property.MeanCellSize().Y);
                fm1.CreateRasterDataset_2(out_path, out_name, ESRI.ArcGIS.Geodatabase.rstPixelType.PT_FLOAT, pRLayer.Raster, ref slopes);
                //fm1.CopyRasterGridFiles(input_path_name, textBox_out_layer.Text);
                fm1.WriteArray2RasterFile(ref slopes, ESRI.ArcGIS.Geodatabase.rstPixelType.PT_FLOAT, out_path, out_name);
                fm1.OpenRasterFile(out_path, out_name);
            }
            else
            {
                MessageBox.Show("输入栅格不能为空!","错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            SaveXmlFile("config.xml", "/DialogResults/FormNode[name = 'SlopeCalculation']");
            this.Close();
        }

        public double[,] get_slopes_arrays()
        {
            return slopes;
        }

        int kk = 0;
        public double[,] slope_calculation(ref double[,] dems, int rows, int columns, int cell_x, int cell_y)
        {
            double[,] output = new double[rows, columns];
            double min = -10000;
            double[,] win = new double[3, 3];
            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 1; j < columns - 1; j++)
                {
                    if (dems[i, j] < min)
                    {
                        output[i, j] = float.MinValue;
                        continue;
                    }
                    //int urow = i - 1;
                    //int drow = i + 1;
                    //int ucol = j - 1;
                    //int dcol = j + 1;
                    //if (dems[i - 1, j - 1] < min || dems[i, j - 1] < min || dems[i + 1, j - 1] < min || dems[i - 1, j] < min || dems[i + 1, j] < min || dems[i - 1, j - 1] < min || dems[i, j - 1] < min || dems[i + 1, j - 1] < min)
                    //{
                    //    output[i, j] = 90;
                    //    continue;
                    //}
                    for (int m = -1; m <= 1; m++)
                    {
                        for (int n = -1; n <= 1; n++)
                        {
                            if (dems[i + m, j + n] < min)
                                win[m + 1, n + 1] = 0;
                            else
                                win[m + 1, n + 1] = dems[i + m, j + n];
                        }
                    }
                    //double dz_dx = ((dems[i - 1, j + 1] + 2 * dems[i, j + 1] + dems[i + 1, j + 1]) - (dems[i - 1, j - 1] + 2 * dems[i, j - 1] + dems[i + 1, j - 1])) / (8 * cell_x);
                    //double dz_dy = ((dems[i + 1, j - 1] + 2 * dems[i + 1, j] + dems[i + 1, j + 1]) - (dems[i - 1, j - 1] + 2 * dems[i - 1, j] + dems[i - 1, j + 1])) / (8 * cell_y);

                    double dz_dx = ((win[0, 2] + 2 * win[1, 2] + win[2, 2]) - (win[0, 0] + 2 * win[1, 0] + win[2, 0])) / (8 * cell_x);
                    double dz_dy = ((win[2, 0] + 2 * win[2, 1] + win[2, 2]) - (win[0, 0] + 2 * win[0, 1] + win[0, 2])) / (8 * cell_y);
                    output[i, j] = (Math.Atan(Math.Sqrt(dz_dx * dz_dx + dz_dy * dz_dy)) * 180 / Math.PI);
                    if (output[i, j] > 90 || output[i, j] < 0)
                    {
                        kk++;
                    }
                }
            }

            Console.WriteLine("kk: "+kk.ToString());
            for (int i = 1; i < rows - 1; i++)
            {
                output[i, 0] = output[i, 1];
                output[i, columns - 1] = output[i, columns - 2];
            }

            for (int j = 0; j < columns; j++)
            {
                output[0, j] = output[1, j];
                output[rows - 1, j] = output[rows - 2, j];
            }

            return output;
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            layer_names = fm1.GetRasterLayerList();
            for (int i = 0; i < layer_names.Length; i++)
            {
                if (layer_names[i] != null)
                {
                    comboBox1.Items.Add(layer_names[i]);
                }
                
            }
        }

        private void btn_path_2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.FileName = "slope_out";
            sdlg.InitialDirectory = fm1.default_dir;
            sdlg.Filter = "所有文件|*.*";
            if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_out_layer.Text = sdlg.FileName;
            }
            sdlg = null;
        }

        private void btn_path_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog odlg = new FolderBrowserDialog();
            odlg.SelectedPath = "E:\\TEST\\slope_out";
            if (odlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_in_layer.Text = odlg.SelectedPath;
            }
            odlg = null;
        }


    }
}
