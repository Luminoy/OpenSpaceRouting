namespace OpenSpaceRouting
{
    partial class SlopeCalculation
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.textBox_in_layer = new System.Windows.Forms.TextBox();
            this.btn_path = new System.Windows.Forms.Button();
            this.textBox_out_layer = new System.Windows.Forms.TextBox();
            this.btn_ok = new System.Windows.Forms.Button();
            this.groupBox_input = new System.Windows.Forms.GroupBox();
            this.groupBox_output = new System.Windows.Forms.GroupBox();
            this.btn_path_2 = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.groupBox_input.SuspendLayout();
            this.groupBox_output.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(47, 59);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(166, 20);
            this.comboBox1.TabIndex = 0;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(27, 23);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(71, 16);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "从图层：";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(27, 95);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(95, 16);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "从文件路径：";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // textBox_in_layer
            // 
            this.textBox_in_layer.Location = new System.Drawing.Point(23, 34);
            this.textBox_in_layer.Name = "textBox_in_layer";
            this.textBox_in_layer.Size = new System.Drawing.Size(166, 21);
            this.textBox_in_layer.TabIndex = 3;
            this.textBox_in_layer.Leave += new System.EventHandler(this.textBox_in_layer_Leave);
            // 
            // btn_path
            // 
            this.btn_path.Location = new System.Drawing.Point(219, 33);
            this.btn_path.Name = "btn_path";
            this.btn_path.Size = new System.Drawing.Size(78, 21);
            this.btn_path.TabIndex = 4;
            this.btn_path.Text = "浏览(&…)";
            this.btn_path.UseVisualStyleBackColor = true;
            this.btn_path.Click += new System.EventHandler(this.btn_path_Click);
            // 
            // textBox_out_layer
            // 
            this.textBox_out_layer.Location = new System.Drawing.Point(22, 32);
            this.textBox_out_layer.Name = "textBox_out_layer";
            this.textBox_out_layer.Size = new System.Drawing.Size(167, 21);
            this.textBox_out_layer.TabIndex = 5;
            // 
            // btn_ok
            // 
            this.btn_ok.Location = new System.Drawing.Point(243, 341);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(78, 21);
            this.btn_ok.TabIndex = 7;
            this.btn_ok.Text = "确定";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // groupBox_input
            // 
            this.groupBox_input.Controls.Add(this.textBox_in_layer);
            this.groupBox_input.Controls.Add(this.btn_path);
            this.groupBox_input.Location = new System.Drawing.Point(24, 117);
            this.groupBox_input.Name = "groupBox_input";
            this.groupBox_input.Size = new System.Drawing.Size(330, 77);
            this.groupBox_input.TabIndex = 8;
            this.groupBox_input.TabStop = false;
            // 
            // groupBox_output
            // 
            this.groupBox_output.Controls.Add(this.btn_path_2);
            this.groupBox_output.Controls.Add(this.textBox_out_layer);
            this.groupBox_output.Location = new System.Drawing.Point(24, 231);
            this.groupBox_output.Name = "groupBox_output";
            this.groupBox_output.Size = new System.Drawing.Size(330, 80);
            this.groupBox_output.TabIndex = 9;
            this.groupBox_output.TabStop = false;
            this.groupBox_output.Text = "输出路径：";
            // 
            // btn_path_2
            // 
            this.btn_path_2.Location = new System.Drawing.Point(219, 32);
            this.btn_path_2.Name = "btn_path_2";
            this.btn_path_2.Size = new System.Drawing.Size(78, 21);
            this.btn_path_2.TabIndex = 6;
            this.btn_path_2.Text = "浏览(&…)";
            this.btn_path_2.UseVisualStyleBackColor = true;
            this.btn_path_2.Click += new System.EventHandler(this.btn_path_2_Click);
            // 
            // btn_refresh
            // 
            this.btn_refresh.Location = new System.Drawing.Point(243, 58);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(78, 20);
            this.btn_refresh.TabIndex = 10;
            this.btn_refresh.Text = "刷新(&R)";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // SlopeCalculation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 388);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_refresh);
            this.Controls.Add(this.groupBox_output);
            this.Controls.Add(this.groupBox_input);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.comboBox1);
            this.Name = "SlopeCalculation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SlopeCalculation";
            this.Load += new System.EventHandler(this.SlopeCalculation_Load);
            this.groupBox_input.ResumeLayout(false);
            this.groupBox_input.PerformLayout();
            this.groupBox_output.ResumeLayout(false);
            this.groupBox_output.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.TextBox textBox_in_layer;
        private System.Windows.Forms.Button btn_path;
        private System.Windows.Forms.TextBox textBox_out_layer;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.GroupBox groupBox_input;
        private System.Windows.Forms.GroupBox groupBox_output;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Button btn_path_2;
    }
}