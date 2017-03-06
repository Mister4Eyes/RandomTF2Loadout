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
	public class Asset
	{
		public string appid { get; set; }
		public string contextid { get; set; }
		public string assetid { get; set; }
		public string classid { get; set; }
		public string instanceid { get; set; }
		public string amount { get; set; }
	}

	public class Description2
	{
		public string type { get; set; }
		public string value { get; set; }
		public string color { get; set; }
	}

	public class Action
	{
		public string link { get; set; }
		public string name { get; set; }
	}

	public class Tag
	{
		public string category { get; set; }
		public string internal_name { get; set; }
		public string localized_category_name { get; set; }
		public string localized_tag_name { get; set; }
		public string color { get; set; }
	}

	public class MarketAction
	{
		public string link { get; set; }
		public string name { get; set; }
	}

	public class Description
	{
		public int appid { get; set; }
		public string classid { get; set; }
		public string instanceid { get; set; }
		public int currency { get; set; }
		public string background_color { get; set; }
		public string icon_url { get; set; }
		public string icon_url_large { get; set; }
		public List<Description2> descriptions { get; set; }
		public int tradable { get; set; }
		public List<Action> actions { get; set; }
		public string name { get; set; }
		public string name_color { get; set; }
		public string type { get; set; }
		public string market_name { get; set; }
		public string market_hash_name { get; set; }
		public int commodity { get; set; }
		public int market_tradable_restriction { get; set; }
		public int market_marketable_restriction { get; set; }
		public int marketable { get; set; }
		public List<Tag> tags { get; set; }
		public string item_expiration { get; set; }
		public List<MarketAction> market_actions { get; set; }
		public List<string> fraudwarnings { get; set; }
	}

	public class InventoryRootObject
	{
		public List<Asset> assets { get; set; }
		public List<Description> descriptions { get; set; }
		public int total_inventory_count { get; set; }
		public int success { get; set; }
		public int rwgrsn { get; set; }
	}
	#endregion

	class SteamInventory
	{
		public static bool TryGetInventory(string steamID64, out InventoryRootObject iro)
		{
			const string inventoryPattern = @"http://steamcommunity.com/inventory/{0}/440/2?l=english&count=5000";

			//I feel like there are some hidden exceptions I don't know about.
			try
			{
				iro = JsonConvert.DeserializeObject<InventoryRootObject>(htmlDownloader.downloadHTML(inventoryPattern, steamID64));

				if (iro.success == 1)
				{
					return true;
				}
			}
			catch (Exception) { }

			iro = null;
			return false;
		}

		public static InventoryRootObject GetInventory(string steamID64)
		{
			InventoryRootObject iro;
			if(TryGetInventory(steamID64, out iro))
			{
				return iro;
			}
			throw new FormatException("Bad steamID64");
		}

	}
}
