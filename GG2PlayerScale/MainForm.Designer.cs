﻿namespace GG2PlayerScale
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblButtonPressed = new System.Windows.Forms.Label();
            this.lblConnect = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtPlayerScale = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtEndScale = new System.Windows.Forms.TextBox();
            this.chkEnableEndScale = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCountdown = new System.Windows.Forms.TextBox();
            this.cbTargetTimeUnit = new System.Windows.Forms.ComboBox();
            this.btnProcessPause = new System.Windows.Forms.Button();
            this.btnProcessStart = new System.Windows.Forms.Button();
            this.txtTargetTimeValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTargetScale = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gbScaleReset = new System.Windows.Forms.GroupBox();
            this.chkResetScaleGradually = new System.Windows.Forms.CheckBox();
            this.chkResetWorldScale = new System.Windows.Forms.CheckBox();
            this._lblEyeHeight = new System.Windows.Forms.Label();
            this._lblPlayerHeight = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbScaleReset.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblButtonPressed);
            this.groupBox1.Controls.Add(this.lblConnect);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(453, 74);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection Status";
            // 
            // lblButtonPressed
            // 
            this.lblButtonPressed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblButtonPressed.Location = new System.Drawing.Point(415, 57);
            this.lblButtonPressed.Name = "lblButtonPressed";
            this.lblButtonPressed.Size = new System.Drawing.Size(32, 14);
            this.lblButtonPressed.TabIndex = 1;
            this.lblButtonPressed.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lblConnect
            // 
            this.lblConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnect.Location = new System.Drawing.Point(6, 16);
            this.lblConnect.Name = "lblConnect";
            this.lblConnect.Size = new System.Drawing.Size(441, 55);
            this.lblConnect.TabIndex = 0;
            this.lblConnect.Text = "Connecting ...";
            this.lblConnect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtPlayerScale);
            this.groupBox2.Location = new System.Drawing.Point(12, 169);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(453, 53);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Player scale";
            // 
            // txtPlayerScale
            // 
            this.txtPlayerScale.Location = new System.Drawing.Point(9, 19);
            this.txtPlayerScale.Name = "txtPlayerScale";
            this.txtPlayerScale.Size = new System.Drawing.Size(438, 20);
            this.txtPlayerScale.TabIndex = 0;
            this.txtPlayerScale.Text = "1.0";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this._lblPlayerHeight);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this._lblEyeHeight);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(12, 95);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(453, 68);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Static parameters";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Player eye height:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtEndScale);
            this.groupBox3.Controls.Add(this.chkEnableEndScale);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.txtCountdown);
            this.groupBox3.Controls.Add(this.cbTargetTimeUnit);
            this.groupBox3.Controls.Add(this.btnProcessPause);
            this.groupBox3.Controls.Add(this.btnProcessStart);
            this.groupBox3.Controls.Add(this.txtTargetTimeValue);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtTargetScale);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(12, 306);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(453, 207);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Gradual scale change over time";
            // 
            // txtEndScale
            // 
            this.txtEndScale.Enabled = false;
            this.txtEndScale.Location = new System.Drawing.Point(155, 123);
            this.txtEndScale.Name = "txtEndScale";
            this.txtEndScale.Size = new System.Drawing.Size(292, 20);
            this.txtEndScale.TabIndex = 6;
            this.txtEndScale.Text = "0.5";
            // 
            // chkEnableEndScale
            // 
            this.chkEnableEndScale.Location = new System.Drawing.Point(9, 122);
            this.chkEnableEndScale.Name = "chkEnableEndScale";
            this.chkEnableEndScale.Size = new System.Drawing.Size(140, 21);
            this.chkEnableEndScale.TabIndex = 5;
            this.chkEnableEndScale.Text = "until reaching the scale:";
            this.chkEnableEndScale.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkEnableEndScale.UseVisualStyleBackColor = true;
            this.chkEnableEndScale.CheckedChanged += new System.EventHandler(this.chkEnableEndScale_CheckedChanged);
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label6.Location = new System.Drawing.Point(9, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(438, 20);
            this.label6.TabIndex = 14;
            this.label6.Text = "(0 second countdown means it will immediately start)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "after counting down";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(356, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 20);
            this.label5.TabIndex = 12;
            this.label5.Text = "seconds.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCountdown
            // 
            this.txtCountdown.Location = new System.Drawing.Point(155, 78);
            this.txtCountdown.Name = "txtCountdown";
            this.txtCountdown.Size = new System.Drawing.Size(195, 20);
            this.txtCountdown.TabIndex = 4;
            this.txtCountdown.Text = "0";
            // 
            // cbTargetTimeUnit
            // 
            this.cbTargetTimeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTargetTimeUnit.FormattingEnabled = true;
            this.cbTargetTimeUnit.Items.AddRange(new object[] {
            "minutes",
            "seconds"});
            this.cbTargetTimeUnit.Location = new System.Drawing.Point(356, 52);
            this.cbTargetTimeUnit.Name = "cbTargetTimeUnit";
            this.cbTargetTimeUnit.Size = new System.Drawing.Size(91, 21);
            this.cbTargetTimeUnit.TabIndex = 3;
            // 
            // btnProcessPause
            // 
            this.btnProcessPause.Enabled = false;
            this.btnProcessPause.Location = new System.Drawing.Point(9, 178);
            this.btnProcessPause.Name = "btnProcessPause";
            this.btnProcessPause.Size = new System.Drawing.Size(438, 23);
            this.btnProcessPause.TabIndex = 8;
            this.btnProcessPause.Text = "Pause";
            this.btnProcessPause.UseVisualStyleBackColor = true;
            this.btnProcessPause.Click += new System.EventHandler(this.btnProcessPause_Click);
            // 
            // btnProcessStart
            // 
            this.btnProcessStart.Location = new System.Drawing.Point(9, 149);
            this.btnProcessStart.Name = "btnProcessStart";
            this.btnProcessStart.Size = new System.Drawing.Size(438, 23);
            this.btnProcessStart.TabIndex = 7;
            this.btnProcessStart.Text = "Start process";
            this.btnProcessStart.UseVisualStyleBackColor = true;
            this.btnProcessStart.Click += new System.EventHandler(this.btnProcessStart_Click);
            // 
            // txtTargetTimeValue
            // 
            this.txtTargetTimeValue.Location = new System.Drawing.Point(155, 52);
            this.txtTargetTimeValue.Name = "txtTargetTimeValue";
            this.txtTargetTimeValue.Size = new System.Drawing.Size(195, 20);
            this.txtTargetTimeValue.TabIndex = 2;
            this.txtTargetTimeValue.Text = "30";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(9, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "over the timespan of";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTargetScale
            // 
            this.txtTargetScale.Location = new System.Drawing.Point(155, 26);
            this.txtTargetScale.Name = "txtTargetScale";
            this.txtTargetScale.Size = new System.Drawing.Size(292, 20);
            this.txtTargetScale.TabIndex = 1;
            this.txtTargetScale.Text = "0.5";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(9, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(140, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Scale by factor";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gbScaleReset
            // 
            this.gbScaleReset.Controls.Add(this.chkResetScaleGradually);
            this.gbScaleReset.Controls.Add(this.chkResetWorldScale);
            this.gbScaleReset.Location = new System.Drawing.Point(12, 228);
            this.gbScaleReset.Name = "gbScaleReset";
            this.gbScaleReset.Size = new System.Drawing.Size(453, 72);
            this.gbScaleReset.TabIndex = 3;
            this.gbScaleReset.TabStop = false;
            this.gbScaleReset.Text = "Scale reset settings";
            // 
            // chkResetScaleGradually
            // 
            this.chkResetScaleGradually.Checked = true;
            this.chkResetScaleGradually.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResetScaleGradually.Location = new System.Drawing.Point(12, 45);
            this.chkResetScaleGradually.Name = "chkResetScaleGradually";
            this.chkResetScaleGradually.Size = new System.Drawing.Size(435, 21);
            this.chkResetScaleGradually.TabIndex = 7;
            this.chkResetScaleGradually.Text = "Adjust scale gradually (within 0.2 seconds)";
            this.chkResetScaleGradually.UseVisualStyleBackColor = true;
            // 
            // chkResetWorldScale
            // 
            this.chkResetWorldScale.Checked = true;
            this.chkResetWorldScale.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResetWorldScale.Location = new System.Drawing.Point(12, 19);
            this.chkResetWorldScale.Name = "chkResetWorldScale";
            this.chkResetWorldScale.Size = new System.Drawing.Size(435, 21);
            this.chkResetWorldScale.TabIndex = 6;
            this.chkResetWorldScale.Text = "Reset world scale while scale reset is active";
            this.chkResetWorldScale.UseVisualStyleBackColor = true;
            // 
            // _lblEyeHeight
            // 
            this._lblEyeHeight.Location = new System.Drawing.Point(152, 19);
            this._lblEyeHeight.Name = "_lblEyeHeight";
            this._lblEyeHeight.Size = new System.Drawing.Size(295, 20);
            this._lblEyeHeight.TabIndex = 2;
            this._lblEyeHeight.Text = "160 cm";
            this._lblEyeHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblPlayerHeight
            // 
            this._lblPlayerHeight.Location = new System.Drawing.Point(152, 42);
            this._lblPlayerHeight.Name = "_lblPlayerHeight";
            this._lblPlayerHeight.Size = new System.Drawing.Size(295, 20);
            this._lblPlayerHeight.TabIndex = 4;
            this._lblPlayerHeight.Text = "170 cm";
            this._lblPlayerHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(9, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(140, 20);
            this.label8.TabIndex = 3;
            this.label8.Text = "Estimated player height:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 521);
            this.Controls.Add(this.gbScaleReset);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Resize player form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.gbScaleReset.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblConnect;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtPlayerScale;
        private System.Windows.Forms.Label lblButtonPressed;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtTargetTimeValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTargetScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnProcessPause;
        private System.Windows.Forms.Button btnProcessStart;
        private System.Windows.Forms.ComboBox cbTargetTimeUnit;
        private System.Windows.Forms.TextBox txtEndScale;
        private System.Windows.Forms.CheckBox chkEnableEndScale;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCountdown;
        private System.Windows.Forms.GroupBox gbScaleReset;
        private System.Windows.Forms.CheckBox chkResetWorldScale;
        private System.Windows.Forms.CheckBox chkResetScaleGradually;
        private System.Windows.Forms.Label _lblEyeHeight;
        private System.Windows.Forms.Label _lblPlayerHeight;
        private System.Windows.Forms.Label label8;
    }
}

