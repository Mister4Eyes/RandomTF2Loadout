using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RandomTF2Loadout.General
{
	class GeneralFunctions
	{
		public static string getBaseDirectory()
		{
			FileInfo fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
			return fi.Directory.FullName;
		}
		public static string getClientDirectory()
		{
			return Path.GetFullPath(getBaseDirectory()+ParseConfigFile("ClientDirectory"));
		}

		public static string ParseConfigFile(string key)
		{
			string path = string.Format("{0}\\config.cfg", getBaseDirectory());
			Match m;
			if (RegexFunctions.matches(File.ReadAllText(path), string.Format(@"{0}=(.+)",key), out m))
			{
				try
				{
					return m.Groups[1].Value.Trim();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}
			}
			return null;
		}
	}
}
