namespace L10NSharpWinforms.UI
{
	partial class LanguageChoosingSimpleDialog
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
			this._uiLanguageListBox = new L10NSharpWinforms.UI.UILanguageListBox();
			this._btnOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _uiLanguageListBox
			//
			this._uiLanguageListBox.DisplayMember = "NativeName";
			this._uiLanguageListBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._uiLanguageListBox.FormattingEnabled = true;
			this._uiLanguageListBox.ItemHeight = 15;
			this._uiLanguageListBox.Location = new System.Drawing.Point(12, 12);
			this._uiLanguageListBox.Name = "_uiLanguageListBox";
			this._uiLanguageListBox.ShowOnlyLanguagesHavingLocalizations = true;
			this._uiLanguageListBox.Size = new System.Drawing.Size(222, 229);
			this._uiLanguageListBox.TabIndex = 3;
			this._uiLanguageListBox.DoubleClick += new System.EventHandler(this.m_uiLanguageListBox_DoubleClick);
			//
			// _btnOk
			//
			this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._btnOk.Location = new System.Drawing.Point(159, 248);
			this._btnOk.Name = "_btnOk";
			this._btnOk.Size = new System.Drawing.Size(75, 23);
			this._btnOk.TabIndex = 4;
			this._btnOk.Text = "OK";
			this._btnOk.UseVisualStyleBackColor = true;
			this._btnOk.Click += new System.EventHandler(this.btnOk_Click);
			//
			// LanguageChoosingSimpleDialog
			//
			this.AcceptButton = this._btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(246, 283);
			this.Controls.Add(this._btnOk);
			this.Controls.Add(this._uiLanguageListBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LanguageChoosingSimpleDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Language";
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharpWinforms.UI.UILanguageListBox _uiLanguageListBox;
		private System.Windows.Forms.Button _btnOk;
	}
}
