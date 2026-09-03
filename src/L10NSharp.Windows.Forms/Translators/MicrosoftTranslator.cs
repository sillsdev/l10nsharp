using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace L10NSharp.Windows.Forms.Translators
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Translates text using the Azure AI Translator Text API v3. Unlike
	/// <see cref="MyMemoryTranslator"/>, this requires a host app to provision its own Azure
	/// Translator resource and supply a subscription key, either by setting
	/// <see cref="SubscriptionKey"/> in code or the L10NSHARP_TRANSLATOR_KEY environment
	/// variable.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class MicrosoftTranslator : TranslatorBase
	{
		private const string kServiceUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";
		private const string kSubscriptionKeyEnvVar = "L10NSHARP_TRANSLATOR_KEY";
		private const string kRegionEnvVar = "L10NSHARP_TRANSLATOR_REGION";

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The Azure Translator subscription key to use. If not set, falls back to the
		/// L10NSHARP_TRANSLATOR_KEY environment variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string SubscriptionKey { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The Azure region of the subscription's Translator resource. Only needed for
		/// regional (as opposed to "Global") resources. If not set, falls back to the
		/// L10NSHARP_TRANSLATOR_REGION environment variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string Region { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The subscription key that will actually be used, from either <see cref="SubscriptionKey"/>
		/// or the L10NSHARP_TRANSLATOR_KEY environment variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string EffectiveSubscriptionKey =>
			string.IsNullOrEmpty(SubscriptionKey) ? Environment.GetEnvironmentVariable(kSubscriptionKeyEnvVar) : SubscriptionKey;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The region that will actually be used, from either <see cref="Region"/> or the
		/// L10NSHARP_TRANSLATOR_REGION environment variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string EffectiveRegion =>
			string.IsNullOrEmpty(Region) ? Environment.GetEnvironmentVariable(kRegionEnvVar) : Region;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// True if a subscription key is available from either <see cref="SubscriptionKey"/>
		/// or the L10NSHARP_TRANSLATOR_KEY environment variable.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool IsConfigured => !string.IsNullOrEmpty(EffectiveSubscriptionKey);

		/// ------------------------------------------------------------------------------------
		public MicrosoftTranslator(string srcCultureId, string tgtCultureId)
		{
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
			var key = EffectiveSubscriptionKey;
			if (string.IsNullOrEmpty(key))
				return srcText;

			try
			{
				var requestUri = $"{kServiceUrl}&from={m_srcCultureId}&to={m_tgtCultureId}";

				using var ms = new MemoryStream();
				var requestSer = new DataContractJsonSerializer(typeof(List<TranslateRequestItem>));
				requestSer.WriteObject(ms, new List<TranslateRequestItem> { new TranslateRequestItem { Text = srcText } });
				var requestBody = Encoding.UTF8.GetString(ms.ToArray());

				using var client = new HttpClient();
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
				var region = EffectiveRegion;
				if (!string.IsNullOrEmpty(region))
					client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);

				using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
				using var response = client.PostAsync(requestUri, content).GetAwaiter().GetResult();
				var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				using var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
				var responseSer = new DataContractJsonSerializer(typeof(List<TranslateResponseItem>));
				var result = responseSer.ReadObject(responseStream) as List<TranslateResponseItem>;

				return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? string.Empty;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	[DataContract]
	internal class TranslateRequestItem
	{
		/// ------------------------------------------------------------------------------------
		[DataMember(Name = "Text")]
		public string Text { get; set; }
	}

	/// ----------------------------------------------------------------------------------------
	[DataContract]
	internal class TranslateResponseItem
	{
		/// ------------------------------------------------------------------------------------
		[DataMember(Name = "translations")]
		public List<TranslationItem> Translations { get; set; }
	}

	/// ----------------------------------------------------------------------------------------
	[DataContract]
	internal class TranslationItem
	{
		/// ------------------------------------------------------------------------------------
		[DataMember(Name = "text")]
		public string Text { get; set; }

		/// ------------------------------------------------------------------------------------
		[DataMember(Name = "to")]
		public string To { get; set; }
	}
}
