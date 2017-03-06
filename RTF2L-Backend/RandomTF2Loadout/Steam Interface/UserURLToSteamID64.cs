using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using RandomTF2Loadout.General;

namespace RandomTF2Loadout.Steam_Interface
{
	#region Json Objects
	public class Response
	{
		public string steamid { get; set; }
		public int success { get; set; }
	}

	public class SteamID64RootObject
	{
		public Response response { get; set; }
	}
	#endregion

	class UserURLToSteamID64
	{
		const string failureResult = null;

		static bool tryGetRoot(string steamUrl, out string result)
		{
			try
			{
				//Checks if it's an url
				SteamID64RootObject steamid = JsonConvert.DeserializeObject<SteamID64RootObject>(htmlDownloader.downloadHTML("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&format=json&vanityurl={1}", SteamKEY.key, steamUrl));
				if(steamid.response.success != 1)
				{
					result = failureResult;
					return false;
				}else
				{
					result = steamid.response.steamid;
					return true;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				result = failureResult;
				return false;
			}
		}
		public static bool tryParseSteamID64(string steamUrl, out string result)
		{
			//Constants used in this function
			const string urlPattern = @"http:\/\/steamcommunity\.com\/";
			const string steam64Pattern = @"7656119\d{10}";
			const string customUrlPattern = @"http:\/\/steamcommunity\.com\/id\/([\w\d]+)";
			const string defaultUrlPattern = @"http:\/\/steamcommunity\.com\/profiles\/(7656119\d{10})";

			//Tests if its a url
			if(RegexFunctions.matches(steamUrl, urlPattern))
			{
				Match match;

				//The default format has the answers
				if(RegexFunctions.matches(steamUrl, defaultUrlPattern, out match))
				{
					result = match.Groups[1].Value;
					return true;
				}
				//The url is a special one
				else if(RegexFunctions.matches(steamUrl, customUrlPattern, out match))
				{
					return tryGetRoot(match.Groups[1].Value, out result);
				}
				else
				{
					result = failureResult;
					return false;
				}
			}
			else
			{
				//It also checks if the length is 17 becasue pure number id's are possible
				if(RegexFunctions.matches(steamUrl, steam64Pattern) && steamUrl.Length == 17)
				{
					result = steamUrl;
					return true;
				}
				else
				{
					return tryGetRoot(steamUrl, out result);
				}
			}
		}
		
		public static string parseSteamID64(string steamUrl)
		{
			string result;
			if(tryParseSteamID64(steamUrl, out result))
			{
				return result;
			}else
			{
				throw new FormatException("The url could not be parsed!");
			}
		}
	}
}
