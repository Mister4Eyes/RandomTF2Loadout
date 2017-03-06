using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
	}
}
