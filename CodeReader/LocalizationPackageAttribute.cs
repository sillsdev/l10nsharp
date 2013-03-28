using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Localization.CodeReader
{
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class LocalizationPackageAttribute : System.Attribute
	{
		/// <summary>
		/// This gives LocalizationManager much what it needs to manage a tmx for your library or application.
		/// Place this attribute on exaclty one class per library or application. It doesn't matter which class.
		/// </summary>
		/// <param name="id">This is used as the name on the TMX file itself.</param>
		/// <param name="namespaceToSearch">The code scanner uses namespaces to know what to scan. Normally, you can omit this and we'll figure it out.</param>
		/// <param name="displayName">The name the user will see, if the program has to show the user where the string comes from.</param>
		/// <param name="version">REVIEW: I (jh) *think* this is used to keep from making some undesirable changes to an existing TMX when
		/// the user starts using an older version.</param>
		public LocalizationPackageAttribute(string id, string version="", string namespaceToSearch = "", string displayName = "")
		{
			_namespaceToSearch = namespaceToSearch;
			_displayName = displayName;
			ID = id;
			_version = version;
			if (string.IsNullOrEmpty(namespaceToSearch))
			{

			}
		}

		/// <summary>
		/// This is used as the name on the TMX file itself.
		/// </summary>
		public string ID { get; set; }


		private string _namespaceToSearch;
		private string _displayName;
		private string _version;

		public string GetNameSpace(Type type)
		{
			if(!string.IsNullOrEmpty(_namespaceToSearch))
				return _namespaceToSearch;

			//just use the primary part of the namespace
			return type.Namespace.Split(new char[] { '.' })[0];//return the first word in the namespace
		}

		public string GetDisplayName()
		{
			if (!string.IsNullOrEmpty(_displayName))
				return _displayName;
			return ID;

		}
		public string GetVersionName(Type type)
		{
			if (!string.IsNullOrEmpty(_version))
				return _version;
			return type.Assembly.FullName;

		}
	}
}
