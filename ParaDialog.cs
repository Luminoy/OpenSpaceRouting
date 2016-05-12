using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Xml;
using System.IO;

namespace OpenSpaceRouting
{
    public partial class ParaDialog : Form
    {
        public Form1 fm1 = null;
        public string m_default_dir = null;
        public string m_target_layer = null;
        public IRasterLayer pRasLayer = null;
        public ParaDialog()
        {
            InitializeComponent();
        }


        public ParaDialog(Form1 mfrm)
        {
            InitializeComponent();
            fm1 = mfrm;
        }

        private void ReadXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            tb_default_dir.Text = xNode.GetElementsByTagName("default_dir").Item(0).InnerText;
            tb_target_layer.Text = xNode.GetElementsByTagName("target").Item(0).InnerText;
            //tb_terrain_layer.Text = xNode.GetElementsByTagName("terrain").Item(0).InnerText;

            tb_start_point.Text = xNode.GetElementsByTagName("start_pt").Item(0).InnerText;
            tb_end_point.Text = xNode.GetElementsByTagName("end_pt").Item(0).InnerText;

            tb_cellsize_x.Text = xNode.GetElementsByTagName("cell_size_x").Item(0).InnerText;
            tb_cellsize_y.Text = xNode.GetElementsByTagName("cell_size_y").Item(0).InnerText;
            tb_rows.Text = xNode.GetElementsByTagName("row_counts").Item(0).InnerText;
            tb_columns.Text = xNode.GetElementsByTagName("column_counts").Item(0).InnerText;

        }

        private void SaveXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)xmlDoc.SelectSingleNode(Nodename);

            xNode.GetElementsByTagName("default_dir").Item(0).InnerText = tb_default_dir.Text;
            xNode.GetElementsByTagName("target").Item(0).InnerText = tb_target_layer.Text;
            //xNode.GetElementsByTagName("terrain").Item(0).InnerText = tb_terrain_layer.Text;

            xNode.GetElementsByTagName("start_pt").Item(0).InnerText = tb_start_point.Text;
            xNode.GetElementsByTagName("end_pt").Item(0).InnerText = tb_end_point.Text;

            xNode.GetElementsByTagName("cell_size_x").Item(0).InnerText = tb_cellsize_x.Text;
            xNode.GetElementsByTagName("cell_size_y").Item(0).InnerText = tb_cellsize_y.Text;
            xNode.GetElementsByTagName("row_counts").Item(0).InnerText = tb_rows.Text;
            xNode.GetElementsByTagName("column_counts").Item(0).InnerText = tb_columns.Text;
            
            xmlDoc.Save(Xfilename);
                
        }

        private void ParaDialog_Load(object sender, EventArgs e)
        {
            ReadXmlFile("config.xml", "/DialogResults/FormNode[name = 'ParaDialog']");
        }

        private void button_target_layer_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sfdlg = new FolderBrowserDialog();
            sfdlg.SelectedPath = m_default_dir;
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_target_layer.Text = sfdlg.SelectedPath;
                int index = tb_target_layer.Text.LastIndexOf('\\');
                string filePath = tb_target_layer.Text.Substring(0, index);
                string fileName = tb_target_layer.Text.Substring(index + 1);
                tb_target_layer.Text = fileName;
                try
                {
                    pRasLayer = fm1.OpenRasterFile(filePath, fileName, false);
                    IRasterProps pRasProps = pRasLayer.Raster as IRasterProps;
                    tb_rows.Text = pRasProps.Height.ToString();
                    tb_columns.Text = pRasProps.Width.ToString();
                    tb_cellsize_x.Text = pRasProps.MeanCellSize().X.ToString();
                    tb_cellsize_y.Text = pRasProps.MeanCellSize().Y.ToString();
                    tb_rows.Enabled = true;
                    tb_columns.Enabled = true;
                    tb_cellsize_x.Enabled = true;
                    tb_cellsize_y.Enabled = true;
                }
                catch (Exception ex)
                {
                    tb_rows.Enabled = false;
                    tb_columns.Enabled = false;
                    tb_cellsize_x.Enabled = false;
                    tb_cellsize_y.Enabled = false;
                }
            }
        }

        private void button_opendlg_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = m_default_dir;

            if (DialogResult.OK == fbd.ShowDialog())
            {
                m_default_dir = fbd.SelectedPath;
                tb_default_dir.Text = fbd.SelectedPath + "\\";
            }
        }

        //private void btn_tarrain_odlg_Click(object sender, EventArgs e)
        //{
        //    FolderBrowserDialog sfdlg = new FolderBrowserDialog();
        //    sfdlg.SelectedPath = m_default_dir;
        //    if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        tb_terrain_layer.Text = sfdlg.SelectedPath.Substring(sfdlg.SelectedPath.LastIndexOf('\\') + 1);
        //    }
        //}

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(tb_default_dir.Text))
            {
                MessageBox.Show("指定的默认路径不存在！请重新选择！","路径错误",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            SaveXmlFile("config.xml","/DialogResults/FormNode[name = 'ParaDialog']");
        }



        private void tb_target_layer_Leave(object sender, EventArgs e)
        {
            string filepath = m_default_dir;
            if (!filepath.EndsWith("\\"))
            {
                filepath += "\\";
            }
            if (System.IO.Directory.Exists(filepath + tb_target_layer.Text))
            {
                pRasLayer = fm1.OpenRasterFile(m_default_dir, tb_target_layer.Text, false);
                m_target_layer = tb_target_layer.Text;
                IRasterProps pRasProps = pRasLayer.Raster as IRasterProps;
                tb_rows.Text = pRasProps.Height.ToString();
                tb_columns.Text = pRasProps.Width.ToString();
                tb_cellsize_x.Text = pRasProps.MeanCellSize().X.ToString();
                tb_cellsize_y.Text = pRasProps.MeanCellSize().Y.ToString();
                tb_rows.Enabled = true;
                tb_columns.Enabled = true;
                tb_cellsize_x.Enabled = true;
                tb_cellsize_y.Enabled = true;
            }
            else
            {
                tb_rows.Enabled = false;
                tb_columns.Enabled = false;
                tb_cellsize_x.Enabled = false;
                tb_cellsize_y.Enabled = false;

            }
        }

        private void tb_default_dir_Leave(object sender, EventArgs e)
        {
            m_default_dir = tb_default_dir.Text;
            //tb_target_layer_Leave(sender, e);
        }

        private void button_sptdlg_Click(object sender, EventArgs e)
        {

        }

        private void button_eptdlg_Click(object sender, EventArgs e)
        {

        }


    }
}
