using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using L10NSharp.CodeReader;

namespace L10NSharp.UI
{
	internal class InitializationProgressDlg<T>: InitializationProgressDlgBase
	{
		public IEnumerable<LocalizingInfo> ExtractedInfo { get; private set; }

		/// ------------------------------------------------------------------------------------
		public InitializationProgressDlg(string appName, params string[] namespaceBeginnings):
			base(appName, namespaceBeginnings)
		{
		}

		public InitializationProgressDlg(string appName, Icon formIcon,
			params string[] namespaceBeginnings) : base(appName, formIcon, namespaceBeginnings)
		{
		}

		protected override void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var extractor = new StringExtractor<T>();
			e.Result = extractor.DoExtractingWork(_namespaceBeginnings, sender as BackgroundWorker);
		}

		protected override void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				ExtractedInfo = (IEnumerable<LocalizingInfo>) e.Result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error in extracting localizable strings: {0} ({1})", ex.Message, e.Error);
			}
			Close();
		}

	}
}
