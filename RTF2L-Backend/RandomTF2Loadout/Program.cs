using HtmlAgilityPack;
using RandomTF2Loadout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RandomTF2Loadout.WebServer;
using System.Net;
using MimeTypes;
using RandomTF2Loadout.Steam_Interface;
using System.Threading;
using System.Text.RegularExpressions;

namespace RandomTF2Loadout
{
	class Program
	{
		static void Main(string[] args) => new Program().Start();
		bool DevMode = false;
		string BaseDirectory;
		string ClientDirectory;
		Random r = new Random();

		Dictionary<string, List<Item>> generalClassItems = InitializeDictonary();

		public static Dictionary<string, List<Item>> InitializeDictonary()
		{
			return new Dictionary<string, List<Item>>()
		{
			{"Scout",	new List<Item>() },
			{"Soldier",	new List<Item>() },
			{"Pyro",	new List<Item>() },
			{"Demoman",	new List<Item>() },
			{"Heavy",	new List<Item>() },
			{"Engineer",new List<Item>() },
			{"Medic",	new List<Item>() },
			{"Sniper",	new List<Item>() },
			{"Spy",		new List<Item>() }
		};

		}
		public string getWeponName(string str)
		{
			string rep = str.Replace("TF_WEAPON_", "");

			switch (rep)
			{
				//Special cases where formatting is off
				case "PDA_SPY":
					return "PDA";
				case "GRENADELAUNCHER":
					return "GRENADE LAUNCHER";
				case "SHOTGUN_HWG":
					return "SHOTGUN";
				case "INVIS":
					return "INVIS WATCH";
				case "PDA_ENGINEER_BUILD":
					return "PDA";
				case "BUILDER_SPY":
					return "SAPPER";
				case "SHOTGUN_PRIMARY":
					return "SHOTGUN";
				case "PISTOL_SCOUT":
					return "PISTOL";
				case "SYRINGEGUN_MEDIC":
					return "SYRINGE GUN";
				case "SNIPERRIFLE":
					return "SNIPER RIFLE";
				case "Panic Attack Shotgun":
					return "Panic Attack";
				case "CLUB":
					return "KUKRI";
				case "Stickybomb Jumper":
					return "The Sticky Jumper";
				case "SHOTGUN_SOLDIER":
					return "SHOTGUN";
				default:
					return rep;
			}
		}
		//Seperates weapons from classes
		public List<Item> getClassItems(string pickedClass)
		{
			List<Item> classItm = new List<Item>();
			foreach(Item itm in generalClassItems[pickedClass])
			{
				//Loops through classes
				foreach(string usedClass in itm.used_by_classes)
				{
					if (usedClass.Equals(pickedClass))
					{
						classItm.Add(itm);
						break;
					}
				}
			}

			return classItm;
		}

		public string ItemView(string str, string pickedClass)
		{
			List<Item> picked = getClassItems(pickedClass);
			List<Item> prune = new List<Item>();
			foreach (Item itm in picked)
			{
				if (itm.item_slot.Equals(str))
				{
					prune.Add(itm);
				}
			}
			if(prune.Count == 0)
			{
				return "";
			}
			//Gets random pruned item
			Item select = prune[r.Next(prune.Count)];
			return string.Format(File.ReadAllText(string.Format(@"{0}Moduels\Item.html", ClientDirectory)), select.image_url, getWeponName(select.name));
		}

		public string FormatItemView(string str, string pickedClass)
		{
			try
			{
				string imgDirectory = string.Format("img/{0}.png", pickedClass);
				string fileText = File.ReadAllText(string.Format(@"{0}Moduels\ItemView.html", ClientDirectory)).Replace("{","{{").Replace("}","}}").Replace("{{0}}","{0}").Replace("{{1}}","{1}");
				return FormatWebpage(string.Format(fileText, imgDirectory, pickedClass).Replace("{{","{").Replace("}}","}"), pickedClass);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return "";
			}
		}
		public string FormatWebpage(string str)
		{

			string[] classes = new string[]
			{
					"Scout"		,
					"Soldier"	,
					"Pyro"		,
					"Demoman"	,
					"Heavy"		,
					"Engineer"	,
					"Medic"		,
					"Sniper"	,
					"Spy"
			};

			string selectClass = classes[r.Next(classes.Length)];
			return FormatWebpage(str, selectClass);
		}
		public string FormatWebpage(string str, string selectClass)
		{
			//Doubles up the {} so the formatter dosen't get confused
			str = str.Replace("{", "{{").Replace("}", "}}");

			{
				Dictionary<string, Func<string, string, string>> dict = new Dictionary<string, Func<string, string, string>>()
				{
					{ "primary",	ItemView		},
					{ "secondary",	ItemView		},
					{ "melee",		ItemView		},
					{ "pda",		ItemView		},
					{ "pda2",		ItemView		},
					{ "building",	ItemView		},
					{"ItemView",	FormatItemView	}
				};
				switch (selectClass)
				{
					case "Engineer":
						dict.Remove("pda2");
						dict.Remove("building");
						break;
					case "Spy":
						dict.Remove("pda");
						break;
				}
				foreach (string code in dict.Keys)
				{
					//Checks if the code is there
					if (str.Contains("{{" + code + "}}"))
					{
						str = str.Replace("{{" + code + "}}", "{0}");
						str = string.Format(str, dict[code](code, selectClass)).Replace("{", "{{").Replace("}", "}}");
					}
				}

				Match staticFormat = Regex.Match(str, @"{{([\w\d-]+)}}");

				//Goes through any static moduels
				while (staticFormat.Success)
				{
					string data;
					if (TryGetModuel(staticFormat.Groups[1].Value, out data))
					{
						str = str.Replace(staticFormat.Value, "{0}");
						str = string.Format(str, FormatWebpage(data, selectClass).Replace("}", "}}").Replace("{", "{{"));
					}

					staticFormat = staticFormat.NextMatch();
				}
			}

			//Returns them to their origional form
			str = str.Replace("{{", "{").Replace("}}", "}");
			return str;
		}

