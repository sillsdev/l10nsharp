using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using L10NSharp.Translators;

namespace L10NSharp.UI
{
	public partial class LanguageChoosingDialog : Form
	{
		private readonly L10NCultureInfo _requestedCulture;
		private readonly string _originalMessageTemplate;

		public LanguageChoosingDialog(L10NCultureInfo requestedCulture, Icon icon)
		{
			_requestedCulture = requestedCulture;
			InitializeComponent();
			this.Icon = icon;
			_originalMessageTemplate = _messageLabel.Text;
			if (requestedCulture.EnglishName == requestedCulture.NativeName)
			{
				// It looks weird and stupid to display "English (English)" or any other such pair where the two strings are the same.
				_originalMessageTemplate = _originalMessageTemplate.Replace(" ({1})", "");
			}
			_messageLabel.Text = string.Format(_originalMessageTemplate, requestedCulture.EnglishName, requestedCulture.NativeName);
			if (requestedCulture.TwoLetterISOLanguageName != "en")
				Application.Idle += Application_Idle;
		}

		void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= Application_Idle;
			var translator = new BingTranslator("en", _requestedCulture.TwoLetterISOLanguageName);
			try
			{
				var s = translator.TranslateText(string.Format(_originalMessageTemplate, _requestedCulture.EnglishName));
				if (s.Contains("{1}") && s.Length > 5) // If we just get back "{1} or "({1})", we won't consider that useful.
				{
					// Bing will presumably have translated the English string into the native language, so now we want
					// to display the English name in parentheses. (As a sanity check, we could look to see whether the
					// native name is in the string, but there could be situations where it may not be an exact match.)
					s = string.Format(s.Replace("{1}", "{0}"), _requestedCulture.EnglishName);
				}
				else if (_originalMessageTemplate.Contains("{1}")) // If the language names are the same, we already weeded out the extra param.
					s = translator.TranslateText(string.Format(_originalMessageTemplate, _requestedCulture.EnglishName, _requestedCulture.NativeName));
				if (!string.IsNullOrEmpty(s))
				{
					_messageLabel.Text = s;
					// In general, we will be able to translate OK and the title bar text iff we were able to translate
					// the message.  This assumption saves a few processor cycles and prevents disappearing text when
					// a language has not been localized (as is likely the case when we display this dialog).
					_OKButton.Text = translator.TranslateText("OK");
					Text = translator.TranslateText(Text);
				}
			}
			catch (Exception)
			{
				//swallow
			}
		}

		public string SelectedLanguage;

		private void _OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			SelectedLanguage = uiLanguageComboBox1.SelectedLanguage;
			base.OnClosing(e);
		}
	}
}
