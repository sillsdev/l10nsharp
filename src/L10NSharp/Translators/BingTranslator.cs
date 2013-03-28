using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using L10NSharp.BingTranslatorService;

namespace Localization.Translators
{
	/// ----------------------------------------------------------------------------------------
	public class BingTranslator : TranslatorBase
	{
		// This is my (David Olson) personal application id, acquired from
		// Microsoft at http://www.bing.com/developer.
		private const string kAppId = "9E98329DE301A6F28025BEFBB66DBD44C1C7265E";

		/// ------------------------------------------------------------------------------------
		protected LanguageServiceClient m_translator;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="BingTranslator"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public BingTranslator(string srcLangId, string tgtLangId)
		{
			EndpointAddress endpoint = new EndpointAddress("http://api.microsofttranslator.com/v1/Soap.svc");
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Name = "BasicHttpBinding_LanguageService";
			binding.CloseTimeout = new TimeSpan(0, 0, 40);
			binding.OpenTimeout = new TimeSpan(0, 0, 40);
			binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
			binding.SendTimeout = new TimeSpan(0, 1, 0);
			binding.AllowCookies = false;
			binding.BypassProxyOnLocal = false;
			binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
			binding.MaxBufferSize = 65536;
			binding.MaxBufferPoolSize = 524288;
			binding.MaxReceivedMessageSize = 65536;
			binding.MessageEncoding = WSMessageEncoding.Text;
			binding.TextEncoding = Encoding.UTF8;
			binding.TransferMode = TransferMode.Buffered;
			binding.UseDefaultWebProxy = true;
			binding.ReaderQuotas.MaxDepth = 32;
			binding.ReaderQuotas.MaxStringContentLength = 8192;
			binding.ReaderQuotas.MaxArrayLength = 16384;
			binding.ReaderQuotas.MaxBytesPerRead = 4096;
			binding.ReaderQuotas.MaxNameTableCharCount = 16384;
			binding.Security.Mode = BasicHttpSecurityMode.None;
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
			binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
			binding.Security.Transport.Realm = string.Empty;
			binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
			binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;

			// I found that sometimes kAppId is not recognized as a valid application
			// Id. When that happens, attempt a few more times then give up.
			int retryCount = 4;
			while (m_translator == null && retryCount > 0)
			{
				try
				{
					m_translator = new LanguageServiceClient(binding, endpoint);

					var availableLocales = m_translator.GetLanguages(kAppId);
					m_srcCultureId = ValidateLocale(availableLocales, srcLangId);
					m_tgtCultureId = ValidateLocale(availableLocales, tgtLangId);
					break;
				}
				catch
				{
					m_translator = null;
					retryCount--;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Validates the locale.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static string ValidateLocale(IEnumerable<string> availableLocales, string locale)
		{
			if (availableLocales.Where(x => x == locale).FirstOrDefault() != null)
				return locale;

			int i = locale.IndexOf('-');
			if (i >= 0)
				locale = locale.Substring(0, i);

			return locale;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Internal method for translating the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override string InternalTranslate(string srcText)
		{
			return (m_translator == null ? srcText :
				m_translator.Translate(kAppId, srcText, m_srcCultureId, m_tgtCultureId));
		}
	}
}