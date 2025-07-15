// // Copyright © 2019-2025 SIL Global
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp.WindowsForms.UI;

namespace L10NSharp.WindowsForms
{
	internal interface ILocalizedStringCacheWinforms<T>:ILocalizedStringCache<T>
	{
		List<LocTreeNode<T>> LeafNodeList { get; }
		Keys GetShortcutKeys(string langId, string id);
		void LoadGroupNodes(TreeNodeCollection topCollection);
	}
}
