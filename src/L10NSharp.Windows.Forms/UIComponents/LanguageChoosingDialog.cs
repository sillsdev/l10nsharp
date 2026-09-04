using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using L10NSharp.Windows.Forms.Translators;

namespace L10NSharp.Windows.Forms.UIComponents
{
	public partial class LanguageChoosingDialog : Form
	{
		private readonly LanguageChoosingDialogViewModel _model;

		public LanguageChoosingDialog(L10NCultureInfo requestedCulture, Icon icon)
		{
			InitializeComponent();
			Icon = icon;
			_model = new LanguageChoosingDialogViewModel(_messageLabel.Text, _OKButton.Text, Text, requestedCulture, () => { Application.Idle += Application_Idle; } );
			_messageLabel.Text = _model.Message;
		}

		void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= Application_Idle;
			var targetCultureId = _model.RequestedCultureTwoLetterISOLanguageName;
			TranslatorBase translator;
			if (MicrosoftTranslator.IsConfigured)
				translator = new MicrosoftTranslator("en", targetCultureId);
			else
				translator = new MyMemoryTranslator("en", targetCultureId);

			// Translation makes a blocking network call (see TranslatorBase.TranslateText). Run it
			// on a background thread so a slow or unresponsive endpoint can't freeze this dialog;
			// only the (fast) UI update needs to happen on the UI thread, once translation is done.
			Task.Run(() =>
			{
				_model.TranslateStrings(translator);
				try
				{
					if (!IsDisposed)
					{
						BeginInvoke((Action)(() =>
						{
							if (IsDisposed)
								return;
							_messageLabel.Text = _model.Message;
							_OKButton.Text = _model.AcceptButtonText;
							Text = _model.WindowTitle;
						}));
					}
				}
				catch (ObjectDisposedException)
				{
					// Dialog was closed before translation finished; nothing to update.
				}
			});
		}

		public string SelectedLanguage;

		private void _OKButton_Click(object sender, EventArgs e)
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
