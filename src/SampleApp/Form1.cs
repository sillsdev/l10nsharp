using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.Windows.Forms;
using L10NSharp.Windows.Forms.UIComponents;
using SampleApp.Properties;

namespace SampleApp
{
	public partial class Form1 : Form
	{
		private Label _dynamicLabel;

		// "ar" is deliberately excluded: LanguageChoosingDialog doesn't set RightToLeft, so
		// Arabic would demo a layout bug rather than the translator.
		private static readonly string[] kLanguageChoosingDialogDemoLanguages = { "de", "it", "fr" };
		private static readonly Random kRandom = new Random();

		public Form1()
		{
			InitializeComponent();

			Program.PrimaryLocalizationManager.UiLanguageChanged += HandleFormLocalized;
			HandleFormLocalized(Program.PrimaryLocalizationManager, EventArgs.Empty);
		}

		private void HandleFormLocalized(object sender, EventArgs e)
		{
			if (sender != Program.PrimaryLocalizationManager)
				throw new InvalidOperationException(
					$"The {nameof(sender)} should have been the primary localization manager on which we subscribed to handle the {nameof(ILocalizationManager.UiLanguageChanged)} event.");

			// At this point, L10NSharpExtender has already reapplied localization,
			// so label2.Text contains the localized *format string*, not a previously
			// formatted value.
			var format = label2.Text;
			var now = DateTime.Now;
			label2.Text = string.Format(format, now.ToShortTimeString(), now.ToShortDateString());
		}

		private void UpdateDynamicLabel()
		{
			if (_dynamicLabel != null)
				_dynamicLabel.Text = LocalizationManager.GetDynamicString("SampleApp", "The User Name", Environment.UserName);
		}

		private void uiLanguageComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.UserInterfaceLanguage = uiLanguageComboBox1.SelectedLanguage;
			LocalizationManagerWinforms.SetUILanguage(uiLanguageComboBox1.SelectedLanguage, true);
			UpdateDynamicLabel();
		}

		// This demonstrates how to handle strings that aren't hard-coded, so can't be discovered
		// by the runtime code scanner. Instead, we ue this GetDynamicString.
		// Note that L10NSharp.LocalizationManager.CollectUpNewStringsDiscoveredDynamically = false
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
			Controls.Add(_dynamicLabel);
		}

		// This demonstrates LanguageChoosingDialog's fail-safe, on-the-fly translation of its
		// own title/message/OK button text: normally it's only shown when a requested UI
		// language has no localization files installed, so this button lets you see it (and
		// the translator behind it -- MyMemoryTranslator by default, or MicrosoftTranslator if
		// you've set MicrosoftTranslator.SubscriptionKey) on demand. The target language is
		// picked randomly each click so you can see a few different translations.
		private void showLanguageChoosingDialogButton_Click(object sender, EventArgs e)
		{
			var cultureId = kLanguageChoosingDialogDemoLanguages[kRandom.Next(kLanguageChoosingDialogDemoLanguages.Length)];
			using var dlg = new LanguageChoosingDialog(L10NCultureInfo.GetCultureInfo(cultureId), Icon);
			dlg.ShowDialog(this);
		}
	}
}
