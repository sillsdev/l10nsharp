using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using SampleApp.Properties;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		private Label _dynamicLabel;

		public Form1()
		{
			InitializeComponent();
		}

		private void UpdateDynamicLabel()
		{
			if(_dynamicLabel!=null)
				_dynamicLabel.Text = LocalizationManager.GetDynamicString("SampleApp", "The User Name", Environment.UserName);
		}

		private void uiLanguageComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.UserInterfaceLanguage = uiLanguageComboBox1.SelectedLanguage;
			LocalizationManager.SetUILanguage(uiLanguageComboBox1.SelectedLanguage, true);
			UpdateDynamicLabel();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox(this);
			UpdateDynamicLabel();
		}

		//This demonstrates how to handle strings that aren't hard-coded, so can't be discovered
		//by the runtime code scanner. Instead, we ue this GetDynamicString.
		//Note that L10NSharp.LocalizationManager.CollectUpNewStringsDiscoveredDynamically = false
		// can be used to avoid adding new strings to the database when inappropriate.
		private void button1_Click(object sender, EventArgs e)
		{
			_getDynamicStringButton.Enabled = false;

			_dynamicLabel = new Label()
			{
				Location = new Point(_getDynamicStringButton.Location.X + 180, _getDynamicStringButton.Location.Y+10)
			};
			uiLanguageComboBox1.SelectedLanguage = Settings.Default.UserInterfaceLanguage;
			UpdateDynamicLabel();
			this.Controls.Add(_dynamicLabel);
		}
	}
}
