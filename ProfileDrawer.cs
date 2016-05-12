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
    public partial class ProfileDrawer : Form
    {
        double[] pixels = null;
        public ProfileDrawer()
        {
            InitializeComponent(); 
            //pixels = new double[100];
            //Random rnd = new Random();
            
            //for (int i = 0; i < 100; i++)
            //{
            //    pixels[i] = i;// +rnd.Next(50, 100);
            //}
        }

        public ProfileDrawer(ref double[] pt)
        {
            InitializeComponent();

            pixels = pt;
        }

        private void ProfileDrawer_Load(object sender, EventArgs e)
        {
            //画出坐标系
            old_height = this.Height - 40;
            old_width = this.Width;
        }

        //class ChartMargin
        //{
        //    //public ChartMargin(int u = 0, int r = 0, int b = 0, int l = 0)
        //    //{
        //    //    upper = u;
        //    //    right = r;
        //    //    bottom = b;
        //    //    left = l;
        //    //}
        //    public int upper, right, bottom, left;
        //}

        double factor_x = 1;
        double factor_y = 1;
        int old_width = 0;
        int old_height = 0;
        private void ProfileDrawer_Paint(object sender, PaintEventArgs e)
        {
            
            int width = this.Width;
            int height = this.Height - 40; //标题栏高40

            factor_x = width / (double)old_width;
            factor_y = height / (double)old_height;

            Console.WriteLine(factor_y);
            //ChartMargin cmargin = new ChartMargin(50, 50, 50, 50);
            int d_horizonal = 50;
            int d_vertical = (int)(height * 0.05);

            
            Rectangle margin = new Rectangle(d_horizonal, d_vertical, width - 2 * d_horizonal, height - 2 * d_vertical);

            Point origin = new Point(margin.Left, margin.Bottom);
            //this.label1.Text = "(0,0)";
            //this.label1.Left = origin.X - this.label1.Size.Width;
            //this.label1.Top = origin.Y - this.label1.Size.Height;
            //MessageBox.Show(origin.X + "," + origin.Y);

            Graphics g = e.Graphics;
            Pen axis = new Pen(Brushes.Black, 1);

            g.DrawLine(axis, origin.X, origin.Y, margin.X, margin.Y);
            g.DrawLine(axis, origin.X, origin.Y, margin.X + margin.Width, margin.Y + margin.Height);

            g.DrawLine(axis, margin.X, margin.Y, margin.X + 4, margin.Y + 4);
            g.DrawLine(axis, margin.X, margin.Y, margin.X - 4, margin.Y + 4);
            g.DrawLine(axis, margin.X + margin.Width, margin.Y + margin.Height, margin.X + margin.Width - 4, margin.Y + margin.Height + 4);
            g.DrawLine(axis, margin.X + margin.Width, margin.Y + margin.Height, margin.X + margin.Width - 4, margin.Y + margin.Height - 4);
            //g.DrawRectangle(axis, margin);

            int scales = 10;
            int length_data = pixels.Length;
            int interval_x = margin.Width / scales;

            Font my_font = new Font("宋体",11, FontStyle.Regular);
            for (int i = 0; i < scales; i++)
            {
                g.DrawLine(axis, origin.X + i * interval_x, origin.Y, origin.X +  i * interval_x, origin.Y - 5);
                g.DrawString((i* (length_data / scales)).ToString(), my_font, Brushes.Black, origin.X + i * interval_x - 5, origin.Y + 5);
            }
            g.DrawString("x", new Font("宋体", 12, FontStyle.Bold), Brushes.Black, origin.X + margin.Width - 8, origin.Y + 4);


            double max_value = -1;
            for (int i = 0; i < length_data; i++)
            {
                //pt[i].X = origin.X + i * margin.Width / length_data;
                //pt[i].Y = origin.Y - (int)(pixels[i] * factor_y);
                if (max_value < pixels[i])
                {
                    max_value = pixels[i];
                }
            }

            scales = 5;
            int interval_y = (int)(margin.Height / scales);
            double value_factor = margin.Height / max_value; 
            //Console.WriteLine(interval_y);
            for (int i = 0; i < scales; i++)
            {
                //Console.WriteLine(i * interval_y + ", " + margin.Height / max_value);
                g.DrawLine(axis, origin.X, origin.Y - i * interval_y, origin.X + 5, origin.Y - i * interval_y);
                g.DrawString((i * (max_value / scales)).ToString(), my_font, Brushes.Black, 0, origin.Y - i * interval_y - 2);
            }

            Point[] pt = new Point[length_data];
            for (int i = 0; i < length_data; i++)
            {
                pt[i].X = origin.X + i * margin.Width / length_data;
                pt[i].Y = origin.Y - (int)(pixels[i] * value_factor);
                //Console.WriteLine(pixels[i] + ", " + value_factor + "," + pixels[i] * value_factor);
            }

            g.DrawCurve(axis, pt,0.1f);

            old_height = height;
            old_width = width;
            
                //g.DrawLine(axis, origin.X, origin.Y, margin.Left,margin.Top);
                //g.DrawLine(axis, 50, 240, 50, 30);
                //g.DrawLine(axis, 100, 240, 100, 242);
                //g.DrawLine(axis, 150, 240, 150, 242);
                //g.DrawLine(axis, 200, 240, 200, 242);
                //g.DrawLine(axis, 250, 240, 250, 242);
                //g.DrawLine(axis, 300, 240, 300, 242);

                //g.DrawString("0", new Font("New Timer", 8), Brushes.Black, new PointF(46, 242));
                //g.DrawString("50", new Font("New Timer", 8), Brushes.Black, new PointF(92, 242));
                //g.DrawString("100", new Font("New Timer", 8), Brushes.Black, new PointF(139, 242));
                //g.DrawString("150", new Font("New Timer", 8), Brushes.Black, new PointF(189, 242));
                //g.DrawString("200", new Font("New Timer", 8), Brushes.Black, new PointF(239, 242));
                //g.DrawString("250", new Font("New Timer", 8), Brushes.Black, new PointF(289, 242));
                //g.DrawLine(axis, 48, 40, 50, 40);
                //g.DrawString("0", new Font("New Timer", 8), Brushes.Black, new PointF(34, 234));
                //g.DrawString(100.ToString(), new Font("New Timer", 8), Brushes.Black, new PointF(18, 34));

                //double temp = 0;
                //for (int i = 0; i < 256; i++)
                //{
                //    temp = 200.0 * countPixel[i] / maxPixel;
                //    g.DrawLine(curPen, 50 + i, 240, 50 + i, 240 - (int)temp);
                //}

                axis.Dispose();
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void ProfileDrawer_MouseMove(object sender, MouseEventArgs e)
        {
            //label1.Text = e.X + "," + e.Y;
        }

        private void ProfileDrawer_SizeChanged(object sender, EventArgs e)
        {
            
        }
    }
}
