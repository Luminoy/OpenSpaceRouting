using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;

namespace OpenSpaceRouting
{
    public partial class ColorCustomize : Form
    {
        public ColorCustomize()
        {
            InitializeComponent();
        }

        public IColor GetFromColor()
        {

            return pFromColor;
        }

        public IColor GetToColor()
        {

            return pToColor;
        }
        public IColor pFromColor = null;
        public IColor pToColor = null;
        private void ColorCustomize_Load(object sender, EventArgs e)
        {
            tb_from_red.Text = "0";
            tb_from_green.Text = "0";
            tb_from_blue.Text = "0";

            tb_to_red.Text = "0";
            tb_to_green.Text = "0";
            tb_to_blue.Text = "255";

            radio_discrete.Checked = true;
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {

            tb_from_red.Text = "0";
            tb_from_green.Text = "0";
            tb_from_blue.Text = "0";

            tb_to_red.Text = "0";
            tb_to_green.Text = "0";
            tb_to_blue.Text = "255";
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            pFromColor = new RgbColorClass();
            IRgbColor pRgbColor = pFromColor as IRgbColor;

            pRgbColor.Red = Convert.ToInt32(tb_from_red.Text);
            pRgbColor.Green = Convert.ToInt32(tb_from_green.Text);
            pRgbColor.Blue = Convert.ToInt32(tb_from_blue.Text);


            pToColor = new RgbColorClass();
            pRgbColor = pToColor as IRgbColor;

            pRgbColor.Red = Convert.ToInt32(tb_to_red.Text);
            pRgbColor.Green = Convert.ToInt32(tb_to_green.Text);
            pRgbColor.Blue = Convert.ToInt32(tb_to_blue.Text);

        }

        private void tb_from_red_Leave(object sender, EventArgs e)
        {
            string texts = tb_from_red.Text.Trim();
            //if(texts)
            int val = -1;
            try
            { 
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
           catch(Exception ex)
            {
                error_provider.SetError(tb_from_red, "不合法的输入！请输入0-255之间的数值！");
                tb_from_red.Focus();
            }

        }

        private void tb_from_green_Leave(object sender, EventArgs e)
        {
            string texts = tb_from_green.Text.Trim();
            //if(texts)
            int val = -1;
            try
            {
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
            catch (Exception ex)
            {
                error_provider.SetError(tb_from_green, "不合法的输入！请输入0-255之间的数值！");
                tb_from_green.Focus();
            }
        }

        private void tb_from_blue_Leave(object sender, EventArgs e)
        {
            string texts = tb_from_blue.Text.Trim();
            //if(texts)
            int val = -1;
            try
            {
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
            catch (Exception ex)
            {
                error_provider.SetError(tb_from_blue, "不合法的输入！请输入0-255之间的数值！");
                tb_from_blue.Focus();
            }
        }

        private void tb_to_red_Leave(object sender, EventArgs e)
        {
            string texts = tb_to_red.Text.Trim();
            //if(texts)
            int val = -1;
            try
            {
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
            catch (Exception ex)
            {
                error_provider.SetError(tb_to_red, "不合法的输入！请输入0-255之间的数值！");
                tb_to_red.Focus();
            }
        }

        private void tb_to_green_Leave(object sender, EventArgs e)
        {
            string texts = tb_to_green.Text.Trim();
            //if(texts)
            int val = -1;
            try
            {
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
            catch (Exception ex)
            {
                error_provider.SetError(tb_to_green, "不合法的输入！请输入0-255之间的数值！");
                tb_to_green.Focus();
            }
        }

        private void tb_to_blue_Leave(object sender, EventArgs e)
        {
            string texts = tb_to_blue.Text.Trim();
            //if(texts)
            int val = -1;
            try
            {
                val = Convert.ToInt32(texts);
                if (val < 0 || val > 255)
                {
                    throw new Exception();
                }
                error_provider.Clear();
            }
            catch (Exception ex)
            {
                error_provider.SetError(tb_to_blue, "不合法的输入！请输入0-255之间的数值！");
                tb_to_blue.Focus();
            }
        }
    }
}
