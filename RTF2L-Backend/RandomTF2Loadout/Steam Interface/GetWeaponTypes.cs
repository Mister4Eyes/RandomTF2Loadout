using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RandomTF2Loadout.Steam_Interface
{
    #region Json Objects
    public class Qualities
    {
        public int Normal { get; set; }
        public int rarity1 { get; set; }
        public int rarity2 { get; set; }
        public int vintage { get; set; }
        public int rarity3 { get; set; }
        public int rarity4 { get; set; }
        public int Unique { get; set; }
        public int community { get; set; }
        public int developer { get; set; }
        public int selfmade { get; set; }
        public int customized { get; set; }
        public int strange { get; set; }
        public int completed { get; set; }
        public int haunted { get; set; }
        public int collectors { get; set; }
        public int paintkitweapon { get; set; }
    }

    public class OriginName
    {
        public int origin { get; set; }
        public string name { get; set; }
    }

    public class Capabilities
    {
        public bool nameable { get; set; }
        public bool can_gift_wrap { get; set; }
        public bool can_craft_mark { get; set; }
        public bool can_be_restored { get; set; }
        public bool strange_parts { get; set; }
        public bool can_card_upgrade { get; set; }
        public bool can_strangify { get; set; }
        public bool can_killstreakify { get; set; }
        public bool can_consume { get; set; }
        public bool? can_collect { get; set; }
        public bool? paintable { get; set; }
        public bool? can_craft_if_purchased { get; set; }
        public bool? can_craft_count { get; set; }
        public bool? can_unusualify { get; set; }
        public bool? usable_gc { get; set; }
        public bool? usable { get; set; }
        public bool? can_customize_texture { get; set; }
        public bool? usable_out_of_game { get; set; }
        public bool? can_spell_page { get; set; }
        public bool? duck_upgradable { get; set; }
        public bool? decodable { get; set; }
    }

    public class AdditionalHiddenBodygroups
    {
        public int hat { get; set; }
        public int headphones { get; set; }
        public int? head { get; set; }
        public int? grenades { get; set; }
    }

    public class Style
    {
        public string name { get; set; }
        public AdditionalHiddenBodygroups additional_hidden_bodygroups { get; set; }
    }

    public class Attribute
    {
        public string name { get; set; }
        public string @class { get; set; }
        public double value { get; set; }
    }

    public class PerClassLoadoutSlots
    {
        public string Soldier { get; set; }
        public string Heavy { get; set; }
        public string Pyro { get; set; }
        public string Engineer { get; set; }
        public string Demoman { get; set; }
    }

    public class UsageCapabilities
    {
        public bool nameable { get; set; }
        public bool? decodable { get; set; }
        public bool? paintable { get; set; }
        public bool? can_customize_texture { get; set; }
        public bool? can_gift_wrap { get; set; }
        public bool? paintable_team_colors { get; set; }
        public bool? can_strangify { get; set; }
        public bool? can_killstreakify { get; set; }
        public bool? duck_upgradable { get; set; }
        public bool? strange_parts { get; set; }
        public bool? can_card_upgrade { get; set; }
        public bool? can_spell_page { get; set; }
        public bool? can_unusualify { get; set; }
        public bool? can_consume { get; set; }
    }

    public class Tool
    {
        public string type { get; set; }
        public UsageCapabilities usage_capabilities { get; set; }
        public string use_string { get; set; }
        public string restriction { get; set; }
    }

    [Serializable]
    public class Item 
    {
        public Item() { }
        public Item(Item item)
        {
            name                    = item.name;
            defindex                = item.defindex;
            item_class              = item.item_class;
            item_type_name          = item.item_type_name;
            item_name               = item.item_name;
            proper_name             = item.proper_name;
            item_slot               = item.item_slot;
            model_player            = item.model_player;
            item_quality            = item.item_quality;
            image_inventory         = item.image_inventory;
            min_ilevel              = item.min_ilevel;
            max_ilevel              = item.max_ilevel;
            image_url               = item.image_url;
            image_url_large         = item.image_url_large;
            craft_class             = item.craft_class;
            craft_material_type     = item.craft_material_type;
            capabilities            = item.capabilities;
            used_by_classes         = item.used_by_classes;
            item_description        = item.item_description;
            styles                  = item.styles;
            attributes              = item.attributes;
            drop_type               = item.drop_type;
            item_set                = item.item_set;
            holiday_restriction     = item.holiday_restriction;
            per_class_loadout_slots = item.per_class_loadout_slots;
            tool                    = item.tool;
        }
        public string name { get; set; }
        public int defindex { get; set; }
        public string item_class { get; set; }
        public string item_type_name { get; set; }
        public string item_name { get; set; }
        public bool proper_name { get; set; }
        public string item_slot { get; set; }
        public string model_player { get; set; }
        public int item_quality { get; set; }
        public string image_inventory { get; set; }
        public int min_ilevel { get; set; }
        public int max_ilevel { get; set; }
        public string image_url { get; set; }
        public string image_url_large { get; set; }
        public string craft_class { get; set; }
        public string craft_material_type { get; set; }
        public Capabilities capabilities { get; set; }
        public List<object> used_by_classes { get; set; }
        public string item_description { get; set; }
        public List<Style> styles { get; set; }
        public List<Attribute> attributes { get; set; }
        public string drop_type { get; set; }
        public string item_set { get; set; }
        public string holiday_restriction { get; set; }
        public PerClassLoadoutSlots per_class_loadout_slots { get; set; }
        public Tool tool { get; set; }
        
    }

    public class Attribute2
    {
        public string name { get; set; }
        public int defindex { get; set; }
        public string attribute_class { get; set; }
        public string description_string { get; set; }
        public string description_format { get; set; }
        public string effect_type { get; set; }
        public bool hidden { get; set; }
        public bool stored_as_integer { get; set; }
    }

    public class Attribute3
    {
        public string name { get; set; }
        public string @class { get; set; }
        public double value { get; set; }
    }

    public class ItemSet
    {
        public string item_set { get; set; }
        public string name { get; set; }
        public List<string> items { get; set; }
        public List<Attribute3> attributes { get; set; }
        public string store_bundle { get; set; }
    }

    public class AttributeControlledAttachedParticle
    {
        public string system { get; set; }
        public int id { get; set; }
        public bool attach_to_rootbone { get; set; }
        public string name { get; set; }
        public string attachment { get; set; }
    }

    public class Level
    {
        public int level { get; set; }
        public int required_score { get; set; }
        public string name { get; set; }
    }

    public class ItemLevel
    {
        public string name { get; set; }
        public List<Level> levels { get; set; }
    }

    public class KillEaterScoreType
    {
        public int type { get; set; }
        public string type_name { get; set; }
        public string level_data { get; set; }
    }

    public class String
    {
        public int index { get; set; }
        public string @string { get; set; }
    }

    public class StringLookup
    {
        public string table_name { get; set; }
        public List<String> strings { get; set; }
    }

    public class Result
    {
        public int status { get; set; }
        public string items_game_url { get; set; }
        public Qualities qualities { get; set; }
        public List<OriginName> originNames { get; set; }
        public List<Item> items { get; set; }
        public List<Attribute2> attributes { get; set; }
        public List<ItemSet> item_sets { get; set; }
        public List<AttributeControlledAttachedParticle> attribute_controlled_attached_particles { get; set; }
        public List<ItemLevel> item_levels { get; set; }
        public List<KillEaterScoreType> kill_eater_score_types { get; set; }
        public List<StringLookup> string_lookups { get; set; }
    }

    public class RootObject
    {
        public Result result { get; set; }
    }
    #endregion

    class WeaponGather
	{
        public static Item[] getWeapons()
		{
			List<Item> weapons = new List<Item>();

			RootObject wtro = JsonConvert.DeserializeObject<RootObject>(General.htmlDownloader.downloadHTML("http://api.steampowered.com/IEconItems_440/GetSchema/v0001/?key={0}", SteamKEY.key));

			//Checks the craft material type. All weapons seems to be labled weapon.
			foreach(Item item in wtro.result.items)
			{
				if (item.item_slot == null || item.item_slot.Contains("action"))
				{
					continue;
				}
				if(item.used_by_classes == null)
				{
					continue;
				}
				if (item.craft_material_type != null && item.craft_material_type.Equals("weapon"))
                {
                    weapons.Add(item);
				}
			}

			return weapons.ToArray();
		}

		public static Item[] RemoveReskins(Item[] weapons)
		{
			int[] something = { 160, 161, 169, 221, 264, 266, 294, 297, 298, 423, 433, 452, 457, 466, 474, 482, 513, 572, 574, 587, 608, 609, 638, 727, 739, 741, 851, 863, 880, 933, 939, 947, 1013, 1092, 1100, 1102, 1105, 1121, 1123, 1127, 30474, 30667, 30666, 30668, 30665 };

			List<Item> pruned = new List<Item>();
			foreach(Item weapon in weapons)
			{
				if (something.Contains(weapon.defindex))
				{
					continue;
				}
				if(weapon.name.Contains("Festive") || weapon.name.Contains("Upgradeable") || weapon.name.Contains("Botkiller") || weapon.name.Contains("Promo") || weapon.name.Contains("Deflector"))
				{
					continue;
				}
				pruned.Add(weapon);
			}

			return pruned.ToArray();
		}
	}
}
