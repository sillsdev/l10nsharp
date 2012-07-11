using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Localization.CodeReader;

namespace Localization.UI
{
	/// ----------------------------------------------------------------------------------------
	internal partial class ProgressDlg : Form
	{
		public IEnumerable<LocalizingInfo> ExtractedInfo { get; private set; }

		private readonly string[] _namespaceBeginnings;

		/// ------------------------------------------------------------------------------------
		public ProgressDlg(string appName, params string[] namespaceBeginnings)
		{
			InitializeComponent();
			Text = appName;
			_namespaceBeginnings = namespaceBeginnings;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			Application.Idle += HandleApplicationIdle;

			var extractor = new StringExtractor();
			ExtractedInfo = extractor.ExtractFromNamespaces(pct =>
				_progressBar.Value = Math.Min(pct, 100), _namespaceBeginnings);
		}

		/// ------------------------------------------------------------------------------------
		void HandleApplicationIdle(object sender, EventArgs e)
		{
			if (_progressBar.Value == 100)
			{
				Application.Idle -= HandleApplicationIdle;
				Close();
			}
		}
	}
}
