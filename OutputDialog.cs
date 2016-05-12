using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace OpenSpaceRouting
{
    public partial class OutputDialog : Form
    {
        string m_default_dir = "";
        public OutputDialog()
        {
            InitializeComponent();
        }

        public OutputDialog(string _default_dir)
        {
            InitializeComponent();
            m_default_dir = _default_dir;

        }
        private void OutputDialog_Load(object sender, EventArgs e)
        {
            ReadXmlFile("config.xml", "/DialogResults/FormNode[name='OutputDialog']");
        }

        private void ReadXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)XmlDoc.SelectSingleNode(Nodename);

            tb_start_point.Text = xNode.GetElementsByTagName("start_pt").Item(0).InnerText;
            tb_end_point.Text = xNode.GetElementsByTagName("end_pt").Item(0).InnerText;
            tb_target_layer.Text = xNode.GetElementsByTagName("target").Item(0).InnerText;
            tb_obstacle_layer.Text = xNode.GetElementsByTagName("obstacle").Item(0).InnerText;

            tb_outpath.Text = xNode.GetElementsByTagName("out_path_name").Item(0).InnerText;
            tb_accu.Text = xNode.GetElementsByTagName("accu_name").Item(0).InnerText;
            tb_parent_x.Text = xNode.GetElementsByTagName("p_x_name").Item(0).InnerText;
            tb_parent_y.Text = xNode.GetElementsByTagName("p_y_name").Item(0).InnerText;

        }

        private void SaveXmlFile(string Xfilename, string Nodename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Xfilename);
            XmlElement xNode = (XmlElement)xmlDoc.SelectSingleNode(Nodename);

            xNode.GetElementsByTagName("start_pt").Item(0).InnerText = tb_start_point.Text;
            xNode.GetElementsByTagName("end_pt").Item(0).InnerText = tb_end_point.Text;
            xNode.GetElementsByTagName("target").Item(0).InnerText = tb_target_layer.Text;
            xNode.GetElementsByTagName("obstacle").Item(0).InnerText = tb_obstacle_layer.Text;

            xNode.GetElementsByTagName("out_path_name").Item(0).InnerText = tb_outpath.Text;
            xNode.GetElementsByTagName("accu_name").Item(0).InnerText = tb_accu.Text;
            xNode.GetElementsByTagName("p_x_name").Item(0).InnerText = tb_parent_x.Text;
            xNode.GetElementsByTagName("p_y_name").Item(0).InnerText = tb_parent_y.Text;

            xmlDoc.Save(Xfilename);
        }

        private void button_accudlg_Click(object sender, EventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.FileName = "accumulation";
            sdlg.InitialDirectory = m_default_dir;
            sdlg.Filter = "所有文件|*.*";
            if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_accu.Text = sdlg.FileName.Substring(sdlg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_pxdlg_Click(object sender, EventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.FileName = "parent_x";
            sdlg.InitialDirectory = m_default_dir;
            sdlg.Filter = "所有文件|*.*";
            if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_parent_x.Text = sdlg.FileName.Substring(sdlg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_pydlg_Click(object sender, EventArgs e)
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.FileName = "parent_y";
            sdlg.InitialDirectory = m_default_dir;
            sdlg.Filter = "所有文件|*.*";
            if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_parent_y.Text = sdlg.FileName.Substring(sdlg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(m_default_dir))
            {
                MessageBox.Show("指定的默认路径不存在！请重新选择！", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (Directory.Exists(m_default_dir + tb_accu.Text))
            {
                MessageBox.Show("路径" + m_default_dir + tb_accu.Text + "已存在！请重新选择！", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (Directory.Exists(m_default_dir + tb_parent_x.Text))
            {
                MessageBox.Show("路径" + m_default_dir + tb_parent_x.Text + "已存在！请重新选择！", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (Directory.Exists(m_default_dir + tb_parent_y.Text))
            {
                MessageBox.Show("路径" + m_default_dir + tb_parent_y.Text + "已存在！请重新选择！", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            SaveXmlFile("config.xml", "/DialogResults/FormNode[name='OutputDialog']");
        }

        private void button_pathdlg_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfg = new SaveFileDialog();
            sfg.InitialDirectory = m_default_dir;
            sfg.Title = "Save Path PolyLine Feature";
            sfg.Filter = @"Shapefile(*.shp)|*.shp";
            sfg.DefaultExt = ".shp";
            if (sfg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_outpath.Text = sfg.FileName.Substring(sfg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_sptdlg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            ofg.InitialDirectory = m_default_dir;
            ofg.Title = "Start Point Feature";
            ofg.Filter = @"Shapefile(*.shp)|*.shp";
            ofg.DefaultExt = ".shp";
            if (ofg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_start_point.Text = ofg.FileName.Substring(ofg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_eptdlg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            ofg.InitialDirectory = m_default_dir;
            ofg.Title = "End Point Feature";
            ofg.Filter = @"Shapefile(*.shp)|*.shp";
            ofg.DefaultExt = ".shp";
            if (ofg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_end_point.Text = ofg.FileName.Substring(ofg.FileName.LastIndexOf('\\') + 1);
            }
        }

        private void button_tlayerdlg_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sfdlg = new FolderBrowserDialog();
            sfdlg.SelectedPath = m_default_dir;
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fullpath = sfdlg.SelectedPath;
                int index = fullpath.LastIndexOf('\\');
                string filePath = fullpath.Substring(0, index);
                string fileName = fullpath.Substring(index + 1);
                if (!Directory.Exists(m_default_dir + fileName))
                {
                    MessageBox.Show("工作路径下该数据不存在！请重新选择", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tb_target_layer.Text = fileName;
            }
        }

        private void btn_obstacle_odlg_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sfdlg = new FolderBrowserDialog();
            sfdlg.SelectedPath = m_default_dir;
            if (sfdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fullpath = sfdlg.SelectedPath;
                int index = fullpath.LastIndexOf('\\');
                string filePath = fullpath.Substring(0, index);
                string fileName = fullpath.Substring(index + 1);
                if (!Directory.Exists(m_default_dir + fileName))
                {
                    MessageBox.Show("工作路径下该数据不存在！请重新选择", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tb_obstacle_layer.Text = fileName;
            }
        }

    }
}
