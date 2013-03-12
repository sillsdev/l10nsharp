using System;
using System.Windows.Forms;
using Localization;
using SampleApp.Properties;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			uiLanguageComboBox1.SelectedLanguage = Settings.Default.UserInterfaceLanguage;
		}

		private void uiLanguageComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.UserInterfaceLanguage = uiLanguageComboBox1.SelectedLanguage;
			LocalizationManager.SetUILanguage(uiLanguageComboBox1.SelectedLanguage, true);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox(this);
		}
	}
}
