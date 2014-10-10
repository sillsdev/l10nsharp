using System;
using System.Windows.Forms;
using L10NSharp.UI;

namespace L10NSharp
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
		LocalizableComponent,
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
		private object _component;
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
		public LocalizingInfo(object component, bool initTextFromObj)
		{
			_component = component;
			Priority = LocalizationPriority.Medium;
			Category = GetCategory(_component);
			UpdateFields = UpdateFields.All;
			if (initTextFromObj)
				UpdateTextFromObject();
		}

		public void UpdateTextFromObject()
		{
			Text = LocalizationManager.StripOffLocalizationInfoFromText(_component is DataGridViewColumn
																			? ((DataGridViewColumn) _component).HeaderText
																			: Utils.GetProperty(_component, "Text") as string);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizingInfo"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizingInfo(object component, string id) : this(component, true)
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

		internal void CreateIdIfMissing(string prefixForId)
		{
			if (prefixForId == null)
				prefixForId = "";
			if (string.IsNullOrEmpty(_id))
				_id = MakeId(_component, prefixForId);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static string MakeId(object component, string idPrefixFromFormExtender="")
		{
			if (idPrefixFromFormExtender == null)
				idPrefixFromFormExtender = "";

			idPrefixFromFormExtender = idPrefixFromFormExtender.Trim(new char[] { '.',' ' });
			if (idPrefixFromFormExtender.Length > 0)
				idPrefixFromFormExtender = idPrefixFromFormExtender + ".";

			if (component is Form)
			{
				Form frm = (Form)component;
				return (frm.Site != null && frm.Site.DesignMode ? frm.Site.Name : frm.Name) + ".WindowTitle";
			}

			if (component is Control)
				return idPrefixFromFormExtender+MakeIdForCtrl(component as Control);

			if (component is ColumnHeader)
				return idPrefixFromFormExtender + MakeIdForColumnHeader((ColumnHeader)component);

			if (component is DataGridViewColumn)
				return idPrefixFromFormExtender + MakeIdForDataGridViewColumn((DataGridViewColumn)component);

			if (component is ToolStripItem)
			{
				string formName = OwningFormName(component as ToolStripItem);
				return idPrefixFromFormExtender + (formName ?? "Miscellaneous") + "." + ((ToolStripItem)component).Name;
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
			if (string.IsNullOrEmpty(prefix))
				return ctrl.Name;

			//return (string.IsNullOrEmpty(prefix) ? string.Empty : prefix + "." + ctrl.Name);
			return prefix.Trim(new char[] { '.' }) + "." + ctrl.Name.Trim(new char[] { '.' });
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes an id for a column header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string MakeIdForColumnHeader(ColumnHeader hdr)
		{
			return (hdr == null ? null : GetIdFromText(hdr.Name));
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

			int i = text.IndexOf("!", StringComparison.Ordinal);
			//review: this is what David had, but I don't understand it (and the unit test fails with it)
					//return (i < 0 ? string.Empty : text.Substring(0, i));
			return (i < 0 ? text : text.Substring(0, i));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tries to get the name of the form that hosts the specified control. That name is
		/// used as the prefix for a localization id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string GetIdPrefix(Control control)
		{
			if (control == null)
				return "Miscellaneous";
			if (control.Parent == null)
				return "";

			while (control.Parent != null &&
				!control.Parent.GetType().FullName.StartsWith("System.Windows.Forms.Design"))
			{
				control = control.Parent;
			}


			return (control.Site != null && control.Site.DesignMode ? control.Site.Name : control.Name);
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
		/// Returns the localization category for the specified IComponent object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static LocalizationCategory GetCategory(object component)
		{
			if (component is ILocalizableComponent)
				return LocalizationCategory.LocalizableComponent;
			if (component is ToolStripMenuItem)
				return LocalizationCategory.MenuItem;
			if (component is ToolStripItem)
				return LocalizationCategory.ToolbarOrStatusBarItem;
			if (component is Button)
				return LocalizationCategory.Button;
			if (component is TextBox)
				return LocalizationCategory.TextBox;
			if (component is LinkLabel)
				return LocalizationCategory.LinkLabel;
			if (component is Label)
				return LocalizationCategory.Label;
			if (component is ComboBox)
				return LocalizationCategory.ComboBox;
			if (component is RadioButton)
				return LocalizationCategory.RadioButton;
			if (component is CheckBox)
				return LocalizationCategory.CheckBox;
			if (component is Form)
				return LocalizationCategory.WindowOrDialog;
			if (component is TabPage)
				return LocalizationCategory.TabPage;
			if (component is ColumnHeader)
				return LocalizationCategory.ListViewColumnHeading;
			if (component is DataGridViewColumn)
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
					_id = MakeId(_component);

				return _id;
			}
			set
			{
				_id = value;
				if (string.IsNullOrEmpty(_id))
				{
					Priority = LocalizationPriority.NotLocalizable;
				}
				else
				{
					_id = _id.Trim().Replace("..", ".");//it happens...
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal object Component
		{
			get { return _component; }
			set
			{
				_component = value;
				if (_component != null && string.IsNullOrEmpty(_id))
					Id = MakeId(_component);
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
		/// Gets or sets the update fields.
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


		/// <summary>
		/// We need to record this so that the string won't be marked as "unused" the next time the static scanner runs.
		/// For dynamic strings, we actually have no way of knowing if they are still used or not.
		/// </summary>
		public bool DiscoveredDynamically;

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
				if (_shortcutKeys == null && _component != null)
				{
					object keysobj = Utils.GetProperty(_component, "ShortcutKeys");
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
				if (_component != null)
				{
					if (_text == null)
						_text = Utils.GetProperty(_component, "Text") as string;

					if (_text == null)
						_text = Utils.GetProperty(_component, "HeaderText") as string;
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