using System.Drawing;

namespace L10NSharp.Windows.Forms.UI
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
			this._messageLabel = new System.Windows.Forms.Label();
			this._OKButton = new System.Windows.Forms.Button();
			this.uiLanguageComboBox1 = new L10NSharp.Windows.Forms.UI.UILanguageComboBox();
			this.SuspendLayout();
			//
			// _messageLabel
			//
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.AutoEllipsis = true;
			this._messageLabel.Location = new System.Drawing.Point(23, 5);
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.Size = new System.Drawing.Size(220, 84);
			this._messageLabel.TabIndex = 1;
			this._messageLabel.Text = "Our apologies, this program has not yet been localized for {0} ({1}). Please choo" +
    "se from one of the following languages:";
			this._messageLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// _OKButton
			//
			this._OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._OKButton.Location = new System.Drawing.Point(168, 150);
			this._OKButton.Name = "_OKButton";
			this._OKButton.Size = new System.Drawing.Size(75, 23);
			this._OKButton.TabIndex = 2;
			this._OKButton.Text = "&OK";
			this._OKButton.UseVisualStyleBackColor = true;
			this._OKButton.Click += new System.EventHandler(this._OKButton_Click);
			//
			// uiLanguageComboBox1
			//
			this.uiLanguageComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
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

        }

        #endregion

        private UILanguageComboBox uiLanguageComboBox1;
        private System.Windows.Forms.Label _messageLabel;
        private System.Windows.Forms.Button _OKButton;
    }
}
