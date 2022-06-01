using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	internal static class Utils
	{
		private static bool? _isMono;

		public static bool IsMono
		{
			get
			{
				if (_isMono == null)
					_isMono = Type.GetType("Mono.Runtime")
							!= null;

				return (bool)_isMono;
			}
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
		private static extern void SendMessageWindows(IntPtr hWnd, int msg, int wParam, int lParam);

		public static void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			if (IsMono)
				Console.WriteLine("Warning--using unimplemented method SendMessage"); // FIXME Linux
			else
			{
				SendMessageWindows(hWnd, msg, wParam, lParam);
			}
		}


		private const int WM_SETREDRAW = 0xB;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Turns window redrawing on or off. After turning on, the window will be invalidated.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void SetWindowRedraw(Control ctrl, bool turnOn)
		{
			SetWindowRedraw(ctrl, turnOn, true);
		}

		/// ------------------------------------------------------------------------------------
		public static void SetWindowRedraw(Control ctrl, bool turnOn,
			bool invalidateAfterTurningOn)
		{
			if (ctrl != null && !ctrl.IsDisposed && ctrl.IsHandleCreated)
			{
				if (IsMono)
				{
					if (turnOn)
						ctrl.ResumeLayout(invalidateAfterTurningOn);
					else
						ctrl.SuspendLayout();
				}
				else
				{
					SendMessage(ctrl.Handle, WM_SETREDRAW, (turnOn ? 1 : 0), 0);
				}

				if (turnOn && invalidateAfterTurningOn)
					ctrl.Invalidate(true);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Asks whether the specified property on the specified binding exists.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool HasProperty(object binding, string propertyName)
		{
			const BindingFlags flags =
				(BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public);

			// If binding is a Type then assume invoke on a static method, property or field.
			// Otherwise invoke on an instance method, property or field.
			if (binding is Type)
			{
				return ((binding as Type).GetMember(propertyName,
					flags | BindingFlags.Static).Length > 0);
			}

			return binding.GetType().GetMember(propertyName,
				flags | BindingFlags.Instance).Length > 0;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified property on the specified binding.
		/// Note: although this routine attempts to catch anything that might go wrong and
		/// just return null, MissingMethodException cannot be caught. So if you are not sure
		/// the method exists you should check first with HasProperty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetProperty(object binding, string propertyName)
		{
			// It's not clear why this is needed. There is some indication (http://stackoverflow.com/questions/3546580/why-is-it-not-possible-to-catch-missingmethodexception)
			// that MissingMethodException cannot be caught. In one situation in Bloom, with a ToolStripButton (which does not implement ShortcutKeys),
			// it was indeed not caught, and Bloom failed to start. But a simple unit test trying to get the ShortcutKeys of a ToolStripButton returns null successfully even without
			// this check. It seems safest to leave it in, since it currently seems to prevent that crash.
			if (!HasProperty(binding, propertyName))
				return null;
			const BindingFlags flags =
				(BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public);

			try
			{
				// If binding is a Type then assume invoke on a static method, property or field.
				// Otherwise invoke on an instance method, property or field.
				if (binding is Type)
				{
					return ((binding as Type).InvokeMember(propertyName,
						flags | BindingFlags.Static, null, binding, null));
				}

				return binding.GetType().InvokeMember(propertyName,
					flags | BindingFlags.Instance, null, binding, null);
			}
			// Warning: this will (sometimes?) NOT catch the most likely exception, MissingMethodException, because it has been defined as unrecoverable.
			catch { }

			return null;
		}

		public static void InitializeWithAvailableUILocales(this ToolStripDropDownItem menu,
			Action<string> localeSelectedAction, Func<string> getMoreMenuText, Action moreSelected,
			ILocalizationManager lm, Dictionary<string, string> additionalNamedLocales = null,
			ISet<string> languagesNotToSimplify = null)
		{
			menu.DropDownItems.Clear();

			var namedLocales = new SortedDictionary<string, string>();
			if (additionalNamedLocales != null)
			{
				foreach (var additionalLocale in additionalNamedLocales)
					namedLocales[additionalLocale.Key] = additionalLocale.Value;
			}

			foreach (var lang in LocalizationManager.GetUILanguages(true))
			{
				string languageId = lang.IetfLanguageTag;
				namedLocales[lang.DisplayName] = languageId;
			}

			foreach (var locale in namedLocales)
			{
				var item = menu.DropDownItems.Add(locale.Key);
				var languageId = locale.Value;
				item.Tag = languageId;
				item.Click += (a, b) =>
				{
					LocalizationManager.SetUILanguage(languageId, true);
					localeSelectedAction?.Invoke(languageId);
					item.Select();
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				};
				if (languageId == LocalizationManager.UILanguageId)
				{
					if (menu is ToolStripDropDownButton btn)
						btn.Text = item.Text;
				}
			}

			if (getMoreMenuText != null)
			{
				menu.DropDownItems.Add(new ToolStripSeparator());
				var moreMenu = menu.DropDownItems.Add(getMoreMenuText());
				moreMenu.Click += (a, b) =>
				{
					moreSelected?.Invoke();
					lm.ShowLocalizationDialogBox(false);
					menu.InitializeWithAvailableUILocales(localeSelectedAction, getMoreMenuText,
						moreSelected, lm, additionalNamedLocales, languagesNotToSimplify);
				};
			}
		}
	}
}
