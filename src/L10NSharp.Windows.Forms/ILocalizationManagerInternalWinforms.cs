using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace L10NSharp.Windows.Forms
{
	internal interface ILocalizationManagerInternalWinforms: ILocalizationManagerInternal
	{
		Dictionary<Control, ToolTip> ToolTipCtrls { get; }
		Dictionary<ILocalizableComponent, Dictionary<string, LocalizingInfoWinforms>> LocalizableComponents { get; }

		void ApplyLocalization(IComponent component);
		void ApplyLocalizationsToILocalizableComponent(LocalizingInfoWinforms locInfo);
		void ReapplyLocalizationsToAllComponents();

		void RegisterComponentForLocalizing(IComponent component, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment);
		void RegisterComponentForLocalizing(LocalizingInfoWinforms info,
			Action<ILocalizationManagerInternalWinforms, LocalizingInfoWinforms> successAction);
	}

	internal interface ILocalizationManagerInternalWinforms<T> : ILocalizationManagerInternalWinforms, ILocalizationManagerInternal<T>
	{
		new ILocalizedStringCacheWinforms<T> StringCache { get; }
	}
}
