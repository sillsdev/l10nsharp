using System;

namespace L10NSharp
{
	/// <summary>
	/// This attribute enables a developer to declare to L10NSharp that nothing in a class or method is localizable.
	/// The presence of localizable content may vary depending on the platform so the attribute takes an optional OS enum value.
	/// <remarks>
	/// This will allow the developer to speed up initialization by reducing string scanning and it can also
	/// provide a way to avoid attempts to localize areas which crash the mono reflection implementation.
	///</remarks>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class NoLocalizableStringsPresent : Attribute
	{
		public OS DoNotLocalizeOn { get; set; }
		public NoLocalizableStringsPresent(OS operatingSystem)
		{
			DoNotLocalizeOn = operatingSystem;
		}

		public NoLocalizableStringsPresent() : this(OS.All) { }

		public enum OS
		{
			All = 0,
			Windows = 1,
			Linux = 2,
			Mac = 3
		};
	}
}
