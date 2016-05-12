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
using System.Xml;
using System.IO;
using ESRI.ArcGIS.DataSourcesRaster;

namespace OpenSpaceRouting
{
    public partial class PlaneReclassify : Form
    {
        public Form1 fm1 = null;
        public string[] layer_names = null;
        double[,] impedance = null;
        string slope_out_path = null;
        public PlaneReclassify()
        {
            InitializeComponent();
        }

        public PlaneReclassify(Form fm, string []names)
        {
            InitializeComponent();
            fm1 = (Form1)fm;
            layer_names = names;
        }

        public PlaneReclassify(Form fm, string[] names,ref double[,] ipd)
        {
            InitializeComponent();
            fm1 = (Form1)fm;
            layer_names = names;
            impedance = ipd;
        }
        public void SaveXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            xNode.GetElementsByTagName("p1").Item(0).InnerText = tb1_p1.Text;
            xNode.GetElementsByTagName("p2").Item(0).InnerText = tb1_p2.Text;
            xNode.GetElementsByTagName("p3").Item(0).InnerText = tb1_p3.Text;

            xNode.GetElementsByTagName("h1").Item(0).InnerText = tb1_h1.Text;
            xNode.GetElementsByTagName("h2").Item(0).InnerText = tb1_h2.Text;
            xNode.GetElementsByTagName("h3").Item(0).InnerText = tb1_h3.Text;

            xNode.GetElementsByTagName("z1").Item(0).InnerText = tb1_z1.Text;
            xNode.GetElementsByTagName("z2").Item(0).InnerText = tb1_z2.Text;
            xNode.GetElementsByTagName("z3").Item(0).InnerText = tb1_z3.Text;

            xNode.GetElementsByTagName("d1").Item(0).InnerText = tb1_d1.Text;
            xNode.GetElementsByTagName("d2").Item(0).InnerText = tb1_d2.Text;
            xNode.GetElementsByTagName("d3").Item(0).InnerText = tb1_d3.Text;

            xNode.GetElementsByTagName("slope_path").Item(0).InnerText = tb_out_path.Text;

            XmlDoc.Save("config.xml");
        }

        public void ReadXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            tb1_p1.Text = xNode.GetElementsByTagName("p1").Item(0).InnerText;
            tb1_p2.Text = xNode.GetElementsByTagName("p2").Item(0).InnerText;
            tb1_p3.Text = xNode.GetElementsByTagName("p3").Item(0).InnerText;

            tb1_h1.Text = xNode.GetElementsByTagName("h1").Item(0).InnerText;
            tb1_h2.Text = xNode.GetElementsByTagName("h2").Item(0).InnerText;
            tb1_h3.Text = xNode.GetElementsByTagName("h3").Item(0).InnerText;

            tb1_z1.Text = xNode.GetElementsByTagName("z1").Item(0).InnerText;
            tb1_z2.Text = xNode.GetElementsByTagName("z2").Item(0).InnerText;
            tb1_z3.Text = xNode.GetElementsByTagName("z3").Item(0).InnerText;

            tb1_d1.Text = xNode.GetElementsByTagName("d1").Item(0).InnerText;
            tb1_d2.Text = xNode.GetElementsByTagName("d2").Item(0).InnerText;
            tb1_d3.Text = xNode.GetElementsByTagName("d3").Item(0).InnerText;

