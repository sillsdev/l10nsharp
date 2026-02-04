using System;
using System.Windows.Forms;

namespace L10NSharp.Windows.Forms.UIComponents
{
	/// ----------------------------------------------------------------------------------------
	internal static class ShortcutKeysConverter
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Convert the specified Keys value to a string representation.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Keys KeysFromString(string keyStr)
		{
			if (string.IsNullOrEmpty(keyStr) || keyStr.Trim() == string.Empty ||
			    keyStr == nameof(Keys.None))
			{
				return Keys.None;
			}

			Keys keys = Keys.None;

			if (keyStr.Contains("Ctrl") || keyStr.Contains("CTRL"))
				keys |= Keys.Control;
			if (keyStr.Contains("Alt") || keyStr.Contains("ALT"))
				keys |= Keys.Alt;
			if (keyStr.Contains("Shift") || keyStr.Contains("SHIFT"))
				keys |= Keys.Shift;

			return keys | GetNonModifierKeyFromString(keyStr);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Convert the specified Keys value to a string representation.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string KeysToString(Keys keys)
		{
			string keyStr = string.Empty;

			if ((keys & Keys.Control) == Keys.Control)
			{
				keyStr += "Ctrl+";
				keys &= ~Keys.Control;
			}

			if ((keys & Keys.Alt) == Keys.Alt)
			{
				keyStr += "Alt+";
				keys &= ~Keys.Alt;
			}

			if ((keys & Keys.Shift) == Keys.Shift)
			{
				keyStr += "Shift+";
				keys &= ~Keys.Shift;
			}

			return keyStr + GetStringFromNonModifierKeys(keys);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// For the values in the Keys enumeration that represent the number keys (i.e., those
		/// across the top of the keyboard), returns the corresponding string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string GetStringFromNonModifierKeys(Keys key)
		{
			// First make sure the keys has no modifiers.
			key &= ~Keys.Control;
			key &= ~Keys.Alt;
			key &= ~Keys.Shift;

			string keyStr = key.ToString();
			if (keyStr.Length == 2 && keyStr[0] == 'D' && keyStr[1] >= '0' && keyStr[1] <= '9')
				return keyStr[1].ToString();

			return keyStr;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static Keys GetNonModifierKeyFromString(string keyStr)
		{
			keyStr = keyStr.Replace("Ctrl", string.Empty);
			keyStr = keyStr.Replace("Alt", string.Empty);
			keyStr = keyStr.Replace("Shift", string.Empty);
			keyStr = keyStr.Replace("CTRL", string.Empty);
			keyStr = keyStr.Replace("ALT", string.Empty);
			keyStr = keyStr.Replace("SHIFT", string.Empty);
			keyStr = keyStr.Replace("+", string.Empty);
			keyStr = keyStr.Replace(",", string.Empty);
			keyStr = keyStr.Replace(" ", string.Empty);
			keyStr = keyStr.Trim();

			if (keyStr.Length == 1 && keyStr[0] >= '0' && keyStr[0] <= '9')
				keyStr = "D" + keyStr;

			try
			{
				return (Keys)Enum.Parse(typeof(Keys), keyStr);
			}
			catch
			{
				return Keys.None;
			}
		}
	}
}
