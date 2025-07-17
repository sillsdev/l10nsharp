using System;
using System.Windows.Forms;
using NUnit.Framework;
using L10NSharp;

namespace L10NSharpWinformsTests
{
	[TestFixture]
	public class UtilsTests
	{
		[Test]
		public void TypeDoesNotHaveUnexpectedProperty()
		{
			Assert.IsFalse(Utils.HasProperty(typeof(String), "nonsense"));
		}

		[Test]
		public void InstanceDoesNotHaveUnexpectedProperty()
		{
			Assert.IsFalse(Utils.HasProperty("", "nonsense"));
		}

		[Test]
		public void InstanceHasExpectedProperty()
		{
			Assert.IsTrue(Utils.HasProperty("", "Length"));
		}

		[Test]
		public void TypeHasExpectedProperty()
		{
			Assert.IsTrue(Utils.HasProperty(typeof(int), "MaxValue"));
		}

		[Test]
		public void GetPropertyOnExistingMethod_Works()
		{
			Assert.AreEqual(3, Utils.GetProperty("abc", "Length"));
		}

		[Test]
		public void GetPropertyOnMissingMethod_ReturnsNull()
		{
			Assert.IsNull(Utils.GetProperty("abc", "nonsence"));
		}

		/// <summary>
		/// Special case test, because in one case we had some crashes where we tried to call this missing method.
		/// </summary>
		[Test]
		public void GetShortcutKeysOnToolStripButton_ReturnsNull()
		{
			var button = new ToolStripButton();
			Assert.IsNull(Utils.GetProperty(button, "ShortcutKeys"));
		}
	}
}