            tb_out_path.Text = xNode.GetElementsByTagName("slope_path").Item(0).InnerText;

        }

        private void PlaneReclassify_Load(object sender, EventArgs e)
        {
            ReadXmlFile("config.xml","/DialogResults/FormNode[name = 'PlaneReclassify']");


            if (layer_names.Length == 0)
            {
                this.comboBox1.Enabled = false;
                this.button1.Enabled = false;
            }
            else
            {
                this.comboBox1.Text = layer_names[0];
                foreach (string s in layer_names)
                {
                    if (s != null)
                    {
                        this.comboBox1.Items.Add(s);
                    }
                }
            }

            int kkk = 1;
            string default_out_path = tb_out_path.Text;
            while (Directory.Exists(default_out_path))
            {
                default_out_path = tb_out_path.Text + kkk.ToString();
                ++kkk;
            }
        }



        private void tb1_p2_TextChanged(object sender, EventArgs e)
        {
            tb1_h1.Text = tb1_p2.Text;
        }

        private void tb1_h2_TextChanged(object sender, EventArgs e)
        {
            tb1_z1.Text = tb1_h2.Text;
        }

        private void tb1_z2_TextChanged(object sender, EventArgs e)
        {
            tb1_d1.Text = tb1_z2.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tb1_p1.Text = "0";
            tb1_p2.Text = "5";
            tb1_p3.Text = "5";
            tb1_h1.Text = "6";
            tb1_h2.Text = "15";
            tb1_h3.Text = "4";
            tb1_z1.Text = "16";
            tb1_z2.Text = "30";
            tb1_z3.Text = "3";
            tb1_d1.Text = "31";
            tb1_d2.Text = "60";
            tb1_d3.Text = "2";
        }



        private void button1_Click(object sender, EventArgs e)
        {
            //if (impedance == null)
            //{
            //    MessageBox.Show("地形数据未定义！！","输入错误",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            SaveXmlFile("config.xml", "/DialogResults/FormNode[name = 'PlaneReclassify']");

            ILayer t_layer = fm1.GetLayerByName(comboBox1.Text);
            IRasterLayer pRLayer = t_layer as IRasterLayer;
            IRasterProps pRProps = (IRasterProps)pRLayer.Raster;

           
            if (Directory.Exists(tb_out_path.Text))
            {
                MessageBox.Show("输出文件已存在！！", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            double xp1 = Convert.ToDouble(tb1_p1.Text);
            double xp2 = Convert.ToDouble(tb1_p2.Text);
            double xp3 = Convert.ToDouble(tb1_p3.Text);
            double xh1 = Convert.ToDouble(tb1_h1.Text);
            double xh2 = Convert.ToDouble(tb1_h2.Text);
            double xh3 = Convert.ToDouble(tb1_h3.Text);
            double xz1 = Convert.ToDouble(tb1_z1.Text);
            double xz2 = Convert.ToDouble(tb1_z2.Text);
            double xz3 = Convert.ToDouble(tb1_z3.Text);
            double xd1 = Convert.ToDouble(tb1_d1.Text);
            double xd2 = Convert.ToDouble(tb1_d2.Text);
            double xd3 = Convert.ToDouble(tb1_d3.Text);
            int rows = pRProps.Height;
            int cols = pRProps.Width;
            //if (impedance == null)
            //{
            //    impedance = new double[rows, cols];
            //}

            System.Array array;
            fm1.ReadPixelValues2Array(pRLayer, out array);
            impedance = new double[pRProps.Height, pRProps.Width];
            fm1.SystemArray2DoubleArray(array, ref impedance);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {

                    double t = impedance[r, c];
                    if (t < 0)
                    {
                        impedance[r, c] = float.MinValue;
                        continue;
                    }
                    if (t < xp2)
                    {
                        impedance[r, c] = xp3;
                    }
                    else if (t < xh2)
                    {
                        impedance[r, c] = xh3;
                    }
                    else if (t < xz2)
                    {
                        impedance[r, c] = xz3;
                    }
                    else if (t < xd2)
                    {
                        impedance[r, c] = xd3;
                    }
                    else
                    {
                        impedance[r, c] = float.MinValue;
                    }
                }
            }
            string out_path = tb_out_path.Text;
            int index = out_path.LastIndexOf("\\");
            string filepath = out_path.Substring(0, index);
            string filename = out_path.Substring(index + 1);
            try
            {
                //fm1.CopyRasterGridFiles(fm1.default_dir + fm1.target_layer, out_path);
                fm1.CreateRasterDataset_2(filepath, filename, rstPixelType.PT_FLOAT, pRLayer.Raster, ref impedance);
                fm1.WriteArray2RasterFile(ref impedance, rstPixelType.PT_FLOAT, filepath, filename);
                fm1.OpenRasterFile(filepath, filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据读写错误！请关闭后重新打开！","ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public double[,] output_reclassify()
        {
            return impedance;
        }
        private void button_lookup_Click(object sender, EventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.FileName = slope_out_path;
            sdlg.InitialDirectory = fm1.default_dir;
            sdlg.Filter = "所有文件|*.*";
            if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_out_path.Text = sdlg.FileName;
            }
        }
    }
}
