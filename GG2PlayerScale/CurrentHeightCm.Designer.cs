namespace GG2PlayerScale
{
    partial class FrmCurrentHeight
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
            this.lblPlayerHeight = new System.Windows.Forms.Label();
            this.lblPlayerScale = new System.Windows.Forms.Label();
            this.lblMultiplicationTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPlayerHeight
            // 
            this.lblPlayerHeight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlayerHeight.BackColor = System.Drawing.Color.Transparent;
            this.lblPlayerHeight.Font = new System.Drawing.Font("Lucida Console", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayerHeight.ForeColor = System.Drawing.Color.Lime;
            this.lblPlayerHeight.Location = new System.Drawing.Point(12, 9);
            this.lblPlayerHeight.Name = "lblPlayerHeight";
            this.lblPlayerHeight.Size = new System.Drawing.Size(260, 76);
            this.lblPlayerHeight.TabIndex = 0;
            this.lblPlayerHeight.Text = "160cm";
            this.lblPlayerHeight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPlayerHeight.UseCompatibleTextRendering = true;
            // 
            // lblPlayerScale
            // 
            this.lblPlayerScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlayerScale.BackColor = System.Drawing.Color.Transparent;
            this.lblPlayerScale.Font = new System.Drawing.Font("Lucida Console", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayerScale.ForeColor = System.Drawing.Color.Gold;
            this.lblPlayerScale.Location = new System.Drawing.Point(12, 70);
            this.lblPlayerScale.Name = "lblPlayerScale";
            this.lblPlayerScale.Size = new System.Drawing.Size(260, 76);
            this.lblPlayerScale.TabIndex = 1;
            this.lblPlayerScale.Text = "100%";
            this.lblPlayerScale.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPlayerScale.UseCompatibleTextRendering = true;
            // 
            // lblMultiplicationTime
            // 
            this.lblMultiplicationTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMultiplicationTime.BackColor = System.Drawing.Color.Transparent;
            this.lblMultiplicationTime.Font = new System.Drawing.Font("Lucida Console", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMultiplicationTime.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblMultiplicationTime.Location = new System.Drawing.Point(12, 131);
            this.lblMultiplicationTime.Name = "lblMultiplicationTime";
            this.lblMultiplicationTime.Size = new System.Drawing.Size(260, 76);
            this.lblMultiplicationTime.TabIndex = 2;
            this.lblMultiplicationTime.Text = "0:00:00";
            this.lblMultiplicationTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMultiplicationTime.UseCompatibleTextRendering = true;
            // 
            // FrmCurrentHeight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(284, 200);
            this.Controls.Add(this.lblMultiplicationTime);
            this.Controls.Add(this.lblPlayerScale);
            this.Controls.Add(this.lblPlayerHeight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCurrentHeight";
            this.Text = "Current player stats";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CurrentHeightCm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblPlayerHeight;
        public System.Windows.Forms.Label lblPlayerScale;
        public System.Windows.Forms.Label lblMultiplicationTime;
    }
}