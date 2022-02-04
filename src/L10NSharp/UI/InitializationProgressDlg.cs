using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using L10NSharp.CodeReader;

namespace L10NSharp.UI
{
	internal class InitializationProgressDlg<T>: InitializationProgressDlgBase
	{
		public IEnumerable<LocalizingInfo> ExtractedInfo { get; private set; }

		/// ------------------------------------------------------------------------------------
		public InitializationProgressDlg(string appName, IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings):
			base(appName, additionalLocalizationMethods, namespaceBeginnings)
		{
		}

		public InitializationProgressDlg(string appName, Icon formIcon,
			IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings) :
			base(appName, formIcon, additionalLocalizationMethods, namespaceBeginnings)
		{
		}

		protected override void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var extractor = new StringExtractor<T>();
			e.Result = extractor.DoExtractingWork(_additionalLocalizationMethods, _namespaceBeginnings, sender as BackgroundWorker);
		}

		protected override void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var message = $"Error in extracting localizable strings: {e.Error.Message} ({e.Error})";
				Console.WriteLine(message);

				ReportError(message);
			}
			else
			{
				try
				{
					if (e.Result is IEnumerable<LocalizingInfo> info)
					{
						ExtractedInfo = info;
					}
					else
					{
						var got = e.Result == null ? "null" : $"{e.Result.GetType()}: {e.Result}";
						ReportError($"Expected IEnumerable<LocalizingInfo> but got {got}");
					}
				}
				catch (Exception ex)
				{
					var message = $"Error in extracting localizable strings: {ex.Message}";
					Debug.WriteLine(message);
					ReportError(message);
				}
			}

			Close();
		}

		private void ReportError(string message)
		{
			// Adding the error to the ExtractedInfo here serves two purposes.
			// 1. It makes sure we get a valid file. Otherwise we get failures later.
			// 2. It provides a way for the developer to see the actual error which caused extraction to fail.
			ExtractedInfo = new[]
			{
				new LocalizingInfo("StringExtractor_Error")
				{
					LangId = "en",
					Text = "An error occurred while collecting strings or there were no strings to collect. " +
					       "Check comment for exception. Note, the exception may not occur again until you delete this file.",
					Comment = message
				}
			};
		}
	}
}
