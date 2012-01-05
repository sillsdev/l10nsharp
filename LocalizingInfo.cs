using System;
using System.Windows.Forms;
using Localization.UI;

namespace Localization
{
	#region Enumerations
	/// ----------------------------------------------------------------------------------------
	public enum LocalizationPriority
	{
		/// <summary></summary>
		High,
		/// <summary></summary>
		MediumHigh,
		/// <summary></summary>
		Medium,
		/// <summary></summary>
		MediumLow,
		/// <summary></summary>
		Low,
		/// <summary></summary>
		NotLocalizable,

		/// <summary>
		/// Do not use this value when passing method from outside
		/// the localization utils. assembly.
		/// </summary>
		InternalUseOnly
	}

	/// ----------------------------------------------------------------------------------------
	public enum LocalizationCategory
	{
		/// <summary></summary>
		DontCare,
		/// <summary></summary>
		WindowOrDialog,
		/// <summary></summary>
		TabPage,
		/// <summary></summary>
		MenuItem,
		/// <summary></summary>
		ToolbarOrStatusBarItem,
		/// <summary></summary>
		Button,
		/// <summary></summary>
		ComboBox,
		/// <summary></summary>
		TextBox,
		/// <summary></summary>
		Label,
		/// <summary></summary>
		ListViewColumnHeading,
		/// <summary></summary>
		DataGridViewColumnHeading,
		/// <summary></summary>
		RadioButton,
		/// <summary></summary>
		CheckBox,
		/// <summary></summary>
		LinkLabel,
		/// <summary></summary>
		SidebarItem,
		/// <summary></summary>
		ErrorOrWarningMessage,
		/// <summary></summary>
		Question,
		/// <summary></summary>
		UndoRedoMessage,
		/// <summary></summary>
		GeneralMessage,
		/// <summary></summary>
		Other,
		/// <summary></summary>
		Unspecified,
	}

	/// ----------------------------------------------------------------------------------------
	[Flags]
	internal enum UpdateFields
	{
		None = 0,
		Text = 1,
		Comment = 2,
		ToolTip = 4,
		ShortcutKeys = 8,
		All = Text | Comment | ToolTip | ShortcutKeys
	}

