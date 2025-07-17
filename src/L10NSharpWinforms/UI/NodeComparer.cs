// Copyright © 2012-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace L10NSharpWinforms.UI
{
	/// ----------------------------------------------------------------------------------------
	internal class NodeComparer<T> : IComparer<LocTreeNode<T>>
	{
		internal enum SortField
		{
			Id            = 0,
			SourceText    = 1,
			TargetText    = 2,
			SourceToolTip = 3,
			TargetToolTip = 4
		}

		private readonly string    _srcLangId;
		private readonly string    _tgtLangId;
		private readonly SortOrder _sortOrder;
		private readonly SortField _sortField;

		/// ------------------------------------------------------------------------------------
		internal NodeComparer(string srcLangId, string tgtLangId, SortOrder sortOrder, SortField sortField)
		{
			_srcLangId = srcLangId;
			_tgtLangId = tgtLangId;
			_sortOrder = sortOrder;
			_sortField = sortField;
		}

		/// ------------------------------------------------------------------------------------
		public int  Compare(LocTreeNode<T> x, LocTreeNode<T> y)
		{
			string xText = string.Empty;
			string yText = string.Empty;

			var prefixToRemove = (x.TreeView != null && x.TreeView.SelectedNode != null ?
				x.TreeView.SelectedNode.Name : string.Empty);

			const string kNonsenseIfNoPrefixExists = "5%ij#a"; //replace fails if the pattern is "", so use this
			if (string.IsNullOrEmpty(prefixToRemove))
				prefixToRemove = kNonsenseIfNoPrefixExists;

			var ci = CultureInfo.GetCultureInfo("en");

			switch ((int)_sortField)
			{
				case 0:
					xText = x.Id.Replace(prefixToRemove, string.Empty).Trim('.');
					yText = y.Id.Replace(prefixToRemove, string.Empty).Trim('.');
					break;

				case 1:
					xText = x.GetText(_srcLangId) ?? string.Empty;
					yText = y.GetText(_srcLangId) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_srcLangId);
					break;

				case 2:
					xText = (x.GetTranslatedText(_tgtLangId) ?? x.GetText(_tgtLangId)) ?? string.Empty;
					yText = (y.GetTranslatedText(_tgtLangId) ?? y.GetText(_tgtLangId)) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_tgtLangId);
					break;

				case 3:
					xText = x.GetToolTip(_srcLangId) ?? string.Empty;
					yText = y.GetToolTip(_srcLangId) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_srcLangId);
					break;

				case 4:
					xText = (x.GetTranslatedToolTip(_tgtLangId) ?? x.GetToolTip(_tgtLangId)) ?? string.Empty;
					yText = (y.GetTranslatedToolTip(_tgtLangId) ?? y.GetToolTip(_tgtLangId)) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_tgtLangId);
					break;
			}

			return (_sortOrder == SortOrder.Ascending ?
				string.Compare(xText, yText, false, ci) :
				string.Compare(yText, xText, false, ci));
		}
	}
}
