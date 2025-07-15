namespace L10NSharp.WindowsForms.UI
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
			this._lblLocalSharing = new System.Windows.Forms.Label();
			this._okButton = new System.Windows.Forms.Button();
			this._lblHowToDistribute = new System.Windows.Forms.LinkLabel();
			this._lblNoteAboutConflictingTranslations = new System.Windows.Forms.Label();
			this._table = new System.Windows.Forms.TableLayoutPanel();
			this._table.SuspendLayout();
			this.SuspendLayout();
			// 
			// _lblLocalSharing
			// 
			this._lblLocalSharing.AutoSize = true;
			this._lblLocalSharing.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lblLocalSharing.Location = new System.Drawing.Point(3, 160);
			this._lblLocalSharing.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this._lblLocalSharing.MaximumSize = new System.Drawing.Size(0, 180);
			this._lblLocalSharing.Name = "_lblLocalSharing";
			this._lblLocalSharing.Size = new System.Drawing.Size(329, 95);
			this._lblLocalSharing.TabIndex = 0;
			this._lblLocalSharing.Text = resources.GetString("_lblLocalSharing.Text");
			// 
			// _okButton
			// 
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.AutoSize = true;
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._okButton.Location = new System.Drawing.Point(260, 374);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			// 
			// _lblHowToDistribute
			// 
			this._lblHowToDistribute.AutoSize = true;
			this._lblHowToDistribute.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lblHowToDistribute.LinkArea = new System.Windows.Forms.LinkArea(81, 84);
			this._lblHowToDistribute.Location = new System.Drawing.Point(3, 107);
			this._lblHowToDistribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this._lblHowToDistribute.MaximumSize = new System.Drawing.Size(0, 180);
			this._lblHowToDistribute.Name = "_lblHowToDistribute";
			this._lblHowToDistribute.Size = new System.Drawing.Size(322, 41);
			this._lblHowToDistribute.TabIndex = 2;
			this._lblHowToDistribute.TabStop = true;
			this._lblHowToDistribute.Text = "To get your translation work into the next version of this program, email {0} to " +
    "{1}";
			this._lblHowToDistribute.UseCompatibleTextRendering = true;
			this._lblHowToDistribute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleLinkClicked);
			// 
			// _lblNoteAboutConflictingTranslations
			// 
			this._lblNoteAboutConflictingTranslations.AutoSize = true;
			this._lblNoteAboutConflictingTranslations.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._lblNoteAboutConflictingTranslations.Location = new System.Drawing.Point(3, 0);
			this._lblNoteAboutConflictingTranslations.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this._lblNoteAboutConflictingTranslations.MaximumSize = new System.Drawing.Size(0, 180);
			this._lblNoteAboutConflictingTranslations.Name = "_lblNoteAboutConflictingTranslations";
			this._lblNoteAboutConflictingTranslations.Size = new System.Drawing.Size(332, 95);
			this._lblNoteAboutConflictingTranslations.TabIndex = 6;
			this._lblNoteAboutConflictingTranslations.Text = resources.GetString("_lblNoteAboutConflictingTranslations.Text");
			// 
			// _table
			// 
			this._table.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._table.ColumnCount = 1;
			this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._table.Controls.Add(this._lblNoteAboutConflictingTranslations, 0, 0);
			this._table.Controls.Add(this._lblHowToDistribute, 0, 1);
			this._table.Controls.Add(this._okButton, 0, 3);
			this._table.Controls.Add(this._lblLocalSharing, 0, 2);
			this._table.Location = new System.Drawing.Point(12, 12);
			this._table.Name = "_table";
			this._table.RowCount = 4;
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._table.Size = new System.Drawing.Size(338, 400);
			this._table.TabIndex = 7;
			// 
			// HowToDistributeDialog
			// 
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._okButton;
			this.ClientSize = new System.Drawing.Size(362, 424);
			this.Controls.Add(this._table);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(378, 463);
			this.Name = "HowToDistributeDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "How To Distribute";
			this._table.ResumeLayout(false);
			this._table.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _lblLocalSharing;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.LinkLabel _lblHowToDistribute;
        private System.Windows.Forms.Label _lblNoteAboutConflictingTranslations;
		private System.Windows.Forms.TableLayoutPanel _table;
	}
}
