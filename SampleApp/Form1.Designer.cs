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
            this.localizationExtender1 = new Localization.UI.LocalizationExtender(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.uiLanguageComboBox1 = new Localization.UI.UILanguageComboBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.localizationExtender1)).BeginInit();
            this.SuspendLayout();
            // 
            // localizationExtender1
            // 
            this.localizationExtender1.LocalizationManagerId = "SampleApp";
            // 
            // button1
            // 
            this.localizationExtender1.SetLocalizableToolTip(this.button1, null);
            this.localizationExtender1.SetLocalizationComment(this.button1, null);
            this.localizationExtender1.SetLocalizingId(this.button1, "button1.button1");
            this.button1.Location = new System.Drawing.Point(26, 160);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "A Button";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.localizationExtender1.SetLocalizableToolTip(this.label1, null);
            this.localizationExtender1.SetLocalizationComment(this.label1, null);
            this.localizationExtender1.SetLocalizingId(this.label1, "label1.label1");
            this.label1.Location = new System.Drawing.Point(23, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "A Label";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.localizationExtender1.SetLocalizableToolTip(this.label2, null);
            this.localizationExtender1.SetLocalizationComment(this.label2, null);
            this.localizationExtender1.SetLocalizingId(this.label2, "label2.label2");
            this.label2.Location = new System.Drawing.Point(26, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(202, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "or Shift-Ctrl Click on items to localize them";
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
            this.localizationExtender1.SetLocalizingId(this.uiLanguageComboBox1, "uiLanguageComboBox1.uiLanguageComboBox1");
            this.uiLanguageComboBox1.Location = new System.Drawing.Point(26, 12);
            this.uiLanguageComboBox1.Name = "uiLanguageComboBox1";
            this.uiLanguageComboBox1.Size = new System.Drawing.Size(121, 23);
            this.uiLanguageComboBox1.TabIndex = 3;
            this.uiLanguageComboBox1.SelectedIndexChanged += new System.EventHandler(this.uiLanguageComboBox1_SelectedIndexChanged);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.localizationExtender1.SetLocalizableToolTip(this.linkLabel1, null);
            this.localizationExtender1.SetLocalizationComment(this.linkLabel1, null);
            this.localizationExtender1.SetLocalizingId(this.linkLabel1, "linkLabel1.linkLabel1");
            this.linkLabel1.Location = new System.Drawing.Point(26, 59);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(124, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Open Translation Dialog,";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.uiLanguageComboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.localizationExtender1.SetLocalizableToolTip(this, null);
            this.localizationExtender1.SetLocalizationComment(this, null);
            this.localizationExtender1.SetLocalizingId(this, "Form1.WindowTitle");
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.localizationExtender1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Localization.UI.LocalizationExtender localizationExtender1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Localization.UI.UILanguageComboBox uiLanguageComboBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

