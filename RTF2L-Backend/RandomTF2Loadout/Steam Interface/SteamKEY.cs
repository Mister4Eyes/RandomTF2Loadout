using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomTF2Loadout.Steam_Interface
{
	class SteamKEY
	{
		public static string key
		{
			get
			{
				if (File.Exists(string.Format(@"{0}\config.cfg",General.GeneralFunctions.getBaseDirectory())))
				{
					//TODO: make this shit work.
					return "fuck off faggot";
				}
				else
				{
					return null;
				}
			}
		}
	}
}
