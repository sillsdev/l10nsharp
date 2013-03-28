using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using L10NSharp.Translators;

namespace L10NSharp.UI
{
	public partial class EditSourceBeforeTranslatingDlg : Form
	{
		/// ------------------------------------------------------------------------------------
		public EditSourceBeforeTranslatingDlg()
		{
			InitializeComponent();
		}

		/// ------------------------------------------------------------------------------------
		public EditSourceBeforeTranslatingDlg(string sourceText, string srcLangId,
			string tgtLangId, string translatorName, ITranslator translator) : this()
		{
			_textBoxSource.Font = SystemFonts.MessageBoxFont;
			_textBoxTarget.Font = SystemFonts.MessageBoxFont;

			_labelDescription.Text = string.Format(_labelDescription.Text, translatorName);
			_lableSource.Text = CultureInfo.GetCultureInfo(srcLangId).DisplayName;
			_lableTarget.Text = CultureInfo.GetCultureInfo(tgtLangId).DisplayName;
			_buttonTranslate.Text = string.Format(_buttonTranslate.Text, _lableSource.Text, _lableTarget.Text);
			_buttonCopyAndClose.Text = string.Format(_buttonCopyAndClose.Text, _lableTarget.Text);

			_textBoxSource.Text = sourceText;

			_buttonCopyAndClose.Click += delegate
			{
				Clipboard.SetText(_textBoxTarget.Text, TextDataFormat.UnicodeText);
				Close();
			};

			_buttonTranslate.Click += delegate
			{
				_textBoxTarget.Text = translator.TranslateText(_textBoxSource.Text.Trim()) ?? string.Empty;
			};
		}
	}
}