	#endregion

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class is used to keep track of all the localization information (i.e. extended
	/// properties of the LocalizationExtender) for a single object extended by the
	/// LocalizationExtender. The type of object is either a Control or ToolStripItem and
	/// the information kept track of is the text, tooltip, shortcut keys, localization
	/// priority, comment and localization category.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class LocalizingInfo
	{
		private object _obj;
		private string _id;
		private string _text;
		private string _shortcutKeys;
		private string _comment;
		private LocalizationCategory _category = LocalizationCategory.Unspecified;

		#region Constructors
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizingInfo"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizingInfo(object obj)
		{
			_obj = obj;
			Priority = LocalizationPriority.High;
			Category = GetCategory(_obj);
			UpdateFields = UpdateFields.All;

			Text = LocalizationManager.StripOffLocalizationInfoFromText(_obj is DataGridViewColumn ?
				((DataGridViewColumn)_obj).HeaderText : Utils.GetProperty(_obj, "Text") as string);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizingInfo"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizingInfo(object obj, string id) : this(obj)
		{
			Id = id;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizingInfo"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizingInfo(string id)
		{
			Id = id;
			Priority = LocalizationPriority.MediumHigh;
			UpdateFields = UpdateFields.All;
		}

		#endregion

		#region Methods for initializing the localization id.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string MakeId(object obj)
		{
			if (obj is Form)
			{
				Form frm = (Form)obj;
				return (frm.Site != null && frm.Site.DesignMode ? frm.Site.Name : frm.Name) + ".WindowTitle";
			}

			if (obj is Control)
				return MakeIdForCtrl(obj as Control);

			if (obj is ColumnHeader)
				return MakeIdForColumnHeader((ColumnHeader)obj);

			if (obj is DataGridViewColumn)
				return MakeIdForDataGridViewColumn((DataGridViewColumn)obj);

			if (obj is ToolStripItem)
			{
				string formName = OwningFormName(obj as ToolStripItem);
				return (formName ?? "Miscellaneous") + "." + ((ToolStripItem)obj).Name;
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the id for CTRL.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string MakeIdForCtrl(Control ctrl)
		{
			if (ctrl == null)
				return null;

			if (ctrl.Text.StartsWith(LocalizationManager.kL10NPrefix))
				return GetIdFromText(ctrl.Text);

			string prefix = GetIdPrefix(ctrl);
			return (string.IsNullOrEmpty(prefix) ? string.Empty : prefix + "." + ctrl.Name);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes an id for a column header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string MakeIdForColumnHeader(ColumnHeader hdr)
		{
			return (hdr == null ? null : GetIdFromText(hdr.Text));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes an id for a DataGridView column.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string MakeIdForDataGridViewColumn(DataGridViewColumn col)
		{
			return (col == null || !col.HeaderText.StartsWith(LocalizationManager.kL10NPrefix) ?
				null : GetIdFromText(col.HeaderText));
		}

		/// ------------------------------------------------------------------------------------
		public static string GetIdFromText(string text)
		{
			if (text.StartsWith(LocalizationManager.kL10NPrefix))
				text = text.Substring(LocalizationManager.kL10NPrefix.Length);

			int i = text.IndexOf("!");
			return (i < 0 ? string.Empty : text.Substring(0, i));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tries to get the name of the form that hosts the specified control. That name is
		/// used as the prefix for a localization id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string GetIdPrefix(Control ctrl)
		{
			if (ctrl == null)
				return "Miscellaneous";

			while (ctrl.Parent != null &&
				!ctrl.Parent.GetType().FullName.StartsWith("System.Windows.Forms.Design"))
			{
				ctrl = ctrl.Parent;
			}

			return (ctrl.Site != null && ctrl.Site.DesignMode ? ctrl.Site.Name : ctrl.Name);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the name of the form to which the specified ToolStripItem belongs.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string OwningFormName(ToolStripItem tsItem)
		{
			if (tsItem != null)
			{
				var item = tsItem;
				while (item.OwnerItem != null)
					item = item.OwnerItem;

				if (item.Owner != null)
				{
					var frm = item.Owner.FindForm();
					if (frm != null)
						return (frm.Site != null && frm.Site.DesignMode ? frm.Site.Name : frm.Name);
				}
			}

			return string.Empty;
		}

		#endregion

		#region Method for initializinig the category
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the localization category for the specified object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static LocalizationCategory GetCategory(object obj)
		{
			if (obj is ToolStripMenuItem)
				return LocalizationCategory.MenuItem;
			if (obj is ToolStripItem)
				return LocalizationCategory.ToolbarOrStatusBarItem;
			if (obj is Button)
				return LocalizationCategory.Button;
			if (obj is TextBox)
				return LocalizationCategory.TextBox;
			if (obj is LinkLabel)
				return LocalizationCategory.LinkLabel;
			if (obj is Label)
				return LocalizationCategory.Label;
			if (obj is ComboBox)
				return LocalizationCategory.ComboBox;
			if (obj is RadioButton)
				return LocalizationCategory.RadioButton;
			if (obj is CheckBox)
				return LocalizationCategory.CheckBox;
			if (obj is Form)
				return LocalizationCategory.WindowOrDialog;
			if (obj is TabPage)
				return LocalizationCategory.TabPage;
			if (obj is ColumnHeader)
				return LocalizationCategory.ListViewColumnHeading;
			if (obj is DataGridViewColumn)
				return LocalizationCategory.ListViewColumnHeading;

			return LocalizationCategory.Other;
		}

		#endregion

		#region Properties
		/// ------------------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				return
					((_text ?? string.Empty).Trim() == string.Empty &&
					(ToolTipText ?? string.Empty).Trim() == string.Empty &&
					(_shortcutKeys ?? string.Empty).Trim() == string.Empty &&
					(_comment ?? string.Empty).Trim() == string.Empty);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Id
		{
			get
			{
				if (string.IsNullOrEmpty(_id))
					_id = MakeId(_obj);

				return _id;
			}
			set
			{
				_id = value;
				if (string.IsNullOrEmpty(_id))
					Priority = LocalizationPriority.NotLocalizable;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal object Obj
		{
			get { return _obj; }
			set
			{
				_obj = value;
				if (_obj != null && string.IsNullOrEmpty(_id))
					Id = MakeId(_obj);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the language id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string LangId { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the comment.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal UpdateFields UpdateFields { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the comment.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Comment
		{
			get { return _comment; }
			set { _comment = (value == "null" ? null : value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizationPriority Priority { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the category.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizationCategory Category
		{
			get { return _category; }
			set
			{
				if (value != LocalizationCategory.Unspecified)
					_category = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the group.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Group { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the tooltip.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ToolTipText { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the shortcutKeys.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ShortcutKeys
		{
			get
			{
				if (_shortcutKeys == null && _obj != null)
				{
					object keysobj = Utils.GetProperty(_obj, "ShortcutKeys");
					if (keysobj != null && keysobj.GetType() == typeof(Keys))
					{
						Keys keys = (Keys)keysobj;
						_shortcutKeys = (keys == Keys.None ?
							string.Empty : ShortcutKeysEditor.KeysToString(keys));
					}
				}

				return _shortcutKeys;
			}
			set { _shortcutKeys = (value == Keys.None.ToString() ? string.Empty : value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the text of the object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Text
		{
			get
			{
				if (_obj != null)
				{
					if (_text == null)
						_text = Utils.GetProperty(_obj, "Text") as string;

					if (_text == null)
						_text = Utils.GetProperty(_obj, "HeaderText") as string;
				}

				return _text;
			}
			set { _text = value; }
		}

		#endregion

		public override string ToString()
		{
			return Id + ", " + Text;
		}
	}
}