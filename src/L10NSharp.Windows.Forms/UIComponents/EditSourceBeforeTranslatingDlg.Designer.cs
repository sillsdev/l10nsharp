namespace L10NSharp.Windows.Forms.UIComponents
{
	partial class EditSourceBeforeTranslatingDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditSourceBeforeTranslatingDlg));
			this._lableTarget = new System.Windows.Forms.Label();
			this._lableSource = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._buttonCopyAndClose = new System.Windows.Forms.Button();
			this._textBoxTarget = new System.Windows.Forms.TextBox();
			this._textBoxSource = new System.Windows.Forms.TextBox();
			this._buttonCancel = new System.Windows.Forms.Button();
			this._buttonTranslate = new System.Windows.Forms.Button();
			this._labelDescription = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// _lableTarget
			// 
			this._lableTarget.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._lableTarget, 3);
			this._lableTarget.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lableTarget.Location = new System.Drawing.Point(0, 163);
			this._lableTarget.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this._lableTarget.Name = "_lableTarget";
			this._lableTarget.Size = new System.Drawing.Size(14, 15);
			this._lableTarget.TabIndex = 3;
			this._lableTarget.Text = "#";
			// 
			// _lableSource
			// 
			this._lableSource.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._lableSource, 3);
			this._lableSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lableSource.Location = new System.Drawing.Point(0, 55);
			this._lableSource.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this._lableSource.Name = "_lableSource";
			this._lableSource.Size = new System.Drawing.Size(14, 15);
			this._lableSource.TabIndex = 1;
			this._lableSource.Text = "#";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this._buttonCopyAndClose, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._textBoxTarget, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this._lableSource, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._lableTarget, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this._textBoxSource, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._buttonCancel, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this._buttonTranslate, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._labelDescription, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(565, 293);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// _buttonCopyAndClose
			// 
			this._buttonCopyAndClose.AutoSize = true;
			this._buttonCopyAndClose.Location = new System.Drawing.Point(281, 267);
			this._buttonCopyAndClose.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this._buttonCopyAndClose.MinimumSize = new System.Drawing.Size(75, 26);
			this._buttonCopyAndClose.Name = "_buttonCopyAndClose";
			this._buttonCopyAndClose.Size = new System.Drawing.Size(203, 26);
			this._buttonCopyAndClose.TabIndex = 6;
			this._buttonCopyAndClose.Text = "&Copy {0} Text to Clipboard and Close";
			this._buttonCopyAndClose.UseVisualStyleBackColor = true;
			// 
			// _textBoxTarget
			// 
			this._textBoxTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._textBoxTarget.BackColor = System.Drawing.SystemColors.Window;
			this.tableLayoutPanel1.SetColumnSpan(this._textBoxTarget, 3);
			this._textBoxTarget.Location = new System.Drawing.Point(0, 181);
			this._textBoxTarget.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this._textBoxTarget.Multiline = true;
			this._textBoxTarget.Name = "_textBoxTarget";
			this._textBoxTarget.ReadOnly = true;
			this._textBoxTarget.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._textBoxTarget.Size = new System.Drawing.Size(565, 80);
			this._textBoxTarget.TabIndex = 4;
			// 
			// _textBoxSource
			// 
			this._textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this._textBoxSource, 3);
			this._textBoxSource.Location = new System.Drawing.Point(0, 73);
			this._textBoxSource.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this._textBoxSource.Multiline = true;
			this._textBoxSource.Name = "_textBoxSource";
			this._textBoxSource.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._textBoxSource.Size = new System.Drawing.Size(565, 80);
			this._textBoxSource.TabIndex = 2;
			// 
			// _buttonCancel
			// 
			this._buttonCancel.AutoSize = true;
			this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._buttonCancel.Location = new System.Drawing.Point(490, 267);
			this._buttonCancel.Margin = new System.Windows.Forms.Padding(3, 6, 0, 0);
			this._buttonCancel.MinimumSize = new System.Drawing.Size(75, 26);
			this._buttonCancel.Name = "_buttonCancel";
			this._buttonCancel.Size = new System.Drawing.Size(75, 26);
			this._buttonCancel.TabIndex = 7;
			this._buttonCancel.Text = "Cancel";
			this._buttonCancel.UseVisualStyleBackColor = true;
			// 
			// _buttonTranslate
			// 
			this._buttonTranslate.AutoSize = true;
			this._buttonTranslate.Location = new System.Drawing.Point(0, 267);
			this._buttonTranslate.Margin = new System.Windows.Forms.Padding(0, 6, 3, 0);
			this._buttonTranslate.MinimumSize = new System.Drawing.Size(75, 26);
			this._buttonTranslate.Name = "_buttonTranslate";
			this._buttonTranslate.Size = new System.Drawing.Size(130, 26);
			this._buttonTranslate.TabIndex = 5;
			this._buttonTranslate.Text = "&Translate from {0} to {1}";
			this._buttonTranslate.UseVisualStyleBackColor = true;
			// 
			// _labelDescription
			// 
			this._labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._labelDescription.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._labelDescription, 3);
			this._labelDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelDescription.Location = new System.Drawing.Point(0, 0);
			this._labelDescription.Margin = new System.Windows.Forms.Padding(0);
			this._labelDescription.Name = "_labelDescription";
			this._labelDescription.Size = new System.Drawing.Size(565, 45);
			this._labelDescription.TabIndex = 0;
			this._labelDescription.Text = resources.GetString("_labelDescription.Text");
			// 
			// EditSourceBeforeTranslatingDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._buttonCancel;
			this.ClientSize = new System.Drawing.Size(589, 317);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditSourceBeforeTranslatingDlg";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Source Before Translating";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label _lableTarget;
		private System.Windows.Forms.Label _lableSource;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TextBox _textBoxTarget;
		private System.Windows.Forms.TextBox _textBoxSource;
		private System.Windows.Forms.Button _buttonCancel;
		private System.Windows.Forms.Button _buttonTranslate;
		private System.Windows.Forms.Label _labelDescription;
		private System.Windows.Forms.Button _buttonCopyAndClose;
	}
}
