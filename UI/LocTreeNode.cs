using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Localization.UI
{
	/// ----------------------------------------------------------------------------------------
	internal class LocTreeNode : TreeNode
	{
		internal string Group { get; set; }
		internal string Id { get; private set; }
		internal LocalizationManager Manager { get; private set; }
		internal Dictionary<string, LocalizingInfo> SavedTranslationInfo { get; private set; }
		internal string SavedComment { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocTreeNode"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal LocTreeNode(LocalizationManager manager, string text, string id, string key)
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
			return (Manager != null ? Manager.StringCache.GetString(langId, Id, false) : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetToolTip(string langId)
		{
			return (Manager != null ? Manager.StringCache.GetToolTipText(langId, Id, false) : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetShortcutKeys(string langId)
		{
			return (Manager != null ? Manager.StringCache.GetShortcutKeysText(langId, Id) : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedText(string langId)
		{
			LocalizingInfo locInfo;
			return (SavedTranslationInfo.TryGetValue(langId, out locInfo) ? locInfo.Text : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedToolTip(string langId)
		{
			LocalizingInfo locInfo;
			return (SavedTranslationInfo.TryGetValue(langId, out locInfo) ? locInfo.ToolTipText : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetTranslatedShortcutKeys(string langId)
		{
			LocalizingInfo locInfo;
			return (SavedTranslationInfo.TryGetValue(langId, out locInfo) ? locInfo.ShortcutKeys : null);
		}

		/// ------------------------------------------------------------------------------------
		public string GetComment()
		{
			if (SavedComment != null && SavedComment.Trim() != string.Empty)
				return SavedComment;

			return (Manager != null ? Manager.StringCache.GetComment(Id) : null);
		}

		/// ------------------------------------------------------------------------------------
		public bool GetHasModifications(bool considerModifiedComment)
		{
			if (SavedTranslationInfo.Values.Any(locInfo => !locInfo.IsEmpty))
				return true;

			return (considerModifiedComment && SavedComment != null && SavedComment.Trim() != string.Empty);
		}
	}
}
