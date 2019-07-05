using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp.Translators;

namespace L10NSharp.UI
{
	public partial class LanguageChoosingDialog : Form
	{
		private readonly LanguageChoosingDialogViewModel _model;

		public LanguageChoosingDialog(L10NCultureInfo requestedCulture, Icon icon)
		{
			InitializeComponent();
			this.Icon = icon;
			_model = new LanguageChoosingDialogViewModel(_messageLabel.Text, _OKButton.Text, Text, requestedCulture, () => { Application.Idle += Application_Idle; } );
			_messageLabel.Text = _model.Message;
		}

		void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= Application_Idle;
			_model.SetTranslator(new BingTranslator("en", _model.RequestedCultureTwoLetterISOLanguageName));
			_messageLabel.Text = _model.Message;
			_OKButton.Text = _model.AcceptButtonText;
			Text = _model.WindowTitle;
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
