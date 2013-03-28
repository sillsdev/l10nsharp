using System;
using System.Drawing;
using System.Windows.Forms;
using Localization;
using SampleApp.Properties;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		private Label _dynamicLabel;

		public Form1()
		{
			InitializeComponent();
			uiLanguageComboBox1.SelectedLanguage = Settings.Default.UserInterfaceLanguage;


			_dynamicLabel = new Label()
			{
				Location = new Point(label1.Location.X+120, label1.Location.Y)
			};
			UpdateDynamicLabel();
			this.Controls.Add(_dynamicLabel);
		}

		//This demonstrates how to handle strings which aren't hard-coded, so can't be discovered
		//by the runtime code scanner. Instead, we ue this GetDynamicString
		private void UpdateDynamicLabel()
		{
			var someRuntimeThing = "some " + "runtime " + "thing";
			_dynamicLabel.Text = LocalizationManager.GetDynamicString("SampleApp", someRuntimeThing, someRuntimeThing);
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
	}
}
