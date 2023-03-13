using L10NSharp.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffLocalizedStringCacheTests
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

		[TestCase(3, "{\u09E6} \u09A7\u09B0\u09A3\u09BE '{1}' \u09AC\u09B9\u09BE\u09B0 {\u09E8}pt.", false, TestName="CheckSubstitutionMarkers_18")]
		public void CheckStringsForValidSubstitutionMarkers(int markerCount, string formatting, bool isValid)
		{
			Assert.That(XliffLocalizedStringCache.CheckForValidSubstitutionMarkers(markerCount,
				formatting, "a.b"), Is.EqualTo(isValid));
		}

		[Test]
		[TestCase("\u0645 '{\u200E'{0 \u0627",           "\u0645 \u200E'{0}'\u200F \u0627",      TestName = "FixBrokenFormattingString_Works_1")]
		[TestCase("\u0647 '{\u200E'{0\u0646\u0627",      "\u0647 \u200E'{0}'\u200F\u0646\u0627", TestName = "FixBrokenFormattingString_Works_2")]
		[TestCase("\u0632 \"{\u200E\"{0\u200F \u0627",   "\u0632 \u200E\"{0}\"\u200F \u0627",    TestName = "FixBrokenFormattingString_Works_3")]
		[TestCase("\u0627\u06CC {\u200E0}\u200F \u0627", "\u0627\u06CC \u200E{0}\u200F \u0627",  TestName = "FixBrokenFormattingString_Works_4")]
		[TestCase("\u0627\u06CC {\u200E{0",              "\u0627\u06CC \u200E{0}\u200F",         TestName = "FixBrokenFormattingString_Works_5")]
		[TestCase("\u0647 '{\u200E0}'\u200F \u0627",     "\u0647 \u200E'{0}'\u200F \u0627",      TestName = "FixBrokenFormattingString_Works_6")]
		[TestCase("\u062A\u0646 {\u200E {0 \u0631",      "\u062A\u0646 \u200E{0}\u200F  \u0631", TestName = "FixBrokenFormattingString_Works_7")]
		[TestCase("\u0632 \"{\u200E\"{0\u200F.",         "\u0632 \u200E\"{0}\"\u200F.",          TestName = "FixBrokenFormattingString_Works_8")]

		[TestCase("{\u09E6} \u09A7\u09B0\u09A3",         "{0} \u09A7\u09B0\u09A3",               TestName = "FixBrokenFormattingString_Works_9")]
		[TestCase("{\u09E6} \u09A7\u09B0\u09A3\u09BE '{1}' \u09AC\u09B9\u09BE\u09B0 {\u09E8}pt.",
						"{0} \u09A7\u09B0\u09A3\u09BE '{1}' \u09AC\u09B9\u09BE\u09B0 {2}pt.",    TestName = "FixBrokenFormattingString_Works_10")]

		[TestCase("\u0632 \"{\"{0. \u0631",              "\u0632 \u200E\"{0}\"\u200F. \u0631",   TestName = "FixBrokenFormattingString_Works_11")]
		[TestCase("\u0632 \"{\"{0 \u0631",               "\u0632 \u200E\"{0}\"\u200F \u0631",    TestName = "FixBrokenFormattingString_Works_12")]
		[TestCase("\u0632 \"{\"{0 \u0631",               "\u0632 \u200E\"{0}\"\u200F \u0631",    TestName = "FixBrokenFormattingString_Works_13")]
		[TestCase("\u0632 0}{{. \u0631",                 "\u0632 \u200E{0}\u200F. \u0631",       TestName = "FixBrokenFormattingString_Works_14")]
		public void TryToFixBrokenSubstitutionMarkers(string badFormat, string goodFormat)
		{
			var result = XliffLocalizedStringCache.FixBrokenFormattingString(badFormat);
			Assert.That(result, Is.EqualTo(goodFormat));
			// Check for the maximum number of possible substitution markers: unused arguments don't matter for validity.
			Assert.That(XliffLocalizedStringCache.CheckForValidSubstitutionMarkers(3, result, "a.b"), Is.EqualTo(true));
		}

		// This checks for a wider range of substitution marker numbers.
		[Test]
		[TestCase("\u0645 '{\u200E'{10 \u0627",           "\u0645 \u200E'{10}'\u200F \u0627",      TestName = "FixBrokenSubstitution_Works_1")]
		[TestCase("\u0647 '{\u200E'{11\u0646\u0627",      "\u0647 \u200E'{11}'\u200F\u0646\u0627", TestName = "FixBrokenSubstitution_Works_2")]
		[TestCase("\u0632 \"{\u200E\"{12\u200F \u0627",   "\u0632 \u200E\"{12}\"\u200F \u0627",    TestName = "FixBrokenSubstitution_Works_3")]
		[TestCase("\u0627\u06CC {\u200E13}\u200F \u0627", "\u0627\u06CC \u200E{13}\u200F \u0627",  TestName = "FixBrokenSubstitution_Works_4")]
		[TestCase("\u0627\u06CC {\u200E{14",              "\u0627\u06CC \u200E{14}\u200F",         TestName = "FixBrokenSubstitution_Works_5")]
		[TestCase("\u0647 '{\u200E15}'\u200F \u0627",     "\u0647 \u200E'{15}'\u200F \u0627",      TestName = "FixBrokenSubstitution_Works_6")]
		[TestCase("\u062A\u0646 {\u200E {16 \u0631",      "\u062A\u0646 \u200E{16}\u200F  \u0631", TestName = "FixBrokenSubstitution_Works_7")]
		[TestCase("\u0632 \"{\u200E\"{17\u200F.",         "\u0632 \u200E\"{17}\"\u200F.",          TestName = "FixBrokenSubstitution_Works_8")]
		[TestCase("\u0632 \"{\"{18. \u0631",              "\u0632 \u200E\"{18}\"\u200F. \u0631",   TestName = "FixBrokenSubstitution_Works_9")]
		[TestCase("\u0632 \"{\"{19 \u0631",               "\u0632 \u200E\"{19}\"\u200F \u0631",    TestName = "FixBrokenSubstitution_Works_10")]
		[TestCase("\u0632 \"{\"{20 \u0631",               "\u0632 \u200E\"{20}\"\u200F \u0631",    TestName = "FixBrokenSubstitution_Works_11")]
		[TestCase("\u0632 21}{{. \u0631",                 "\u0632 \u200E{21}\u200F. \u0631",       TestName = "FixBrokenSubstitution_Works_12")]
		public void FixBrokenSubstitutionMarkersOnly(string badFormat, string goodFormat)
		{
			var result = XliffLocalizedStringCache.FixBrokenFormattingString(badFormat);
			Assert.That(result, Is.EqualTo(goodFormat));
		}
	}
}
