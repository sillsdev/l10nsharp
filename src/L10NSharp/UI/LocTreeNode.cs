using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	internal class LocTreeNode<T> : TreeNode
	{
		internal string Group { get; set; }
		internal string Id { get; private set; }
		internal ILocalizationManagerInternal<T> Manager { get; private set; }
		internal Dictionary<string, LocalizingInfo> SavedTranslationInfo { get; private set; }
		internal string SavedComment { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocTreeNode{T}"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal LocTreeNode(ILocalizationManagerInternal<T> manager, string text, string id, string key)
		{
			Manager = manager;
			Text = text;
			Id = id;
			Name = key;

			SavedTranslationInfo = new Dictionary<string, LocalizingInfo>();
		}

		/// ------------------------------------------------------------------------------------
		public string GetText(string langId)
		{
			return Manager?.StringCache.GetString(langId, Id, false);
		}

		/// ------------------------------------------------------------------------------------
		public string GetToolTip(string langId)
		{
			return Manager?.StringCache.GetToolTipText(langId, Id, false);
		}

		/// ------------------------------------------------------------------------------------
		public string GetShortcutKeys(string langId)
		{
			return Manager?.StringCache.GetShortcutKeysText(langId, Id);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedText(string langId)
		{
			return (SavedTranslationInfo.TryGetValue(langId, out var locInfo) ? locInfo.Text : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedToolTip(string langId)
		{
			return (SavedTranslationInfo.TryGetValue(langId, out var locInfo) ? locInfo.ToolTipText : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedShortcutKeys(string langId)
		{
			return (SavedTranslationInfo.TryGetValue(langId, out var locInfo) ? locInfo.ShortcutKeys : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetComment()
		{
			if (SavedComment != null && SavedComment.Trim() != string.Empty)
				return SavedComment;

			return Manager?.StringCache.GetComment(Id);
		}

		/// ------------------------------------------------------------------------------------
		public bool GetHasModifications(bool considerModifiedComment)
		{
			if (SavedTranslationInfo.Values.Any(locInfo => !locInfo.IsEmpty))
				return true;

			return considerModifiedComment && SavedComment != null && SavedComment.Trim() != string.Empty;
		}
	}
}
