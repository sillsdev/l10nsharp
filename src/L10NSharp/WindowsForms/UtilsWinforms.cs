using System.Windows.Forms;

namespace L10NSharp.WindowsForms
{
	/// ----------------------------------------------------------------------------------------
	internal static class UtilsWinforms
	{
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
				if (Utils.IsMono)
				{
					if (turnOn)
						ctrl.ResumeLayout(invalidateAfterTurningOn);
					else
						ctrl.SuspendLayout();
				}
				else
				{
					Utils.SendMessage(ctrl.Handle, WM_SETREDRAW, (turnOn ? 1 : 0), 0);
				}

				if (turnOn && invalidateAfterTurningOn)
					ctrl.Invalidate(true);
			}
		}
	}
}
