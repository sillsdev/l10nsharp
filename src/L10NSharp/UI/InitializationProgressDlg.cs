using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using L10NSharp.CodeReader;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	internal partial class InitializationProgressDlg : Form
	{
		public IEnumerable<LocalizingInfo> ExtractedInfo { get; private set; }

		private readonly string[] _namespaceBeginnings;

		/// ------------------------------------------------------------------------------------
		public InitializationProgressDlg(string appName, params string[] namespaceBeginnings)
		{
			InitializeComponent();
			Text = appName;
			_namespaceBeginnings = namespaceBeginnings;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			_backgroundWorker.RunWorkerAsync();
		}

		private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var extractor = new StringExtractor();
			e.Result = extractor.DoExtractingWork(_namespaceBeginnings, sender as BackgroundWorker);
		}


		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_progressBar.Value = Math.Min(e.ProgressPercentage, 100);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ExtractedInfo = (IEnumerable<LocalizingInfo>) e.Result;
			Close();
		}
	}
}
