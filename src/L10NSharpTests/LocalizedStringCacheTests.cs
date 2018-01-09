using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class LocalizedStringCacheTests
	{
		[Test]
		[TestCase(0, "This is a test.", true, TestName="CheckSubstitutionMarkers_1")]
		[TestCase(1, "This is a {0}.", true, TestName="CheckSubstitutionMarkers_2")]
		[TestCase(2, "This is {0} for {1}.", true, TestName="CheckSubstitutionMarkers_3")]
		[TestCase(3, "This is a {0} test of {1} for {2}.", true, TestName="CheckSubstitutionMarkers_4")]
		[TestCase(4, "This is a {3} test {2} for {1} because {0}.", true, TestName="CheckSubstitutionMarkers_5")]

		[TestCase(1, "This is a {0{.", false, TestName="CheckSubstitutionMarkers_6")]
		[TestCase(2, "This is {0 for 1}.", false, TestName="CheckSubstitutionMarkers_7")]
		[TestCase(3, "This is a }0} test of { 1 } for {2}.", false, TestName="CheckSubstitutionMarkers_8")]
		[TestCase(4, "This is a {3} test {} for {1} because {0}.", false, TestName="CheckSubstitutionMarkers_9")]

		[TestCase(1, "\u0645 '{\u200E'{0 \u0627", false, TestName="CheckSubstitutionMarkers_10")]
		[TestCase(1, "\u0647 '{\u200E'{0\u0646\u0627", false, TestName="CheckSubstitutionMarkers_11")]
		[TestCase(1, "\u0632 \"{\u200E\"{0\u200F \u0627", false, TestName="CheckSubstitutionMarkers_12")]
		[TestCase(1, "\u0627\u06CC {\u200E0}\u200F \u0627", false, TestName="CheckSubstitutionMarkers_13")]
		[TestCase(1, "\u0627\u06CC {\u200E{0", false, TestName="CheckSubstitutionMarkers_14")]
		[TestCase(1, "\u0647 '{\u200E0}'\u200F \u0627", false, TestName="CheckSubstitutionMarkers_15")]
		[TestCase(1, "\u062A\u0646 {\u200E {0 \u0631", false, TestName="CheckSubstitutionMarkers_16")]
		[TestCase(1, "\u0632\u0020\"{\u200E\"{0\u200F.", false, TestName="CheckSubstitutionMarkers_17")]
		public void CheckStringsForValidSubstitionMarkers(int markerCount, string formatting, bool isValid)
		{
			Assert.That(LocalizedStringCache.CheckForValidSubstitutionMarkers(markerCount, formatting, "a.b"), Is.EqualTo(isValid));
		}

		[Test]
		[TestCase("\u0645 '{\u200E'{0 \u0627",           "\u0645 \u200E'{0}'\u200F \u0627",      TestName = "FixBrokenFormattingString_Works_1")]
		[TestCase("\u0647 '{\u200E'{0\u0646\u0627",      "\u0647 \u200E'{0}'\u200F\u0646\u0627", TestName = "FixBrokenFormattingString_Works_2")]
		[TestCase("\u0632 \"{\u200E\"{0\u200F \u0627",   "\u0632 \u200E\"{0}\"\u200F \u0627",    TestName = "FixBrokenFormattingString_Works_3")]
		[TestCase("\u0627\u06CC {\u200E0}\u200F \u0627", "\u0627\u06CC \u200E{0}\u200F \u0627",  TestName = "FixBrokenFormattingString_Works_4")]
		[TestCase("\u0627\u06CC {\u200E{0",              "\u0627\u06CC \u200E{0}\u200F",         TestName = "FixBrokenFormattingString_Works_5")]
		[TestCase("\u0647 '{\u200E0}'\u200F \u0627",     "\u0647 \u200E'{0}'\u200F \u0627",      TestName = "FixBrokenFormattingString_Works_6")]
		[TestCase("\u062A\u0646 {\u200E {0 \u0631",      "\u062A\u0646 \u200E{0}\u200F  \u0631",  TestName = "FixBrokenFormattingString_Works_7")]
		[TestCase("\u0632 \"{\u200E\"{0\u200F.",         "\u0632 \u200E\"{0}\"\u200F.",          TestName = "FixBrokenFormattingString_Works_8")]
		public void TryToFixBrokenSubstitutionMarkers(string badFormat, string goodFormat)
		{
			var result = LocalizedStringCache.FixBrokenFormattingString(badFormat);
			Assert.That(result, Is.EqualTo(goodFormat));
			Assert.That(LocalizedStringCache.CheckForValidSubstitutionMarkers(1, result, "a.b"), Is.True);
		}
	}
}
