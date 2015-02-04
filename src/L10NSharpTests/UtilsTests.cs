using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class UtilsTests
	{
		[Test]
		public void TypeDoesNotHaveUnexpectedProperty()
		{
			Assert.IsFalse(UI.Utils.HasProperty(typeof(String), "nonsense"));
		}

		[Test]
		public void InstanceDoesNotHaveUnexpectedProperty()
		{
			Assert.IsFalse(UI.Utils.HasProperty("", "nonsense"));
		}

		[Test]
		public void InstanceHasExpectedProperty()
		{
			Assert.IsTrue(UI.Utils.HasProperty("", "Length"));
		}

		[Test]
		public void TypeHasExpectedProperty()
		{
			Assert.IsTrue(UI.Utils.HasProperty(typeof(int), "MaxValue"));
		}

		[Test]
		public void GetPropertyOnExistingMethod_Works()
		{
			Assert.AreEqual(3, UI.Utils.GetProperty("abc", "Length"));
		}

		[Test]
		public void GetPropertyOnMissingMethod_ReturnsNull()
		{
			Assert.IsNull(UI.Utils.GetProperty("abc", "nonsence"));
		}

		/// <summary>
		/// Special case test, because in one case we had some crashes where we tried to call this missing method.
		/// </summary>
		[Test]
		public void GetShortcutKeysOnToolStripButton_ReturnsNull()
		{
			var button = new ToolStripButton();
			Assert.IsNull(UI.Utils.GetProperty(button, "ShortcutKeys"));
		}
	}
}