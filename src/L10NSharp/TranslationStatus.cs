using System.Xml.Serialization;

namespace L10NSharp
{
	public enum TranslationStatus
	{
		[XmlEnum("yes")]
		Approved,
		[XmlEnum("no")]
		Unapproved
	}

}
