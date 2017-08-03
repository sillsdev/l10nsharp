using System;
using System.IO;
using System.Xml;

namespace XliffToHtml
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: XliffToHtml xlifffile");
				return;
			}
			var infile = args[0];
			var outfile = Path.ChangeExtension(infile, "html");

			var xliffDoc = new XmlDocument();
			xliffDoc.Load(infile);
			var converter = new XliffToHtmlConverter(xliffDoc);
			var htmlDoc = converter.Convert();
			htmlDoc.Save(outfile);
		}
	}
}
