using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace Localization.Translators
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
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
			var text = HttpUtility.UrlPathEncode(srcText);
			var ver = HttpUtility.UrlEncode("1.0");
			var langPair = HttpUtility.UrlEncode(string.Format("{0}|{1}", m_srcCultureId, m_tgtCultureId));
			var encodedRequestUrlFragment = string.Format("?v={0}&q={1}&langpair={2}", ver, text, langPair);

			try
			{
				var request = WebRequest.Create(kServiceUrl + encodedRequestUrlFragment);
				var response = request.GetResponse();

				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					var json = reader.ReadLine();
					using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
					{
						var ser = new DataContractJsonSerializer(typeof(JSONResponse));
						var translation = ser.ReadObject(ms) as JSONResponse;
						reader.Close();
						return translation.responseData.translatedText;
					}
				}
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