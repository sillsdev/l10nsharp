using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using L10NSharp.Utility;

namespace L10NSharp.Windows.Forms.Translators
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Translates text using the free, anonymous MyMemory Translation API
	/// (https://mymemory.translated.net/doc/spec.php). Requires no signup or key, but is
	/// limited to a 5,000 character/day/IP quota, so this is intended only as a fail-safe
	/// default for light, best-effort use. Host apps wanting more robust translation can
	/// configure <see cref="MicrosoftTranslator"/> instead.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal class MyMemoryTranslator : TranslatorBase
	{
		private const string kServiceUrl = "https://api.mymemory.translated.net/get";

		/// ------------------------------------------------------------------------------------
		public MyMemoryTranslator(string srcCultureId, string tgtCultureId)
		{
			// MyMemory can't handle regions.
			int i = srcCultureId.IndexOf('_');
			if (i >= 0)
				srcCultureId = srcCultureId.Substring(0, i);

			i = srcCultureId.IndexOf('-');
			if (i >= 0)
				srcCultureId = srcCultureId.Substring(0, i);

			i = tgtCultureId.IndexOf('_');
			if (i >= 0)
				tgtCultureId = tgtCultureId.Substring(0, i);

			i = tgtCultureId.IndexOf('-');
			if (i >= 0)
				tgtCultureId = tgtCultureId.Substring(0, i);

			m_srcCultureId = srcCultureId;
			m_tgtCultureId = tgtCultureId;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Internal method for translating the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override string InternalTranslate(string srcText)
		{
			var text = HttpUtilityFromMono.UrlEncode(srcText);
			var langPair = HttpUtilityFromMono.UrlEncode($"{m_srcCultureId}|{m_tgtCultureId}");
			var requestUri = $"{kServiceUrl}?q={text}&langpair={langPair}";

			try
			{
				using var client = new HttpClient();
				var responseString = client.GetStringAsync(requestUri).GetAwaiter().GetResult(); // sync wait

				using var ms = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
				var ser = new DataContractJsonSerializer(typeof(JSONResponse));
				var translation = ser.ReadObject(ms) as JSONResponse;

				if (translation == null || translation.quotaFinished)
					return string.Empty;

				return translation.responseData?.translatedText ?? string.Empty;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	[Serializable]
	internal class JSONResponse
	{
		/// ------------------------------------------------------------------------------------
		public TranslationResponseData responseData = new TranslationResponseData();
		/// ------------------------------------------------------------------------------------
		public string responseDetails;
		/// ------------------------------------------------------------------------------------
		public int responseStatus;
		/// ------------------------------------------------------------------------------------
		public bool quotaFinished;
	}

	/// ----------------------------------------------------------------------------------------
	[Serializable]
	internal class TranslationResponseData
	{
		/// ------------------------------------------------------------------------------------
		public string translatedText;
	}
}
