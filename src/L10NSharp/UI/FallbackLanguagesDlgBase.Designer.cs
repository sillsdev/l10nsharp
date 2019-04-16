namespace L10NSharp.UI
{
	partial class FallbackLanguagesDlgBase
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FallbackLanguagesDlgBase));
			this._listBoxAvailableLanguages = new System.Windows.Forms.ListBox();
			this._listBoxFallbackLanguages = new System.Windows.Forms.ListBox();
			this._LabelAvailableLanguages = new System.Windows.Forms.Label();
			this._labelFallbackLanguages = new System.Windows.Forms.Label();
			this._buttonRemove = new System.Windows.Forms.Button();
			this._buttonMoveUp = new System.Windows.Forms.Button();
			this._buttonAdd = new System.Windows.Forms.Button();
			this._buttonOK = new System.Windows.Forms.Button();
			this._buttonCancel = new System.Windows.Forms.Button();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._buttonMoveDown = new System.Windows.Forms.Button();
			this._labelMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _listBoxAvailableLanguages
			// 
			this._listBoxAvailableLanguages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._listBoxAvailableLanguages.FormattingEnabled = true;
			this._listBoxAvailableLanguages.ItemHeight = 15;
			this._listBoxAvailableLanguages.Location = new System.Drawing.Point(12, 107);
			this._listBoxAvailableLanguages.Name = "_listBoxAvailableLanguages";
			this._listBoxAvailableLanguages.Size = new System.Drawing.Size(170, 184);
			this._listBoxAvailableLanguages.TabIndex = 2;
			this._listBoxAvailableLanguages.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._listBoxAvailableLanguages_MouseDoubleClick);
			this._listBoxAvailableLanguages.SelectedValueChanged += new System.EventHandler(this._listBoxAvailableLanguages_SelectedValueChanged);
			// 
			// _listBoxFallbackLanguages
			// 
			this._listBoxFallbackLanguages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._listBoxFallbackLanguages.FormattingEnabled = true;
			this._listBoxFallbackLanguages.ItemHeight = 15;
			this._listBoxFallbackLanguages.Location = new System.Drawing.Point(269, 107);
			this._listBoxFallbackLanguages.Name = "_listBoxFallbackLanguages";
			this._listBoxFallbackLanguages.Size = new System.Drawing.Size(170, 184);
			this._listBoxFallbackLanguages.TabIndex = 6;
			this._listBoxFallbackLanguages.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._listBoxFallbackLanguages_MouseDoubleClick);
			this._listBoxFallbackLanguages.SelectedValueChanged += new System.EventHandler(this._listBoxFallbackLanguages_SelectedValueChanged);
			// 
			// _LabelAvailableLanguages
			// 
			this._LabelAvailableLanguages.AutoSize = true;
			this._LabelAvailableLanguages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._LabelAvailableLanguages.Location = new System.Drawing.Point(12, 89);
			this._LabelAvailableLanguages.Name = "_LabelAvailableLanguages";
			this._LabelAvailableLanguages.Size = new System.Drawing.Size(115, 15);
			this._LabelAvailableLanguages.TabIndex = 1;
			this._LabelAvailableLanguages.Text = "&Available Languages";
			// 
			// _labelFallbackLanguages
			// 
			this._labelFallbackLanguages.AutoSize = true;
			this._labelFallbackLanguages.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelFallbackLanguages.Location = new System.Drawing.Point(269, 89);
			this._labelFallbackLanguages.Name = "_labelFallbackLanguages";
			this._labelFallbackLanguages.Size = new System.Drawing.Size(110, 15);
			this._labelFallbackLanguages.TabIndex = 5;
			this._labelFallbackLanguages.Text = "&Fallback Languages";
			// 
			// _buttonRemove
			// 
			this._buttonRemove.Location = new System.Drawing.Point(188, 155);
			this._buttonRemove.Name = "_buttonRemove";
			this._buttonRemove.Size = new System.Drawing.Size(75, 26);
			this._buttonRemove.TabIndex = 4;
			this._buttonRemove.Text = "&Remove";
			this._toolTip.SetToolTip(this._buttonRemove, "Remove selected language from the fallback list");
			this._buttonRemove.UseVisualStyleBackColor = true;
			this._buttonRemove.Click += new System.EventHandler(this._buttonRemove_Click);
			// 
			// _buttonMoveUp
			// 
			this._buttonMoveUp.Image = global::L10NSharp.Properties.Resources.Move_up;
			this._buttonMoveUp.Location = new System.Drawing.Point(445, 123);
			this._buttonMoveUp.Name = "_buttonMoveUp";
			this._buttonMoveUp.Size = new System.Drawing.Size(26, 26);
			this._buttonMoveUp.TabIndex = 7;
			this._toolTip.SetToolTip(this._buttonMoveUp, "Increase priority of selected fallback language");
			this._buttonMoveUp.UseVisualStyleBackColor = true;
			this._buttonMoveUp.Click += new System.EventHandler(this._buttonMoveUp_Click);
			// 
			// _buttonAdd
			// 
            this._buttonAdd.Image = global::L10NSharp.Properties.Resources.Move;
			this._buttonAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._buttonAdd.Location = new System.Drawing.Point(188, 123);
			this._buttonAdd.Name = "_buttonAdd";
			this._buttonAdd.Size = new System.Drawing.Size(75, 26);
			this._buttonAdd.TabIndex = 3;
			this._buttonAdd.Text = "A&dd";
			this._toolTip.SetToolTip(this._buttonAdd, "Adds the selected available language to the fallback list");
			this._buttonAdd.UseVisualStyleBackColor = true;
			this._buttonAdd.Click += new System.EventHandler(this._buttonAdd_Click);
			// 
			// _buttonOK
			// 
			this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._buttonOK.Location = new System.Drawing.Point(313, 318);
			this._buttonOK.Name = "_buttonOK";
			this._buttonOK.Size = new System.Drawing.Size(75, 26);
			this._buttonOK.TabIndex = 9;
			this._buttonOK.Text = "OK";
			this._buttonOK.UseVisualStyleBackColor = true;
			// 
			// _buttonCancel
			// 
			this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._buttonCancel.Location = new System.Drawing.Point(394, 318);
			this._buttonCancel.Name = "_buttonCancel";
			this._buttonCancel.Size = new System.Drawing.Size(75, 26);
			this._buttonCancel.TabIndex = 10;
			this._buttonCancel.Text = "Cancel";
			this._buttonCancel.UseVisualStyleBackColor = true;
			// 
			// _buttonMoveDown
			// 
			this._buttonMoveDown.Image = global::L10NSharp.Properties.Resources.Move_down;
			this._buttonMoveDown.Location = new System.Drawing.Point(445, 155);
			this._buttonMoveDown.Name = "_buttonMoveDown";
			this._buttonMoveDown.Size = new System.Drawing.Size(26, 26);
			this._buttonMoveDown.TabIndex = 8;
			this._toolTip.SetToolTip(this._buttonMoveDown, "Decrease priority of selected fallback language");
			this._buttonMoveDown.UseVisualStyleBackColor = true;
			this._buttonMoveDown.Click += new System.EventHandler(this._buttonMoveDown_Click);
			// 
			// _labelMessage
			// 
			this._labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelMessage.Location = new System.Drawing.Point(12, 13);
			this._labelMessage.Name = "_labelMessage";
			this._labelMessage.Size = new System.Drawing.Size(457, 74);
			this._labelMessage.TabIndex = 0;
			this._labelMessage.Text = resources.GetString("_labelMessage.Text");
			// 
			// FallbackLanguagesDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(481, 356);
			this.Controls.Add(this._labelMessage);
			this.Controls.Add(this._buttonMoveDown);
			this.Controls.Add(this._buttonCancel);
			this.Controls.Add(this._buttonOK);
			this.Controls.Add(this._buttonMoveUp);
			this.Controls.Add(this._buttonRemove);
			this.Controls.Add(this._buttonAdd);
			this.Controls.Add(this._labelFallbackLanguages);
			this.Controls.Add(this._LabelAvailableLanguages);
			this.Controls.Add(this._listBoxFallbackLanguages);
			this.Controls.Add(this._listBoxAvailableLanguages);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FallbackLanguagesDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Fallback Languages";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.ListBox _listBoxAvailableLanguages;
		protected System.Windows.Forms.ListBox _listBoxFallbackLanguages;
		protected System.Windows.Forms.Label _LabelAvailableLanguages;
		protected System.Windows.Forms.Label _labelFallbackLanguages;
		protected System.Windows.Forms.Button _buttonAdd;
		protected System.Windows.Forms.Button _buttonRemove;
		protected System.Windows.Forms.Button _buttonMoveUp;
		protected System.Windows.Forms.Button _buttonOK;
		protected System.Windows.Forms.Button _buttonCancel;
		protected System.Windows.Forms.ToolTip _toolTip;
		protected System.Windows.Forms.Button _buttonMoveDown;
		protected System.Windows.Forms.Label _labelMessage;
	}
}
