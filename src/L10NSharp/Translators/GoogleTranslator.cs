using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using JetBrains.Annotations;
using L10NSharp.Utility;

namespace L10NSharp.Translators
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[PublicAPI]
	public class GoogleTranslator : TranslatorBase
	{
		private const string kServiceUrl = "http://ajax.googleapis.com/ajax/services/language/translate";

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public GoogleTranslator(string srcCultureId, string tgtCultureId)
		{
			// Google can't handle regions.
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
			var text = HttpUtilityFromMono.UrlPathEncode(srcText);
			var ver = HttpUtilityFromMono.UrlEncode("1.0");
			var langPair = HttpUtilityFromMono.UrlEncode($"{m_srcCultureId}|{m_tgtCultureId}");
			var encodedRequestUrlFragment = $"?v={ver}&q={text}&langpair={langPair}";

			var requestUri = kServiceUrl + encodedRequestUrlFragment;

			try
			{
				using var client = new HttpClient();
				var responseString = client.GetStringAsync(requestUri).GetAwaiter().GetResult(); // sync wait

				using var ms = new MemoryStream(Encoding.Unicode.GetBytes(responseString));
				var ser = new DataContractJsonSerializer(typeof(JSONResponse));
				var translation = ser.ReadObject(ms) as JSONResponse;

				return translation?.responseData?.translatedText ?? string.Empty;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	[Serializable]
	public class JSONResponse
	{
		/// ------------------------------------------------------------------------------------
		public TranslationResponseData responseData = new TranslationResponseData();
		/// ------------------------------------------------------------------------------------
		public string responseDetails;
		/// ------------------------------------------------------------------------------------
		public string responseStatus;
	}

	/// ----------------------------------------------------------------------------------------
	[Serializable]
	public class TranslationResponseData
	{
		/// ------------------------------------------------------------------------------------
		public string translatedText;
	}
}
