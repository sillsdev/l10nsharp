// // Copyright © 2019-2025 SIL Global
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharpWinforms.UI;

namespace L10NSharpWinforms
{
	internal interface ILocalizedStringCacheWinforms<T>:L10NSharp.ILocalizedStringCache<T>
	{
		List<LocTreeNode<T>> LeafNodeList { get; }
		Keys GetShortcutKeys(string langId, string id);
		void LoadGroupNodes(TreeNodeCollection topCollection);
	}
}
