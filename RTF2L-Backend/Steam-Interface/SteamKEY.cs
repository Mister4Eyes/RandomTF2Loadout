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
				try
				{
					return File.ReadAllText(General.GeneralFunctions.ParseConfigFile("SteamKeyLocation"));
				}
				catch (Exception)
				{
					Console.WriteLine("Error, couldn't find steam key location.");
					return null;
				}
			}
		}
	}
}
