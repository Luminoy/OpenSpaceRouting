namespace OpenSpaceRouting
{
    partial class ColorCustomize
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_to_red = new System.Windows.Forms.MaskedTextBox();
            this.tb_to_green = new System.Windows.Forms.MaskedTextBox();
            this.tb_to_blue = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_from_blue = new System.Windows.Forms.MaskedTextBox();
            this.tb_from_green = new System.Windows.Forms.MaskedTextBox();
            this.tb_from_red = new System.Windows.Forms.MaskedTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_reset = new System.Windows.Forms.Button();
            this.error_provider = new System.Windows.Forms.ErrorProvider(this.components);
            this.radio_discrete = new System.Windows.Forms.RadioButton();
            this.radio_continous = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.error_provider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "R:";
            // 
            // tb_to_red
            // 
            this.tb_to_red.Location = new System.Drawing.Point(48, 16);
            this.tb_to_red.Name = "tb_to_red";
            this.tb_to_red.Size = new System.Drawing.Size(58, 21);
            this.tb_to_red.TabIndex = 4;
            this.tb_to_red.Leave += new System.EventHandler(this.tb_to_red_Leave);
            // 
            // tb_to_green
            // 
            this.tb_to_green.Location = new System.Drawing.Point(48, 62);
            this.tb_to_green.Name = "tb_to_green";
            this.tb_to_green.Size = new System.Drawing.Size(58, 21);
            this.tb_to_green.TabIndex = 5;
            this.tb_to_green.Leave += new System.EventHandler(this.tb_to_green_Leave);
            // 
            // tb_to_blue
            // 
            this.tb_to_blue.Location = new System.Drawing.Point(48, 108);
            this.tb_to_blue.Name = "tb_to_blue";
            this.tb_to_blue.Size = new System.Drawing.Size(58, 21);
            this.tb_to_blue.TabIndex = 6;
            this.tb_to_blue.Leave += new System.EventHandler(this.tb_to_blue_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "G:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "B:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tb_to_blue);
            this.groupBox1.Controls.Add(this.tb_to_green);
            this.groupBox1.Controls.Add(this.tb_to_red);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(176, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(136, 143);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "To";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tb_from_blue);
            this.groupBox2.Controls.Add(this.tb_from_green);
            this.groupBox2.Controls.Add(this.tb_from_red);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(12, 39);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(136, 143);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "From";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "B:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "G:";
            // 
            // tb_from_blue
            // 
            this.tb_from_blue.Location = new System.Drawing.Point(48, 108);
            this.tb_from_blue.Name = "tb_from_blue";
            this.tb_from_blue.Size = new System.Drawing.Size(58, 21);
            this.tb_from_blue.TabIndex = 3;
            this.tb_from_blue.Leave += new System.EventHandler(this.tb_from_blue_Leave);
            // 
            // tb_from_green
            // 
            this.tb_from_green.Location = new System.Drawing.Point(48, 62);
            this.tb_from_green.Name = "tb_from_green";
            this.tb_from_green.Size = new System.Drawing.Size(58, 21);
            this.tb_from_green.TabIndex = 2;
            this.tb_from_green.Leave += new System.EventHandler(this.tb_from_green_Leave);
            // 
            // tb_from_red
            // 
            this.tb_from_red.Location = new System.Drawing.Point(48, 16);
            this.tb_from_red.Name = "tb_from_red";
            this.tb_from_red.PromptChar = ' ';
            this.tb_from_red.RejectInputOnFirstFailure = true;
            this.tb_from_red.Size = new System.Drawing.Size(58, 21);
            this.tb_from_red.TabIndex = 1;
            this.tb_from_red.Leave += new System.EventHandler(this.tb_from_red_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(25, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "R:";
            // 
            // btn_ok
            // 
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Location = new System.Drawing.Point(225, 193);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(57, 24);
            this.btn_ok.TabIndex = 8;
            this.btn_ok.Text = "确定";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(134, 193);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(57, 24);
            this.btn_cancel.TabIndex = 9;
            this.btn_cancel.Text = "取消";
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // btn_reset
            // 
            this.btn_reset.Location = new System.Drawing.Point(39, 193);
            this.btn_reset.Name = "btn_reset";
            this.btn_reset.Size = new System.Drawing.Size(57, 24);
            this.btn_reset.TabIndex = 10;
            this.btn_reset.Text = "重置";
            this.btn_reset.UseVisualStyleBackColor = true;
            this.btn_reset.Click += new System.EventHandler(this.btn_reset_Click);
            // 
            // error_provider
            // 
            this.error_provider.ContainerControl = this;
            // 
            // radio_discrete
            // 
            this.radio_discrete.AutoSize = true;
            this.radio_discrete.Location = new System.Drawing.Point(39, 15);
            this.radio_discrete.Name = "radio_discrete";
            this.radio_discrete.Size = new System.Drawing.Size(71, 16);
            this.radio_discrete.TabIndex = 11;
            this.radio_discrete.TabStop = true;
            this.radio_discrete.Text = "离散色带";
            this.radio_discrete.UseVisualStyleBackColor = true;
            // 
            // radio_continous
            // 
            this.radio_continous.AutoSize = true;
            this.radio_continous.Location = new System.Drawing.Point(203, 15);
            this.radio_continous.Name = "radio_continous";
            this.radio_continous.Size = new System.Drawing.Size(71, 16);
            this.radio_continous.TabIndex = 12;
            this.radio_continous.TabStop = true;
            this.radio_continous.Text = "连续色带";
            this.radio_continous.UseVisualStyleBackColor = true;
            // 
            // ColorCustomize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 226);
            this.Controls.Add(this.radio_continous);
            this.Controls.Add(this.radio_discrete);
            this.Controls.Add(this.btn_reset);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ColorCustomize";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ColorCustomize";
            this.Load += new System.EventHandler(this.ColorCustomize_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.error_provider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox tb_to_red;
        private System.Windows.Forms.MaskedTextBox tb_to_green;
        private System.Windows.Forms.MaskedTextBox tb_to_blue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox tb_from_blue;
        private System.Windows.Forms.MaskedTextBox tb_from_green;
        private System.Windows.Forms.MaskedTextBox tb_from_red;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_reset;
        private System.Windows.Forms.ErrorProvider error_provider;
        public System.Windows.Forms.RadioButton radio_continous;
        public System.Windows.Forms.RadioButton radio_discrete;
    }
}