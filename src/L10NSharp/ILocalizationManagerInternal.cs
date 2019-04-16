// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace L10NSharp
{
	internal interface ILocalizationManagerInternal: ILocalizationManager
	{
		Dictionary<IComponent, string> ComponentCache { get; }
		Dictionary<Control, ToolTip> ToolTipCtrls { get; }
		Dictionary<ILocalizableComponent, Dictionary<string, LocalizingInfo>> LocalizableComponents { get; }
		Icon ApplicationIcon { get; set; }

		void ApplyLocalization(IComponent component);
		void ApplyLocalizationsToILocalizableComponent(LocalizingInfo locInfo);

		void ReapplyLocalizationsToAllComponents();

		void RegisterComponentForLocalizing(IComponent component, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment);
		void RegisterComponentForLocalizing(LocalizingInfo info,
			Action<ILocalizationManagerInternal, LocalizingInfo> successAction);

		string GetStringFromStringCache(string uiLangId, string id);

		void SaveIfDirty(ICollection<string> langIdsToForceCreate);
		string GetPathForLanguage(string langId, bool getCustomPathEvenIfNonexistent);
	}

	internal interface ILocalizationManagerInternal<T>: ILocalizationManagerInternal
	{
		ILocalizedStringCache<T> StringCache { get; }
		/// <summary>
		/// Merge and save the document that results from merging <paramref name="newDoc"/>
		/// and the document at <paramref name="oldDocPath"/>.
		/// </summary>
		void MergeTranslationDocuments(string appId, T newDoc, string oldDocPath);
	}
}
