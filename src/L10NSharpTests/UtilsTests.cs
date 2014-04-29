using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	}
}
