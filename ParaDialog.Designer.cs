namespace OpenSpaceRouting
{
    partial class ParaDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.tb_default_dir = new System.Windows.Forms.TextBox();
            this.button_opendlg = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_target_layer = new System.Windows.Forms.TextBox();
            this.button_tlayerdlg = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_cellsize_x = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_cellsize_y = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tb_columns = new System.Windows.Forms.TextBox();
            this.tb_rows = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_CANCEL = new System.Windows.Forms.Button();
            this.button_eptdlg = new System.Windows.Forms.Button();
            this.tb_end_point = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_sptdlg = new System.Windows.Forms.Button();
            this.tb_start_point = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "默认路径：";
            // 
            // tb_default_dir
            // 
            this.tb_default_dir.Location = new System.Drawing.Point(103, 32);
            this.tb_default_dir.Name = "tb_default_dir";
            this.tb_default_dir.Size = new System.Drawing.Size(200, 21);
            this.tb_default_dir.TabIndex = 1;
            this.tb_default_dir.Leave += new System.EventHandler(this.tb_default_dir_Leave);
            // 
            // button_opendlg
            // 
            this.button_opendlg.Location = new System.Drawing.Point(325, 31);
            this.button_opendlg.Name = "button_opendlg";
            this.button_opendlg.Size = new System.Drawing.Size(70, 21);
            this.button_opendlg.TabIndex = 2;
            this.button_opendlg.Text = "浏览(…)";
            this.button_opendlg.UseVisualStyleBackColor = true;
            this.button_opendlg.Click += new System.EventHandler(this.button_opendlg_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标图层：";
            // 
            // tb_target_layer
            // 
            this.tb_target_layer.Location = new System.Drawing.Point(103, 69);
            this.tb_target_layer.Name = "tb_target_layer";
            this.tb_target_layer.Size = new System.Drawing.Size(200, 21);
            this.tb_target_layer.TabIndex = 4;
            this.tb_target_layer.Leave += new System.EventHandler(this.tb_target_layer_Leave);
            // 
            // button_tlayerdlg
            // 
            this.button_tlayerdlg.Location = new System.Drawing.Point(326, 69);
            this.button_tlayerdlg.Name = "button_tlayerdlg";
            this.button_tlayerdlg.Size = new System.Drawing.Size(68, 20);
            this.button_tlayerdlg.TabIndex = 5;
            this.button_tlayerdlg.Text = "浏览(…)";
            this.button_tlayerdlg.UseVisualStyleBackColor = true;
            this.button_tlayerdlg.Click += new System.EventHandler(this.button_target_layer_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "CellSize(X):";
            // 
            // tb_cellsize_x
            // 
            this.tb_cellsize_x.Location = new System.Drawing.Point(95, 20);
            this.tb_cellsize_x.Name = "tb_cellsize_x";
            this.tb_cellsize_x.Size = new System.Drawing.Size(85, 21);
            this.tb_cellsize_x.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "CellSize(Y):";
            // 
            // tb_cellsize_y
            // 
            this.tb_cellsize_y.Location = new System.Drawing.Point(95, 58);
            this.tb_cellsize_y.Name = "tb_cellsize_y";
            this.tb_cellsize_y.Size = new System.Drawing.Size(85, 21);
            this.tb_cellsize_y.TabIndex = 9;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_columns);
            this.groupBox1.Controls.Add(this.tb_rows);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.tb_cellsize_y);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tb_cellsize_x);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 192);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(396, 100);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "栅格属性";
            // 
            // tb_columns
            // 
            this.tb_columns.Location = new System.Drawing.Point(298, 58);
            this.tb_columns.Name = "tb_columns";
            this.tb_columns.Size = new System.Drawing.Size(85, 21);
            this.tb_columns.TabIndex = 13;
            // 
            // tb_rows
            // 
            this.tb_rows.Location = new System.Drawing.Point(298, 22);
            this.tb_rows.Name = "tb_rows";
            this.tb_rows.Size = new System.Drawing.Size(85, 21);
            this.tb_rows.TabIndex = 12;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(226, 61);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 11;
            this.label9.Text = "Columns:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(226, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "Rows:";
            // 
            // button_OK
            // 
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(310, 307);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 22);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定(&O)";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_CANCEL
            // 
            this.button_CANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_CANCEL.Location = new System.Drawing.Point(190, 307);
            this.button_CANCEL.Name = "button_CANCEL";
            this.button_CANCEL.Size = new System.Drawing.Size(85, 21);
            this.button_CANCEL.TabIndex = 13;
            this.button_CANCEL.Text = "取消(&C)";
            this.button_CANCEL.UseVisualStyleBackColor = true;
            // 
            // button_eptdlg
            // 
            this.button_eptdlg.Location = new System.Drawing.Point(327, 150);
            this.button_eptdlg.Name = "button_eptdlg";
            this.button_eptdlg.Size = new System.Drawing.Size(68, 20);
            this.button_eptdlg.TabIndex = 28;
            this.button_eptdlg.Text = "浏览(…)";
            this.button_eptdlg.UseVisualStyleBackColor = true;
            this.button_eptdlg.Click += new System.EventHandler(this.button_eptdlg_Click);
            // 
            // tb_end_point
            // 
            this.tb_end_point.Location = new System.Drawing.Point(104, 150);
            this.tb_end_point.Name = "tb_end_point";
            this.tb_end_point.Size = new System.Drawing.Size(200, 21);
            this.tb_end_point.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 153);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 26;
            this.label5.Text = "终止点：";
            // 
            // button_sptdlg
            // 
            this.button_sptdlg.Location = new System.Drawing.Point(326, 112);
            this.button_sptdlg.Name = "button_sptdlg";
            this.button_sptdlg.Size = new System.Drawing.Size(70, 21);
            this.button_sptdlg.TabIndex = 25;
            this.button_sptdlg.Text = "浏览(…)";
            this.button_sptdlg.UseVisualStyleBackColor = true;
            this.button_sptdlg.Click += new System.EventHandler(this.button_sptdlg_Click);
            // 
            // tb_start_point
            // 
            this.tb_start_point.Location = new System.Drawing.Point(104, 113);
            this.tb_start_point.Name = "tb_start_point";
            this.tb_start_point.Size = new System.Drawing.Size(200, 21);
            this.tb_start_point.TabIndex = 24;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 23;
            this.label6.Text = "起始点：";
            // 
            // ParaDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 356);
            this.Controls.Add(this.button_eptdlg);
            this.Controls.Add(this.tb_end_point);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_sptdlg);
            this.Controls.Add(this.tb_start_point);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button_CANCEL);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_tlayerdlg);
            this.Controls.Add(this.tb_target_layer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_opendlg);
            this.Controls.Add(this.tb_default_dir);
            this.Controls.Add(this.label1);
            this.Name = "ParaDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "参数的初始化";
            this.Load += new System.EventHandler(this.ParaDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_default_dir;
        private System.Windows.Forms.Button button_opendlg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_target_layer;
        private System.Windows.Forms.Button button_tlayerdlg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_cellsize_x;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_cellsize_y;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_CANCEL;
        private System.Windows.Forms.TextBox tb_columns;
        private System.Windows.Forms.TextBox tb_rows;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_eptdlg;
        private System.Windows.Forms.TextBox tb_end_point;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_sptdlg;
        private System.Windows.Forms.TextBox tb_start_point;
        private System.Windows.Forms.Label label6;
    }
}