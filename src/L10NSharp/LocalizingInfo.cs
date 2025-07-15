using System;
using System.ComponentModel;

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
		private IComponent _component;
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
		public LocalizingInfo(IComponent component, bool initTextFromObj)
		{
			_component = component;
			Priority = LocalizationPriority.Medium;
			UpdateFields = UpdateFields.All;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizingInfo"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizingInfo(IComponent component, string id) : this(component, true)
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
		public static string GetIdFromText(string text)
		{
			if (text.StartsWith(LocalizationManager.kL10NPrefix))
				text = text.Substring(LocalizationManager.kL10NPrefix.Length);

			int i = text.IndexOf("!", StringComparison.Ordinal);
			//review: this is what David had, but I don't understand it (and the unit test fails with it)
					//return (i < 0 ? string.Empty : text.Substring(0, i));
			return (i < 0 ? text : text.Substring(0, i));
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
					_id = null;

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
		internal IComponent Component
		{
			get { return _component; }
			set
			{
				_component = value;
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
		public string ShortcutKeys;

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
