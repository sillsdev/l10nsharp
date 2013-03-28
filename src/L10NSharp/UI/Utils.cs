using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Localization.UI
{
	/// ----------------------------------------------------------------------------------------
	internal static class Utils
	{

#if !__MonoCS__
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
#else
		public static void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			Console.WriteLine("Warning--using unimplemented method SendMessage"); // FIXME Linux
			return;
		}
#endif


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
#if !__MonoCS__
				SendMessage(ctrl.Handle, WM_SETREDRAW, (turnOn ? 1 : 0), 0);
#else
				if (turnOn)
					ctrl.ResumeLayout(invalidateAfterTurningOn);
				else
					ctrl.SuspendLayout();
#endif
				if (turnOn && invalidateAfterTurningOn)
					ctrl.Invalidate(true);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified property on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetProperty(object binding, string propertyName)
		{
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
			catch { }

			return null;
		}
	}
}