		public bool TryGetModuel(string path, out string data)
		{
			DirectoryInfo moduelsDirectory = new DirectoryInfo(string.Format("{0}moduels", ClientDirectory));

			//Checks if there is a moduel directory
			if (moduelsDirectory.Exists)
			{
				FileInfo moduelFile = new FileInfo(string.Format(@"{0}Moduels\{1}.html", ClientDirectory, path));

				if (moduelFile.Exists)
				{
					data = FormatWebpage(File.ReadAllText(moduelFile.FullName));
					return true;
				}
			}

			data = null;
			return false;
		}

		public bool TryGetStatic(HttpListenerContext hlc, out byte[] data)
		{
			DirectoryInfo staticDirectory = new DirectoryInfo(ClientDirectory + @"static");

			//Checks if there is a static directory
			if (staticDirectory.Exists)
			{
				FileInfo urlFile = new FileInfo(string.Format(@"{0}static{1}", ClientDirectory, hlc.Request.Url.AbsolutePath));

				//Checks if file exists
				if (urlFile.Exists)
				{
					hlc.Response.StatusCode = 200;
					hlc.Response.ContentType = MimeTypeMap.GetMimeType(urlFile.Extension);
					hlc.Response.ContentEncoding = Encoding.UTF8;

					if (urlFile.Extension.Equals(".html"))
					{
						data = Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(urlFile.FullName)));
					}
					else
					{
						data = File.ReadAllBytes(urlFile.FullName);
					}
					return true;
				}
			}

			data = null;
			return false;
		}

		public byte[] BaseSite(HttpListenerContext hlc)
		{
			FileInfo fzf = new FileInfo(string.Format(@"{0}base.html", ClientDirectory));
			hlc.Response.StatusCode = 200;
			hlc.Response.ContentType = "text/html";
			hlc.Response.ContentEncoding = Encoding.UTF8;

			if (fzf.Exists)
			{
				return Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(fzf.FullName)));
			}
			return FourZeroFour(hlc);
		}

		public byte[] FourZeroFour(HttpListenerContext hlc)
		{
			FileInfo fzf = new FileInfo(string.Format(@"{0}404.html", ClientDirectory));

			//The list of response codes
			hlc.Response.StatusCode = 404;
			hlc.Response.ContentType = "text/html";
			hlc.Response.ContentEncoding = Encoding.UTF8;

			//Checks for 404 file. If not found then it gives a 404 for the 404 (meta)
			if (fzf.Exists)
			{
				return File.ReadAllBytes(fzf.FullName);
			}
			string none = "<head><title>404</title></head><body>The 404 could not be found.</body>";
			return Encoding.UTF8.GetBytes(none);
		}

		public byte[] HttpFunction(HttpListenerContext hlc)
		{
			string url = hlc.Request.Url.AbsolutePath;
			Console.WriteLine(url);

			//Uri controller
			switch (url)
			{
				case "/":
					return BaseSite(hlc);

				case "/PostTag":
					

					//Defaults to static data
				default:
					byte[] data;
					string modDat;
					if(TryGetStatic(hlc, out data))
					{
						return data;
					}
					else if(DevMode && TryGetModuel(url, out modDat))
					{
						return Encoding.UTF8.GetBytes(modDat);
					}
					else
					{
						return FourZeroFour(hlc);
					}
			}
		}
		/*
		public void Start()
		{
			string customSteamName = "Mister_4_Eyes";
			string steamID64 = UserURLToSteamID64.parseSteamID64(customSteamName);
			Item[] items = WeaponGather.RemoveReskins(WeaponGather.getWeapons());

			List<string> bla = new List<string>();
			foreach(Item i in items)
			{
				if (!bla.Contains(i.item_slot))
				{
					bla.Add(i.item_slot);
				}
			}
			foreach(string str in bla)
			{
				Console.WriteLine(str);
			}
			Console.WriteLine("Total length:{0}",items.Length);
			Console.ReadKey(true);
		}
		//*/
		//*
		public void Start()
		{
			bool.TryParse(General.GeneralFunctions.ParseConfigFile("DevMode"), out DevMode);

			BaseDirectory = General.GeneralFunctions.getBaseDirectory();
			ClientDirectory = General.GeneralFunctions.getClientDirectory();
			if (DevMode)
			{
				Console.WriteLine("Development mode engaged.");
			}

			Console.WriteLine("Getting base weapons.");
			foreach(Item i in WeaponGather.RemoveReskins(WeaponGather.getWeapons()))
			{
				foreach(string str in i.used_by_classes)
				{
					generalClassItems[str].Add(i);
				}
			}

			WebServer.WebServer ws = new WebServer.WebServer(new[] { "http://localhost:9090/" }, HttpFunction);
			ws.Run();
			Console.WriteLine("Press any key to stop.");

			Console.ReadKey(true);
			ws.Stop();
		}
		//*/
	}
}
