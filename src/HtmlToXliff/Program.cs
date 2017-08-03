using System;
using System.IO;
using System.Text;
using HtmlAgilityPack;

namespace HtmlToXliff
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: HtmlToXliff htmlfile");
				return;
			}
			var infile = args[0];
			var outfile = Path.ChangeExtension(infile, "xlf");

			HtmlToXliffConverter.FixHtmlParserBug();	// call before loading any HtmlDocument!

			var htmlDoc = new HtmlDocument();
			htmlDoc.Load(infile, Encoding.UTF8);
			var converter = new HtmlToXliffConverter(htmlDoc, infile);
			var xliffDoc = converter.Convert();
			xliffDoc.Save(outfile);
		}
	}
}
