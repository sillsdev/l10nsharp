using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	internal partial class InitializationProgressDlgBase : Form
	{
		protected readonly IEnumerable<MethodInfo> _additionalLocalizationMethods;
		protected readonly string[] _namespaceBeginnings;
		private readonly Icon _formIcon;

		/// ------------------------------------------------------------------------------------
		protected InitializationProgressDlgBase(string appName,
			IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings)
		{
			InitializeComponent();
			Text = appName;
			_additionalLocalizationMethods = additionalLocalizationMethods;
			_namespaceBeginnings = namespaceBeginnings;
		}

		protected InitializationProgressDlgBase(string appName, Icon formIcon, 
			IEnumerable<MethodInfo> additionalLocalizationMethods, params string[] namespaceBeginnings) :
			this(appName, additionalLocalizationMethods, namespaceBeginnings)
		{
			_formIcon = formIcon;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			_backgroundWorker.RunWorkerAsync();
		}

		protected virtual void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
		}


		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_progressBar.Value = Math.Min(e.ProgressPercentage, 100);
		}

		protected virtual void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			// a bug in Mono requires us to wait to set Icon until handle created.
			if (_formIcon != null) Icon = _formIcon;
		}
	}
}
