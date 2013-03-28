using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Localization.UI
{
	/// ----------------------------------------------------------------------------------------
	internal partial class CustomDropDownComboBox : UserControl
	{
		private bool m_mouseDown;
		private bool m_buttonHot;
		private PopupControl m_popupCtrl;

		/// ------------------------------------------------------------------------------------
		public EventHandler PopupClosed;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="CustomDropDownComboBox"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public CustomDropDownComboBox()
		{
			AlignDropToLeft = true;
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

			TextBox.BackColor = SystemColors.Window;

			Padding = new Padding(Application.RenderWithVisualStyles ?
				SystemInformation.BorderSize.Width : SystemInformation.Border3DSize.Width);

			m_button.Width = SystemInformation.VerticalScrollBarWidth;
			TextBox.Left = Padding.Left + 2;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string Text
		{
			get { return TextBox.Text; }
			set { TextBox.Text = value; }
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets or sets the font of the text displayed by the control.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public override Font Font
		//{
		//    get { return m_txtBox.Font; }
		//    set { m_txtBox.Font = value; }
		//}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the background color for the control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				TextBox.BackColor = value;
				base.BackColor = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TextBox TextBox { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the left edge of the drop-down is aligned
		/// with the left edge of the combo control. To align the left edges, set this value
		/// to true. To align the right edge of the drop-down, set this value to false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AlignDropToLeft { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the popup Control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal PopupControl PopupCtrl
		{
			get { return m_popupCtrl; }
			set
			{
				if (m_popupCtrl != null)
					m_popupCtrl.PopupClosed -= OnPopupClosed;

				m_popupCtrl = value;

				if (m_popupCtrl != null)
					m_popupCtrl.PopupClosed += OnPopupClosed;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the VisibleChanged event of the m_popupCtrl control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual void OnPopupClosed(object sender, EventArgs e)
		{
			if (PopupClosed != null)
				PopupClosed(this, e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This will center the text box vertically within the control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			int newTop = (Height - TextBox.Height) / 2;
			TextBox.Top = (newTop < 0 ? 0 : newTop);
			TextBox.Width = (ClientSize.Width - Padding.Left - Padding.Right - m_button.Width - 2);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (!Application.RenderWithVisualStyles)
				ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
			else
			{
				VisualStyleRenderer renderer = new VisualStyleRenderer(Enabled ?
					VisualStyleElement.TextBox.TextEdit.Normal :
					VisualStyleElement.TextBox.TextEdit.Disabled);

				renderer.DrawBackground(e.Graphics, ClientRectangle, e.ClipRectangle);

				// When the textbox background is drawn in normal mode (at least when the
				// theme is one of the standard XP themes), it's drawn with a white background
				// and not the System Window background color. Therefore, we need to create
				// a rectangle that doesn't include the border. Then fill it with the text
				// box's background color.
				Rectangle rc = renderer.GetBackgroundExtent(e.Graphics, ClientRectangle);
				int dx = (rc.Width - ClientRectangle.Width) / 2;
				int dy = (rc.Height - ClientRectangle.Height) / 2;
				rc = ClientRectangle;
				rc.Inflate(-dx, -dy);

				using (var br = new SolidBrush(TextBox.BackColor))
					e.Graphics.FillRectangle(br, rc);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the painting the button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_button_Paint(object sender, PaintEventArgs e)
		{
			var state = ButtonState.Normal;
			var element = VisualStyleElement.ComboBox.DropDownButton.Normal;

			if (!Enabled)
			{
				state = ButtonState.Inactive;
				element = VisualStyleElement.ComboBox.DropDownButton.Disabled;
			}
			else if (m_mouseDown)
			{
				state = ButtonState.Pushed;
				element = VisualStyleElement.ComboBox.DropDownButton.Pressed;
			}
			else if (m_buttonHot)
				element = VisualStyleElement.ComboBox.DropDownButton.Hot;

			if (!Application.RenderWithVisualStyles)
				PaintNonThemeButton(e.Graphics, state);
			else
			{
				VisualStyleRenderer renderer = new VisualStyleRenderer(element);
				renderer.DrawBackground(e.Graphics, m_button.ClientRectangle);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void PaintNonThemeButton(Graphics g, ButtonState state)
		{
			ControlPaint.DrawButton(g, m_button.ClientRectangle, state);

			using (var fnt = new Font("Marlett", 10))
			{
				TextRenderer.DrawText(g, "6", fnt, m_button.ClientRectangle, SystemColors.ControlDarkDark,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_button_MouseEnter(object sender, EventArgs e)
		{
			m_buttonHot = true;
			m_button.Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_button_MouseLeave(object sender, EventArgs e)
		{
			m_buttonHot = false;
			m_button.Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_button_MouseUp(object sender, MouseEventArgs e)
		{
			// Repaint the drop down button so that it displays normal instead of pressed
			if (m_mouseDown)
			{
				m_mouseDown = false;
				m_button.Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected void m_button_MouseDown(object sender, MouseEventArgs e)
		{
			// Repaint the drop down button so that it displays pressed
			if (e.Button == MouseButtons.Left)
			{
				m_mouseDown = true;
				m_button.Invalidate();
			}

			if (m_popupCtrl != null)
			{
				var pt = new Point(0, Height);
				if (!AlignDropToLeft)
					pt.X -= (m_popupCtrl.Width - Width);

				m_popupCtrl.Show(this, pt);
			}
		}
	}
}
