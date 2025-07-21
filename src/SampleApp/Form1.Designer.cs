using L10NSharp;
using L10NSharp.XLiffUtils;

namespace SampleApp
{
    partial class Form1
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
			this.localizationExtender1 = new L10NSharpWinforms.UI.L10NSharpExtender(this.components);
			this._getDynamicStringButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.uiLanguageComboBox1 = new L10NSharpWinforms.UI.UILanguageComboBox();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.listView1 = new System.Windows.Forms.ListView();
			((System.ComponentModel.ISupportInitialize)(this.localizationExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// localizationExtender1
			// 
			this.localizationExtender1.LocalizationManagerId = "SampleApp";
			this.localizationExtender1.PrefixForNewItems = "TheSampleForm";
			// 
			// _getDynamicStringButton
			// 
			this.localizationExtender1.SetLocalizableToolTip(this._getDynamicStringButton, null);
			this.localizationExtender1.SetLocalizationComment(this._getDynamicStringButton, null);
			this.localizationExtender1.SetLocalizingId(this._getDynamicStringButton, "TheSampleForm.button1");
			this._getDynamicStringButton.Location = new System.Drawing.Point(51, 198);
			this._getDynamicStringButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this._getDynamicStringButton.Name = "_getDynamicStringButton";
			this._getDynamicStringButton.Size = new System.Drawing.Size(168, 28);
			this._getDynamicStringButton.TabIndex = 0;
			this._getDynamicStringButton.Text = "Get Name Dynamically";
			this._getDynamicStringButton.UseVisualStyleBackColor = true;
			this._getDynamicStringButton.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.localizationExtender1.SetLocalizableToolTip(this.label1, null);
			this.localizationExtender1.SetLocalizationComment(this.label1, null);
			this.localizationExtender1.SetLocalizingId(this.label1, "TheSampleForm.ASubHeading.Label");
			this.label1.Location = new System.Drawing.Point(53, 261);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "A Static Label";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.localizationExtender1.SetLocalizableToolTip(this.label2, null);
			this.localizationExtender1.SetLocalizationComment(this.label2, null);
			this.localizationExtender1.SetLocalizingId(this.label2, "TheSampleForm.label2");
			this.label2.Location = new System.Drawing.Point(97, 137);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(249, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Alt+Shift Click on items to localize them";
			// 
			// uiLanguageComboBox1
			// 
			this.uiLanguageComboBox1.DisplayMember = "NativeName";
			this.uiLanguageComboBox1.DropDownHeight = 200;
			this.uiLanguageComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.uiLanguageComboBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.uiLanguageComboBox1.FormattingEnabled = true;
			this.uiLanguageComboBox1.IntegralHeight = false;
			this.localizationExtender1.SetLocalizableToolTip(this.uiLanguageComboBox1, null);
			this.localizationExtender1.SetLocalizationComment(this.uiLanguageComboBox1, null);
			this.localizationExtender1.SetLocalizationPriority(this.uiLanguageComboBox1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.localizationExtender1.SetLocalizingId(this.uiLanguageComboBox1, "uiLanguageComboBox1.uiLanguageComboBox1");
			this.uiLanguageComboBox1.Location = new System.Drawing.Point(53, 50);
			this.uiLanguageComboBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.uiLanguageComboBox1.Name = "uiLanguageComboBox1";
			this.uiLanguageComboBox1.ShowOnlyLanguagesHavingLocalizations = true;
			this.uiLanguageComboBox1.Size = new System.Drawing.Size(160, 28);
			this.uiLanguageComboBox1.TabIndex = 3;
			this.uiLanguageComboBox1.SelectedIndexChanged += new System.EventHandler(this.uiLanguageComboBox1_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.localizationExtender1.SetLocalizableToolTip(this.columnHeader1, null);
			this.localizationExtender1.SetLocalizationComment(this.columnHeader1, null);
			this.localizationExtender1.SetLocalizingId(this.columnHeader1, "TheSampleForm.columnHeader1");
			this.columnHeader1.Text = "One";
			// 
			// columnHeader2
			// 
			this.localizationExtender1.SetLocalizableToolTip(this.columnHeader2, null);
			this.localizationExtender1.SetLocalizationComment(this.columnHeader2, null);
			this.localizationExtender1.SetLocalizingId(this.columnHeader2, "TheSampleForm.columnHeader2");
			this.columnHeader2.Text = "Two";
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
			this.localizationExtender1.SetLocalizableToolTip(this.dataGridView1, null);
			this.localizationExtender1.SetLocalizationComment(this.dataGridView1, null);
			this.localizationExtender1.SetLocalizingId(this.dataGridView1, "TheSampleForm.dataGridView1");
			this.dataGridView1.Location = new System.Drawing.Point(53, 464);
			this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowHeadersWidth = 51;
			this.dataGridView1.Size = new System.Drawing.Size(595, 90);
			this.dataGridView1.TabIndex = 6;
			// 
			// Column1
			// 
			this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column1.HeaderText = "_L10N_:TheSampleForm.SampleDataGridView.ColumnHeadings.FirstColumn!First";
			this.Column1.MinimumWidth = 6;
			this.Column1.Name = "Column1";
			this.Column1.ToolTipText = "My tooltip";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(53, 315);
			this.listView1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(449, 118);
			this.listView1.TabIndex = 5;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(745, 569);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.uiLanguageComboBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._getDynamicStringButton);
			this.localizationExtender1.SetLocalizableToolTip(this, null);
			this.localizationExtender1.SetLocalizationComment(this, null);
			this.localizationExtender1.SetLocalizingId(this, "Form1.WindowTitle");
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Localization Sample App";
			((System.ComponentModel.ISupportInitialize)(this.localizationExtender1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private L10NSharpWinforms.UI.L10NSharpExtender localizationExtender1;
        private System.Windows.Forms.Button _getDynamicStringButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private L10NSharpWinforms.UI.UILanguageComboBox uiLanguageComboBox1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}

