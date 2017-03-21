using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomTF2Loadout.General;
using Newtonsoft.Json;

namespace RandomTF2Loadout.Steam_Interface
{
	#region Json Objects
	public class AccountInfo
	{
		public object steamid { get; set; }
		public string personaname { get; set; }
	}

	public class PlayerAttribute
	{
		public int defindex { get; set; }
		public object value { get; set; }
		public double float_value { get; set; }
		public AccountInfo account_info { get; set; }
	}

	public class Equipped
	{
		public int @class { get; set; }
		public int slot { get; set; }
	}

	public class PlayerItem
	{
		public object id { get; set; }
		public object original_id { get; set; }
		public int defindex { get; set; }
		public int level { get; set; }
		public int quality { get; set; }
		public object inventory { get; set; }
		public int quantity { get; set; }
		public int origin { get; set; }
		public bool flag_cannot_trade { get; set; }
		public List<PlayerAttribute> attributes { get; set; }
		public bool? flag_cannot_craft { get; set; }
		public List<Equipped> equipped { get; set; }
		public int? style { get; set; }
		public string custom_name { get; set; }
		public string custom_desc { get; set; }
	}

	public class PlayerResult
	{
		public int status { get; set; }
		public int num_backpack_slots { get; set; }
		public List<PlayerItem> items { get; set; }
	}

	public class PlayerRootObject
	{
		public PlayerResult result { get; set; }
	}
	#endregion

	class SteamInventory
	{
		public static bool TryGetInventory(string steamID64, out PlayerRootObject iro)
		{
			const string inventoryPattern = @"http://api.steampowered.com/IEconItems_440/GetPlayerItems/v0001/?key={0}&SteamID={1}&count=2000";

			//I feel like there are some hidden exceptions I don't know about.
			try
			{
				string json = htmlDownloader.downloadHTML(inventoryPattern, SteamKEY.key, steamID64);

				iro = JsonConvert.DeserializeObject<PlayerRootObject>(json);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			iro = null;
			return false;
		}

		public static PlayerRootObject GetInventory(string steamID64)
		{
			PlayerRootObject iro;
			if(TryGetInventory(steamID64, out iro))
			{
				return iro;
			}
			throw new FormatException("Bad steamID64");
		}

	}
}
