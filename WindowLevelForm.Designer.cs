namespace DRSystem
{
    partial class WindowLevelForm
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
            this.btn_ValueSet = new System.Windows.Forms.Button();
            this.tbx_UpperValue = new System.Windows.Forms.TextBox();
            this.tbx_LowerValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBarUpperLimit = new System.Windows.Forms.TrackBar();
            this.trackBarLowerLimit = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarUpperLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLowerLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_ValueSet
            // 
            this.btn_ValueSet.Location = new System.Drawing.Point(547, 272);
            this.btn_ValueSet.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_ValueSet.Name = "btn_ValueSet";
            this.btn_ValueSet.Size = new System.Drawing.Size(90, 34);
            this.btn_ValueSet.TabIndex = 15;
            this.btn_ValueSet.Text = "参数设置";
            this.btn_ValueSet.UseVisualStyleBackColor = true;
            this.btn_ValueSet.Click += new System.EventHandler(this.btn_ValueSet_Click);
            // 
            // tbx_UpperValue
            // 
            this.tbx_UpperValue.Location = new System.Drawing.Point(547, 195);
            this.tbx_UpperValue.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbx_UpperValue.Name = "tbx_UpperValue";
            this.tbx_UpperValue.Size = new System.Drawing.Size(90, 28);
            this.tbx_UpperValue.TabIndex = 14;
            this.tbx_UpperValue.Text = "65535";
            // 
            // tbx_LowerValue
            // 
            this.tbx_LowerValue.Location = new System.Drawing.Point(547, 57);
            this.tbx_LowerValue.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbx_LowerValue.Name = "tbx_LowerValue";
            this.tbx_LowerValue.Size = new System.Drawing.Size(90, 28);
            this.tbx_LowerValue.TabIndex = 13;
            this.tbx_LowerValue.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(80, 203);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 22);
            this.label2.TabIndex = 12;
            this.label2.Text = "上限";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(80, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 22);
            this.label1.TabIndex = 11;
            this.label1.Text = "下限";
            // 
            // trackBarUpperLimit
            // 
            this.trackBarUpperLimit.Location = new System.Drawing.Point(133, 183);
            this.trackBarUpperLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.trackBarUpperLimit.Maximum = 65535;
            this.trackBarUpperLimit.Name = "trackBarUpperLimit";
            this.trackBarUpperLimit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.trackBarUpperLimit.Size = new System.Drawing.Size(406, 69);
            this.trackBarUpperLimit.TabIndex = 10;
            this.trackBarUpperLimit.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarUpperLimit.UseWaitCursor = true;
            this.trackBarUpperLimit.Value = 65535;
            this.trackBarUpperLimit.Scroll += new System.EventHandler(this.trackBarUpperLimit_Scroll);
            this.trackBarUpperLimit.ValueChanged += new System.EventHandler(this.trackBarUpperLimit_ValueChanged);
            // 
            // trackBarLowerLimit
            // 
            this.trackBarLowerLimit.Location = new System.Drawing.Point(133, 44);
            this.trackBarLowerLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.trackBarLowerLimit.Maximum = 65535;
            this.trackBarLowerLimit.Name = "trackBarLowerLimit";
            this.trackBarLowerLimit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.trackBarLowerLimit.Size = new System.Drawing.Size(406, 69);
            this.trackBarLowerLimit.TabIndex = 9;
            this.trackBarLowerLimit.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBarLowerLimit.UseWaitCursor = true;
            this.trackBarLowerLimit.Scroll += new System.EventHandler(this.trackBarLowerLimit_Scroll);
            this.trackBarLowerLimit.ValueChanged += new System.EventHandler(this.trackBarLowerLimit_ValueChanged);
            // 
            // WindowLevelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 350);
            this.Controls.Add(this.btn_ValueSet);
            this.Controls.Add(this.tbx_UpperValue);
            this.Controls.Add(this.tbx_LowerValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarUpperLimit);
            this.Controls.Add(this.trackBarLowerLimit);
            this.Name = "WindowLevelForm";
            this.Text = "WindowLevelForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowLevelForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarUpperLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLowerLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_ValueSet;
        private System.Windows.Forms.TextBox tbx_UpperValue;
        private System.Windows.Forms.TextBox tbx_LowerValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBarUpperLimit;
        private System.Windows.Forms.TrackBar trackBarLowerLimit;
    }
}