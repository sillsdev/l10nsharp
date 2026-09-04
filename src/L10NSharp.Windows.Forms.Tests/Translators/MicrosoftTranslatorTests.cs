using System;
using L10NSharp.Windows.Forms.Translators;
using NUnit.Framework;

namespace L10NSharp.Windows.Forms.Tests.Translators
{
	[TestFixture]
	public class MicrosoftTranslatorTests
	{
		private const string kKeyEnvVar = "L10NSHARP_TRANSLATOR_KEY";
		private const string kRegionEnvVar = "L10NSHARP_TRANSLATOR_REGION";

		[TearDown]
		public void TearDown()
		{
			MicrosoftTranslator.SubscriptionKey = null;
			MicrosoftTranslator.Region = null;
			Environment.SetEnvironmentVariable(kKeyEnvVar, null);
			Environment.SetEnvironmentVariable(kRegionEnvVar, null);
		}

		[Test]
		public void IsConfigured_NoKeySetAnywhere_ReturnsFalse()
		{
			Assert.That(MicrosoftTranslator.IsConfigured, Is.False);
		}

		[Test]
		public void IsConfigured_SubscriptionKeySetDirectly_ReturnsTrue()
		{
			MicrosoftTranslator.SubscriptionKey = "some-key";
			Assert.That(MicrosoftTranslator.IsConfigured, Is.True);
		}

		[Test]
		public void IsConfigured_SubscriptionKeySetViaEnvironmentVariable_ReturnsTrue()
		{
			Environment.SetEnvironmentVariable(kKeyEnvVar, "some-key");
			Assert.That(MicrosoftTranslator.IsConfigured, Is.True);
		}

		[Test]
		public void SubscriptionKey_SetDirectly_TakesPrecedenceOverEnvironmentVariable()
		{
			Environment.SetEnvironmentVariable(kKeyEnvVar, "env-key");
			MicrosoftTranslator.SubscriptionKey = "direct-key";
			Assert.That(MicrosoftTranslator.EffectiveSubscriptionKey, Is.EqualTo("direct-key"));
		}

		[Test]
		public void Region_NotSetDirectly_FallsBackToEnvironmentVariable()
		{
			Environment.SetEnvironmentVariable(kRegionEnvVar, "westus");
			Assert.That(MicrosoftTranslator.EffectiveRegion, Is.EqualTo("westus"));
		}

		[Test]
		public void Region_NeitherSet_IsNull()
		{
			Assert.That(MicrosoftTranslator.EffectiveRegion, Is.Null);
		}
	}
}
