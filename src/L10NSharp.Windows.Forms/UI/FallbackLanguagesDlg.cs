using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using L10NSharp;

namespace L10NSharp.Windows.Forms.UI
{
	public class FallbackLanguagesDlg<T> : FallbackLanguagesDlgBase
	{
		public FallbackLanguagesDlg()
		{
			_uiCulture = CultureInfo.GetCultureInfo(LocalizationManager.UILanguageId);

			_listBoxAvailableLanguages.Items.AddRange(LocalizationManagerInternal<T>.GetUILanguages(false).ToArray());
			_listBoxAvailableLanguages.Items.Remove(_uiCulture);
			_listBoxAvailableLanguages.Items.Remove(CultureInfo.GetCultureInfo(LocalizationManager.kDefaultLang));

			foreach (var langId in LocalizationManagerInternal<T>.FallbackLanguageIds)
				_listBoxFallbackLanguages.Items.Add(CultureInfo.GetCultureInfo(langId));

			_labelMessage.Text = string.Format(_labelMessage.Text, _uiCulture.DisplayName, _uiCulture.DisplayName);

			UpdateDisplay();
		}
	}
}
