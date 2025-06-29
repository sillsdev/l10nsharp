using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace L10NSharp
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
	}
}
