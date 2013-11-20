namespace L10NSharp.UI
{
    partial class HowToDistributeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HowToDistributeDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._showTMXFile = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this._emailLabel = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 263);
            this.label1.MaximumSize = new System.Drawing.Size(300, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 95);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._okButton.Location = new System.Drawing.Point(243, 383);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "&OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(34, 161);
            this.label2.MaximumSize = new System.Drawing.Size(300, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 38);
            this.label2.TabIndex = 2;
            this.label2.Text = "To get your translation work into the next version of this program, email";
            // 
            // _showTMXFile
            // 
            this._showTMXFile.AutoSize = true;
            this._showTMXFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._showTMXFile.Location = new System.Drawing.Point(34, 199);
            this._showTMXFile.Name = "_showTMXFile";
            this._showTMXFile.Size = new System.Drawing.Size(90, 19);
            this._showTMXFile.TabIndex = 3;
            this._showTMXFile.TabStop = true;
            this._showTMXFile.Text = "your TMX file";
            this._showTMXFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnShowTMXFile);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(130, 199);
            this.label3.MaximumSize = new System.Drawing.Size(300, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 19);
            this.label3.TabIndex = 4;
            this.label3.Text = "to";
            // 
            // _emailLabel
            // 
            this._emailLabel.AutoSize = true;
            this._emailLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._emailLabel.Location = new System.Drawing.Point(34, 218);
            this._emailLabel.Name = "_emailLabel";
            this._emailLabel.Size = new System.Drawing.Size(177, 19);
            this._emailLabel.TabIndex = 5;
            this._emailLabel.TabStop = true;
            this._emailLabel.Text = "someone@somewhere.com";
            this._emailLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._emailLabel_LinkClicked);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(34, 35);
            this.label4.MaximumSize = new System.Drawing.Size(300, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(299, 95);
            this.label4.TabIndex = 6;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // HowToDistributeDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._okButton;
            this.ClientSize = new System.Drawing.Size(347, 418);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._emailLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._showTMXFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HowToDistributeDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "How To Distribute";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel _showTMXFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel _emailLabel;
        private System.Windows.Forms.Label label4;
    }
}
