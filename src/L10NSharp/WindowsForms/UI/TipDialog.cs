using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp.Properties;

namespace L10NSharp.WindowsForms.UI
{
	public partial class TipDialog : Form
	{
		public string Message
		{
			get => _message.Text;
			set => _message.Text = value;
		}

		public new Image Icon
		{
			get => _icon.Image;
			set => _icon.Image = value;
		}

		internal static void ShowAltShiftClickTip(IWin32Window owner = null)
		{
			Show("If you click on an item while you hold alt and shift keys down, this tool will " +
				"open up with that item already selected.", owner);
		}

		public static void Show(string message, IWin32Window owner)
		{
			using (var d = new TipDialog(message, "Tip", SystemIcons.Information.ToBitmap()))
			{
				if (owner != null)
					d.StartPosition = FormStartPosition.CenterParent;
				d.ShowDialog(owner);
			}
		}

		private TipDialog()
		{
			InitializeComponent();
			_message.Font = SystemFonts.MessageBoxFont;
			_message.BackColor = BackColor;
			_message.ForeColor = ForeColor;
			_icon.Image = SystemIcons.Warning.ToBitmap();
			base.Icon = SystemIcons.Warning;
			dontShowThisAgainButton1.ResetDontShowMemory(Settings.Default);
			dontShowThisAgainButton1.CloseIfShouldNotShow(Settings.Default, Message);
		}

		public TipDialog(string message, string dialogTitle, Image icon) : this()
		{
			if (icon != null)
				Icon = icon;

			Text = dialogTitle;
			Message = message;
		}

		private void _acceptButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void HandleMessageTextChanged(object sender, EventArgs e)
		{
			AdjustHeights();
		}

		private void AdjustHeights()
		{
			//hack: I don't know why this is needed, but it was chopping off the last line in the case of the following message:
			// "There was a problem connecting to the Internet.\r\nWarning: This machine does not have a live network connection.\r\nConnection attempt failed."
			const int kFudge = 50;
			_message.Height = GetDesiredTextBoxHeight()+kFudge;


			var desiredWindowHeight = tableLayout.Height + Padding.Top +
				Padding.Bottom + (Height - ClientSize.Height);

			var scn = Screen.FromControl(this);
			int maxWindowHeight = scn.WorkingArea.Height - 25;

			if (desiredWindowHeight > maxWindowHeight)
			{
				_message.Height -= (desiredWindowHeight - maxWindowHeight);
				_message.ScrollBars = ScrollBars.Vertical;
			}

			Height = Math.Min(desiredWindowHeight, maxWindowHeight);
		}

		private int GetDesiredTextBoxHeight()
		{
			if (!IsHandleCreated)
				CreateHandle();

			using (var g = _message.CreateGraphics())
			{
				const TextFormatFlags flags = TextFormatFlags.NoClipping | TextFormatFlags.NoPadding |
					TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak;

				return TextRenderer.MeasureText(g, _message.Text, _message.Font,
					new Size(_message.ClientSize.Width, 0), flags).Height;
			}
		}

	}
}
