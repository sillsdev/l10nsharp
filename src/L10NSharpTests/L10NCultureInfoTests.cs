using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class L10NCultureInfoTests
	{
		private static bool? _isMono;
		private static bool IsMono
		{
			get
			{
				if (_isMono == null)
					_isMono = Type.GetType("Mono.Runtime") != null;

				return (bool)_isMono;
			}
		}

		[Test]
		public void L10NCultureInfo_TestEn()
		{
			var enci = L10NCultureInfo.GetCultureInfo("en");
			Assert.AreEqual("English", enci.EnglishName);
			Assert.IsNotNull(enci.RawCultureInfo);
		}

		[Test]
		public void L10NCultureInfo_TestPbu()
		{
			var pbuci = L10NCultureInfo.GetCultureInfo("pbu");
			Assert.AreEqual("Northern Pashto", pbuci.EnglishName);
			// Linux/Mono4 and Windows7 will have null for pbuci.RawCultureInfo.
			// Windows10 will produce a dummy object with no read information.

			if (IsMono)
				Assert.IsNull(pbuci.RawCultureInfo);
			else if (pbuci.RawCultureInfo != null)
			{
				Assert.AreEqual("pbu", pbuci.RawCultureInfo.Name);
				Assert.AreEqual("Unknown Language (pbu)", pbuci.RawCultureInfo.EnglishName);
			}
		}

		[Test]
		public void L10NCultureInfo_TestList()
		{
			var list = L10NCultureInfo.GetCultures(CultureTypes.AllCultures).ToList();
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("en")));
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("pbu")));
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("prs")));
		}
	}
}
