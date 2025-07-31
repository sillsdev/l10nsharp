using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace L10NSharp.Windows.Forms.UI
{
	/// ------------------------------------------------------------------------------------
	/// <summary>
	/// Possible painting states for DrawHotBackground
	/// </summary>
	/// ------------------------------------------------------------------------------------
	internal enum PaintState
	{
		Normal,
		Hot,
		HotDown,
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Contains misc. static methods for various customized painting.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal class PaintingHelper
	{
		#region Windows API imported methods
		[DllImport("User32.dll")]
		extern static public IntPtr GetWindowDC(IntPtr hwnd);

		[DllImport("User32.dll")]
		extern static public int ReleaseDC(IntPtr hwnd, IntPtr hdc);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		#endregion

		public static int WM_NCPAINT = 0x85;

		///-------------------------------------------------------------------------------------
		/// <summary>
		/// Calculates a color by applying the specified alpha value to the specified front
		/// color, assuming the color behind the front color is the specified back color. The
		/// returned color has the alpha channel set to completely opaque, but whose alpha
		/// channel value appears to be the one specified.
		/// </summary>
		///-------------------------------------------------------------------------------------
		public static Color CalculateColor(Color front, Color back, int alpha)
		{
			// Use alpha blending to brigthen the colors but don't use it
			// directly. Instead derive an opaque color that we can use.
			// -- if we use a color with alpha blending directly we won't be able
			// to paint over whatever color was in the background and there
			// would be shadows of that color showing through
			Color frontColor = Color.FromArgb(255, front);
			Color backColor = Color.FromArgb(255, back);

			float frontRed = frontColor.R;
			float frontGreen = frontColor.G;
			float frontBlue = frontColor.B;
			float backRed = backColor.R;
			float backGreen = backColor.G;
			float backBlue = backColor.B;

			float fRed = frontRed * alpha / 255 + backRed * ((float)(255 - alpha) / 255);
			byte newRed = (byte)fRed;
			float fGreen = frontGreen * alpha / 255 + backGreen * ((float)(255 - alpha) / 255);
			byte newGreen = (byte)fGreen;
			float fBlue = frontBlue * alpha / 255 + backBlue * ((float)(255 - alpha) / 255);
			byte newBlue = (byte)fBlue;

			return Color.FromArgb(255, newRed, newGreen, newBlue);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws around the specified control, a fixed single border the color of text
		/// boxes in a themed environment. If themes are not enabled, the border is black.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawCustomBorder(Control ctrl)
		{
			DrawCustomBorder(ctrl, CanPaintVisualStyle() ?
				VisualStyleInformation.TextControlBorder : Color.Black);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws around the specified control, a fixed single border of the specified color.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawCustomBorder(Control ctrl, Color clrBorder)
		{
			IntPtr hdc = GetWindowDC(ctrl.Handle);

			using (Graphics g = Graphics.FromHdc(hdc))
			{
				Rectangle rc = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
				ControlPaint.DrawBorder(g, rc, clrBorder, ButtonBorderStyle.Solid);
			}

			ReleaseDC(ctrl.Handle, hdc);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws a background in the specified rectangle that looks like a toolbar button
		/// when the mouse is over it, with consideration for whether the look should be like
		/// the mouse is down or not. Note, when a PaintState of normal is specified, this
		/// method does nothing. Normal background painting is up to the caller.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawHotBackground(Graphics g, Rectangle rc, PaintState state)
		{
			// The caller has to handle painting when the state is normal.
			if (state == PaintState.Normal)
				return;

			var hotDown = (state == PaintState.HotDown);

			var clr1 = (hotDown ? ProfessionalColors.ButtonPressedGradientBegin :
				ProfessionalColors.ButtonSelectedGradientBegin);

			var clr2 = (hotDown ? ProfessionalColors.ButtonPressedGradientEnd :
				 ProfessionalColors.ButtonSelectedGradientEnd);

			using (var br = new LinearGradientBrush(rc, clr1, clr2, 90))
					g.FillRectangle(br, rc);

			var clrBrdr = (hotDown ? ProfessionalColors.ButtonPressedHighlightBorder :
				ProfessionalColors.ButtonSelectedHighlightBorder);

			ControlPaint.DrawBorder(g, rc, clrBrdr, ButtonBorderStyle.Solid);

			//// Determine the highlight color.
			//Color clrHot = (CanPaintVisualStyle() ?
			//    VisualStyleInformation.ControlHighlightHot : SystemColors.MenuHighlight);

			//int alpha = (CanPaintVisualStyle() ? 95 : 120);

			//// Determine the angle and one of the colors for the gradient highlight. When state is
			//// hot down, the gradiant goes from bottom (lighter) to top (darker). When the state
			//// is just hot, the gradient is from top (lighter) to bottom (darker).
			//float angle = (state == PaintState.HotDown ? 270 : 90);
			//Color clr2 = ColorHelper.CalculateColor(Color.White, clrHot, alpha);

			//// Draw the label's background.
			//if (state == PaintState.Hot)
			//{
			//    using (LinearGradientBrush br = new LinearGradientBrush(rc, Color.White, clr2, angle))
			//        g.FillRectangle(br, rc);
			//}
			//else
			//{
			//    using (LinearGradientBrush br = new LinearGradientBrush(rc, clr2, clrHot, angle))
			//        g.FillRectangle(br, rc);
			//}

			//// Draw a black border around the label.
			//ControlPaint.DrawBorder(g, rc, Color.Black, ButtonBorderStyle.Solid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a value indicating whether or not visual style rendering is supported
		/// in the application and if the specified element can be rendered.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool CanPaintVisualStyle(VisualStyleElement element)
		{
			return (CanPaintVisualStyle() && VisualStyleRenderer.IsElementDefined(element));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a value indicating whether or not visual style rendering is supported
		/// in the application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool CanPaintVisualStyle()
		{
			return (Application.VisualStyleState != VisualStyleState.NoneEnabled &&
				VisualStyleInformation.IsSupportedByOS &&
				VisualStyleInformation.IsEnabledByUser &&
				VisualStyleRenderer.IsSupported);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Because the popup containers forces a little padding above and below, we need to get
		/// the popup's parent (which is the popup container) and paint its background to match
		/// the menu color.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Graphics PaintDropDownContainer(IntPtr hwnd, bool returnGraphics)
		{
			var hwndParent = GetParent(hwnd);
			var g = Graphics.FromHwnd(hwndParent);
			var rc = g.VisibleClipBounds;
			rc.Inflate(-1, -1);
			g.FillRectangle(SystemBrushes.Menu, rc);

			if (!returnGraphics)
			{
				g.Dispose();
				g = null;
			}

			return g;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background consistent with the
		/// current system's color scheme.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc)
		{
			DrawGradientBackground(g, rc, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background consistent with the
		/// current system's color scheme.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc, bool makeDark)
		{
			Color clrTop;
			Color clrBottom;

			if (makeDark)
			{
				clrTop = ColorHelper.CalculateColor(Color.White,
					SystemColors.ActiveCaption, 70);

				clrBottom = ColorHelper.CalculateColor(SystemColors.ActiveCaption,
					SystemColors.ActiveCaption, 0);
			}
			else
			{
				clrTop = ColorHelper.CalculateColor(Color.White,
					SystemColors.GradientActiveCaption, 190);

				clrBottom = ColorHelper.CalculateColor(SystemColors.ActiveCaption,
					SystemColors.GradientActiveCaption, 50);
			}

			DrawGradientBackground(g, rc, clrTop, clrBottom, makeDark);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Fills the specified rectangle with a gradient background using the specified
		/// colors.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void DrawGradientBackground(Graphics g, Rectangle rc,
			Color clrTop, Color clrBottom, bool makeDark)
		{
			try
			{
				if (rc.Width > 0 && rc.Height > 0)
				{
					using (var br = new LinearGradientBrush(rc, clrTop, clrBottom, 90))
						g.FillRectangle(br, rc);
				}
			}
			catch { }
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class NoToolStripBorderRenderer : ToolStripProfessionalRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			// Eat this event.
		}
	}
}
