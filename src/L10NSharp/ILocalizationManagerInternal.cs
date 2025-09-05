// Copyright © 2019-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel;

namespace L10NSharp
{
	internal interface ILocalizationManagerInternal: ILocalizationManager
	{
		Dictionary<IComponent, string> ComponentCache { get; }

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
