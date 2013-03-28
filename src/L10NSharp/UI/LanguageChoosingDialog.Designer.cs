namespace L10NSharp.UI
{
    partial class LanguageChoosingDialog
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
            this.uiLanguageComboBox1 = new UILanguageComboBox();
            this._messageLabel = new System.Windows.Forms.Label();
            this._OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // uiLanguageComboBox1
            // 
            this.uiLanguageComboBox1.DisplayMember = "NativeName";
            this.uiLanguageComboBox1.DropDownHeight = 200;
            this.uiLanguageComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.uiLanguageComboBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.uiLanguageComboBox1.FormattingEnabled = true;
            this.uiLanguageComboBox1.IntegralHeight = false;
            this.uiLanguageComboBox1.Location = new System.Drawing.Point(26, 92);
            this.uiLanguageComboBox1.Name = "uiLanguageComboBox1";
            this.uiLanguageComboBox1.ShowOnlyLanguagesHavingLocalizations = true;
            this.uiLanguageComboBox1.Size = new System.Drawing.Size(217, 23);
            this.uiLanguageComboBox1.TabIndex = 0;
            // 
            // _messageLabel
            // 
            this._messageLabel.AutoSize = true;
            this._messageLabel.Location = new System.Drawing.Point(23, 25);
            this._messageLabel.MaximumSize = new System.Drawing.Size(220, 0);
            this._messageLabel.Name = "_messageLabel";
            this._messageLabel.Size = new System.Drawing.Size(220, 39);
            this._messageLabel.TabIndex = 1;
            this._messageLabel.Text = "Our apologies, this program has not yet been localized for {0}. Please choose fro" +
    "m one of the following languages:";
            // 
            // _OKButton
            // 
            this._OKButton.Location = new System.Drawing.Point(168, 150);
            this._OKButton.Name = "_OKButton";
            this._OKButton.Size = new System.Drawing.Size(75, 23);
            this._OKButton.TabIndex = 2;
            this._OKButton.Text = "&OK";
            this._OKButton.UseVisualStyleBackColor = true;
            this._OKButton.Click += new System.EventHandler(this._OKButton_Click);
            // 
            // LanguageChoosingDialog
            // 
            this.AcceptButton = this._OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 195);
            this.Controls.Add(this._OKButton);
            this.Controls.Add(this._messageLabel);
            this.Controls.Add(this.uiLanguageComboBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LanguageChoosingDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Choose a Language";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UILanguageComboBox uiLanguageComboBox1;
        private System.Windows.Forms.Label _messageLabel;
        private System.Windows.Forms.Button _OKButton;
    }
}