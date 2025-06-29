// Copyright © 2019-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace L10NSharp.WindowsForms
{
	internal interface ILocalizationManagerInternalWinforms: ILocalizationManagerInternal
	{
		Dictionary<Control, ToolTip> ToolTipCtrls { get; }
		Icon ApplicationIcon { get; set; }

		void RegisterComponentForLocalizing(IComponent component, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment);
		void RegisterComponentForLocalizing(LocalizingInfo info,
			Action<ILocalizationManagerInternalWinforms, LocalizingInfo> successAction);

	}

	internal interface ILocalizationManagerInternalWinforms<T> : ILocalizationManagerInternalWinforms, ILocalizationManagerInternal<T>
	{
		
	}
}
