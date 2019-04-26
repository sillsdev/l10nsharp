// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
