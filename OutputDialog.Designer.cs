namespace OpenSpaceRouting
{
    partial class OutputDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_pathdlg = new System.Windows.Forms.Button();
            this.tb_outpath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_pydlg = new System.Windows.Forms.Button();
            this.tb_parent_y = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_pxdlg = new System.Windows.Forms.Button();
            this.tb_parent_x = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button_accudlg = new System.Windows.Forms.Button();
            this.tb_accu = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_CANCEL = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_eptdlg = new System.Windows.Forms.Button();
            this.tb_end_point = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_sptdlg = new System.Windows.Forms.Button();
            this.tb_start_point = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_tlayerdlg = new System.Windows.Forms.Button();
            this.tb_target_layer = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_obstacle_odlg = new System.Windows.Forms.Button();
            this.tb_obstacle_layer = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_pathdlg);
            this.groupBox2.Controls.Add(this.tb_outpath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.button_pydlg);
            this.groupBox2.Controls.Add(this.tb_parent_y);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.button_pxdlg);
            this.groupBox2.Controls.Add(this.tb_parent_x);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.button_accudlg);
            this.groupBox2.Controls.Add(this.tb_accu);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(16, 235);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(426, 205);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出文件";
            // 
            // button_pathdlg
            // 
            this.button_pathdlg.Location = new System.Drawing.Point(336, 34);
            this.button_pathdlg.Name = "button_pathdlg";
            this.button_pathdlg.Size = new System.Drawing.Size(69, 20);
            this.button_pathdlg.TabIndex = 11;
            this.button_pathdlg.Text = "浏览(…)";
            this.button_pathdlg.UseVisualStyleBackColor = true;
            this.button_pathdlg.Click += new System.EventHandler(this.button_pathdlg_Click);
            // 
            // tb_outpath
            // 
            this.tb_outpath.Location = new System.Drawing.Point(113, 35);
            this.tb_outpath.Name = "tb_outpath";
            this.tb_outpath.Size = new System.Drawing.Size(192, 21);
            this.tb_outpath.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "矢量路径:";
            // 
            // button_pydlg
            // 
            this.button_pydlg.Location = new System.Drawing.Point(336, 155);
            this.button_pydlg.Name = "button_pydlg";
            this.button_pydlg.Size = new System.Drawing.Size(69, 21);
            this.button_pydlg.TabIndex = 8;
            this.button_pydlg.Text = "浏览(…)";
            this.button_pydlg.UseVisualStyleBackColor = true;
            this.button_pydlg.Click += new System.EventHandler(this.button_pydlg_Click);
            // 
            // tb_parent_y
            // 
            this.tb_parent_y.Location = new System.Drawing.Point(113, 157);
            this.tb_parent_y.Name = "tb_parent_y";
            this.tb_parent_y.Size = new System.Drawing.Size(192, 21);
            this.tb_parent_y.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 159);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "父节点Y值:";
            // 
            // button_pxdlg
            // 
            this.button_pxdlg.Location = new System.Drawing.Point(336, 115);
            this.button_pxdlg.Name = "button_pxdlg";
            this.button_pxdlg.Size = new System.Drawing.Size(69, 21);
            this.button_pxdlg.TabIndex = 5;
            this.button_pxdlg.Text = "浏览(…)";
            this.button_pxdlg.UseVisualStyleBackColor = true;
            this.button_pxdlg.Click += new System.EventHandler(this.button_pxdlg_Click);
            // 
            // tb_parent_x
            // 
            this.tb_parent_x.Location = new System.Drawing.Point(113, 116);
            this.tb_parent_x.Name = "tb_parent_x";
            this.tb_parent_x.Size = new System.Drawing.Size(192, 21);
            this.tb_parent_x.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 118);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "父节点X值:";
            // 
            // button_accudlg
            // 
            this.button_accudlg.Location = new System.Drawing.Point(336, 75);
            this.button_accudlg.Name = "button_accudlg";
            this.button_accudlg.Size = new System.Drawing.Size(69, 20);
            this.button_accudlg.TabIndex = 2;
            this.button_accudlg.Text = "浏览(…)";
            this.button_accudlg.UseVisualStyleBackColor = true;
            this.button_accudlg.Click += new System.EventHandler(this.button_accudlg_Click);
            // 
            // tb_accu
            // 
            this.tb_accu.Location = new System.Drawing.Point(113, 76);
            this.tb_accu.Name = "tb_accu";
            this.tb_accu.Size = new System.Drawing.Size(192, 21);
            this.tb_accu.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "累计值:";
            // 
            // button_CANCEL
            // 
            this.button_CANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_CANCEL.Location = new System.Drawing.Point(216, 469);
            this.button_CANCEL.Name = "button_CANCEL";
            this.button_CANCEL.Size = new System.Drawing.Size(85, 21);
            this.button_CANCEL.TabIndex = 15;
            this.button_CANCEL.Text = "取消(&C)";
            this.button_CANCEL.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(336, 469);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 22);
            this.button_OK.TabIndex = 14;
            this.button_OK.Text = "确定(&O)";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_eptdlg
            // 
            this.button_eptdlg.Location = new System.Drawing.Point(336, 67);
            this.button_eptdlg.Name = "button_eptdlg";
            this.button_eptdlg.Size = new System.Drawing.Size(68, 20);
            this.button_eptdlg.TabIndex = 22;
            this.button_eptdlg.Text = "浏览(…)";
            this.button_eptdlg.UseVisualStyleBackColor = true;
            this.button_eptdlg.Click += new System.EventHandler(this.button_eptdlg_Click);
            // 
            // tb_end_point
            // 
            this.tb_end_point.Location = new System.Drawing.Point(114, 67);
            this.tb_end_point.Name = "tb_end_point";
            this.tb_end_point.Size = new System.Drawing.Size(200, 21);
            this.tb_end_point.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "终止点：";
            // 
            // button_sptdlg
            // 
            this.button_sptdlg.Location = new System.Drawing.Point(336, 29);
            this.button_sptdlg.Name = "button_sptdlg";
            this.button_sptdlg.Size = new System.Drawing.Size(68, 21);
            this.button_sptdlg.TabIndex = 19;
            this.button_sptdlg.Text = "浏览(…)";
            this.button_sptdlg.UseVisualStyleBackColor = true;
            this.button_sptdlg.Click += new System.EventHandler(this.button_sptdlg_Click);
            // 
            // tb_start_point
            // 
            this.tb_start_point.Location = new System.Drawing.Point(114, 30);
            this.tb_start_point.Name = "tb_start_point";
            this.tb_start_point.Size = new System.Drawing.Size(200, 21);
            this.tb_start_point.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "起始点：";
            // 
            // button_tlayerdlg
            // 
            this.button_tlayerdlg.Location = new System.Drawing.Point(336, 105);
            this.button_tlayerdlg.Name = "button_tlayerdlg";
            this.button_tlayerdlg.Size = new System.Drawing.Size(68, 20);
            this.button_tlayerdlg.TabIndex = 25;
            this.button_tlayerdlg.Text = "浏览(…)";
            this.button_tlayerdlg.UseVisualStyleBackColor = true;
            this.button_tlayerdlg.Click += new System.EventHandler(this.button_tlayerdlg_Click);
            // 
            // tb_target_layer
            // 
            this.tb_target_layer.Location = new System.Drawing.Point(113, 105);
            this.tb_target_layer.Name = "tb_target_layer";
            this.tb_target_layer.Size = new System.Drawing.Size(200, 21);
            this.tb_target_layer.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 23;
            this.label4.Text = "耗费栅格：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_obstacle_odlg);
            this.groupBox1.Controls.Add(this.tb_obstacle_layer);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.button_tlayerdlg);
            this.groupBox1.Controls.Add(this.tb_target_layer);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.button_eptdlg);
            this.groupBox1.Controls.Add(this.tb_end_point);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.button_sptdlg);
            this.groupBox1.Controls.Add(this.tb_start_point);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(16, 23);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(426, 192);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入数据";
            // 
            // btn_obstacle_odlg
            // 
            this.btn_obstacle_odlg.Location = new System.Drawing.Point(335, 143);
            this.btn_obstacle_odlg.Name = "btn_obstacle_odlg";
            this.btn_obstacle_odlg.Size = new System.Drawing.Size(69, 22);
            this.btn_obstacle_odlg.TabIndex = 28;
            this.btn_obstacle_odlg.Text = "浏览(…)";
            this.btn_obstacle_odlg.UseVisualStyleBackColor = true;
            this.btn_obstacle_odlg.Click += new System.EventHandler(this.btn_obstacle_odlg_Click);
            // 
            // tb_obstacle_layer
            // 
            this.tb_obstacle_layer.Location = new System.Drawing.Point(113, 144);
            this.tb_obstacle_layer.Name = "tb_obstacle_layer";
            this.tb_obstacle_layer.Size = new System.Drawing.Size(199, 21);
            this.tb_obstacle_layer.TabIndex = 27;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 148);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 12);
            this.label10.TabIndex = 26;
            this.label10.Text = "障碍物(可选)：";
            // 
            // OutputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 514);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_CANCEL);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox2);
            this.Name = "OutputDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "运行参数设置";
            this.Load += new System.EventHandler(this.OutputDialog_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_pydlg;
        private System.Windows.Forms.TextBox tb_parent_y;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_pxdlg;
        private System.Windows.Forms.TextBox tb_parent_x;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_accudlg;
        private System.Windows.Forms.TextBox tb_accu;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_CANCEL;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_eptdlg;
        private System.Windows.Forms.TextBox tb_end_point;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_sptdlg;
        private System.Windows.Forms.TextBox tb_start_point;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_pathdlg;
        private System.Windows.Forms.TextBox tb_outpath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_tlayerdlg;
        private System.Windows.Forms.TextBox tb_target_layer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_obstacle_odlg;
        private System.Windows.Forms.TextBox tb_obstacle_layer;
        private System.Windows.Forms.Label label10;
    }
}