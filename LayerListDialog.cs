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
    public partial class LayerListDialog : Form
    {
        string[] layer_names = null;
        int layer_count = 0;

        public LayerListDialog()
        {
            InitializeComponent();
        }
        public LayerListDialog(string[] in_layer_names)
        {
            InitializeComponent();
            layer_names = in_layer_names;
        }
        public LayerListDialog(string[] in_layer_names, int in_layer_count)
        {
            InitializeComponent();
            layer_names = in_layer_names;
            layer_count = in_layer_count;
        }
        private void LayerListDialog_Load(object sender, EventArgs e)
        {
            if (layer_names.Length > 0)
            {
                for (int i = 0; i < layer_names.Length; i++)
                {
                    if (layer_names[i] != null)
                    {
                        comboBox1.Items.Add(layer_names[i]);
                        comboBox1.Text = layer_names[i];
                    }
                }
            }
            else
            {
                comboBox1.Enabled = false;
                button1.Enabled = false;
            }
        }
        public string GetReturnString()
        {
            if(comboBox1.Enabled && comboBox1.Text != null)
                return comboBox1.Text;
            return null;
        }
    }
}
