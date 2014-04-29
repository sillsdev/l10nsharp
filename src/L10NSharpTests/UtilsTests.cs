using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class UtilsTests
	{
		[Test]
		public void TypeDoesNotHaveUnexpectedProperty()
		{
			Assert.That(L10NSharp.UI.Utils.HasProperty(typeof(String), "nonsense"), Is.False);
		}

		[Test]
		public void InstanceDoesNotHaveUnexpectedProperty()
		{
			Assert.That(L10NSharp.UI.Utils.HasProperty("", "nonsense"), Is.False);
		}

		[Test]
		public void InstanceHasExpectedProperty()
		{
			Assert.That(L10NSharp.UI.Utils.HasProperty("", "Length"), Is.True);
		}

		[Test]
		public void TypeHasExpectedProperty()
		{
			Assert.That(L10NSharp.UI.Utils.HasProperty(typeof(int), "MaxValue"), Is.True);
		}

		[Test]
		public void GetPropertyOnExistingMethod_Works()
		{
			Assert.That(UI.Utils.GetProperty("abc", "Length"), Is.EqualTo(3));
		}

		[Test]
		public void GetPropertyOnMissingMethod_ReturnsNull()
		{
			Assert.That(UI.Utils.GetProperty("abc", "nonsence"), Is.Null);
		}

		/// <summary>
		/// Special case test, because in one case we had some crashes where we tried to call this missing method.
		/// </summary>
		[Test]
		public void GetShortcutKeysOnToolStripButton_ReturnsNull()
		{
			var button = new ToolStripButton();
			Assert.That(UI.Utils.GetProperty(button, "ShortcutKeys"), Is.Null);
		}
	}
}
