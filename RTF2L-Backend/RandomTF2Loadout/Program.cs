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
using RandomTF2Loadout.General;

namespace RandomTF2Loadout
{
	class Program
	{
		static void Main(string[] args) => new Program().Start();
		bool DevMode = false;
        bool running = false;
		string BaseDirectory;
		string ClientDirectory;
		Random r = new Random();
        List<Session> sessions = new List<Session>();

		Dictionary<string, List<Item>> generalClassItems = GeneralFunctions.InitializeDictonary();

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
                case "PIPEBOMBLAUNCHER":
                    return "Stickybomb Launcher";

                default:
					return rep;
			}
		}
		//Seperates weapons from classes
		public List<Item> getClassItems(string pickedClass, Dictionary<string, List<Item>> classItems)
		{
			List<Item> classItm = new List<Item>();
			foreach(Item itm in classItems[pickedClass])
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

		public string ItemView(string str, string pickedClass, Session session)
        {
            Dictionary<string, List<Item>> classItems;
            if (session == null)
            {
                classItems = generalClassItems;
            }
            else
            {
                session.UpdateTask.Wait();
                classItems = session.sessionClassItems;
            }
            List<Item> picked = getClassItems(pickedClass, classItems);
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

		public string FormatItemView(string str, string pickedClass, Session session)
		{
			try
			{
				string imgDirectory = string.Format("img/{0}.png", pickedClass);
				string fileText = File.ReadAllText(string.Format(@"{0}Moduels\ItemView.html", ClientDirectory)).Replace("{","{{").Replace("}","}}").Replace("{{0}}","{0}").Replace("{{1}}","{1}");
				return string.Format(fileText, imgDirectory, pickedClass);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return "";
			}
		}

        public string parseSwitches(string site, Session session)
        {
            //Short for Long Ass Pattern
            const string LAP = @"#if\s+([\w\d-]+)\s*?\n([\w\W]*?)(?:#else\s*?\n([\w\W]*?))#endif";

            //Matches the long ass pattern to the text
            MatchCollection matches = Regex.Matches(site, LAP);

            /*
             *LAP has 3 groups
             * 1 This is the parameter to check..
             * 2 This is the first resultant text.
             * 3 (Optional) This is the optional failure text.
             */
            foreach(Match match in matches)
            {
                bool success;
                string sucString = match.Groups[2].Value;
                string falString = (match.Groups[3].Success) ? match.Groups[3].Value : string.Empty;

                //Varius ways of initializing success
                switch (match.Groups[1].Value.Trim())
                {
                    case "HasSession":
                        success = session != null;
                        break;
                    //Defaults to failure
                    default:
                        success = false;
                        break;
                }
                //Replaces statement with correct string and overrites old string
                site = site.Replace(
                    match.Value,
                    (success) ? sucString : falString);
            }

            return site;
        }

		public string FormatWebpage(string str, Session session)
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
			return FormatWebpage(str, selectClass, session);
		}

        public string FormatWebpage(string str, string selectClass, Session session)
        {
               DirectoryInfo moduelsDirectory = new DirectoryInfo(string.Format("{0}moduels", ClientDirectory));
            List<string> staticNames = new List<string>();

            str = parseSwitches(str, session);
            foreach(FileInfo fi in moduelsDirectory.EnumerateFiles())
            {
                staticNames.Add(fi.Name.Replace(".html", ""));
            }

            //Static custom functions
            Dictionary<string, Func<string, string, Session, string>> dict = new Dictionary<string, Func<string, string, Session, string>>()
			{
				{ "primary",	ItemView		},
				{ "secondary",	ItemView		},
				{ "melee",		ItemView		},
				{ "pda",		ItemView		},
				{ "pda2",		ItemView		},
				{ "building",	ItemView		},
				{ "ItemView",	FormatItemView	}
			};

            //Removes proper things for the classes
            switch (selectClass)
			{
				case "Engineer":
					dict.Remove("pda2");
					dict.Remove("building");
                    str = str.Replace("{pda2}", "").Replace("{building}", "");
					break;
				case "Spy":
					dict.Remove("pda");
                    str = str.Replace("{pda}", "");
					break;
			}
            List<FormatKeyPair> keyPairs = new List<FormatKeyPair>();
            const string pattern = @"{([\w\d-]+)(?::[\w\d-]+)?}";

            //Searches for the moduels
            MatchCollection mc = Regex.Matches(str, pattern);
            foreach(Match m in mc)
            {
                string name = m.Groups[1].Value;

                //Adds in static custom functions
                if (dict.ContainsKey(name))
                {
                    keyPairs.Add(new FormatKeyPair(name, FormatWebpage(dict[name](name, selectClass, session), selectClass, session)));
                }
                //Adds in static directory
                else if (staticNames.Contains(name))
                {
                    keyPairs.Add(new FormatKeyPair(name, FormatWebpage(File.ReadAllText(string.Format(@"{0}\{1}.html", moduelsDirectory.FullName, name)), session)));
                }
            }
            
            return AdvancedFormat.Format(str, session, keyPairs.ToArray());
		}

		public bool TryGetModuel(string path, out string data, Session session)
		{
			DirectoryInfo moduelsDirectory = new DirectoryInfo(string.Format("{0}moduels", ClientDirectory));
            
            //Checks if there is a moduel directory
            if (moduelsDirectory.Exists)
			{
				FileInfo moduelFile = new FileInfo(string.Format(@"{0}Moduels\{1}.html", ClientDirectory, path));

				if (moduelFile.Exists)
				{
					data = FormatWebpage(File.ReadAllText(moduelFile.FullName), session);
					return true;
				}
			}

			data = null;
			return false;
		}

		public bool TryGetStatic(HttpListenerContext hlc, Session sesh, out byte[] data)
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
						data = Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(urlFile.FullName), sesh));
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

		public byte[] BaseSite(HttpListenerContext hlc, Session sesh)
		{
			FileInfo fzf = new FileInfo(string.Format(@"{0}base.html", ClientDirectory));
			hlc.Response.StatusCode = 200;
			hlc.Response.ContentType = "text/html";
			hlc.Response.ContentEncoding = Encoding.UTF8;

			if (fzf.Exists)
			{
				return Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(fzf.FullName), sesh));
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

			//Checks for 404 file. If not found then it gives a 500 for the 404
			if (fzf.Exists)
			{
				return Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(fzf.FullName), null));
			}
            hlc.Response.StatusCode = 500;
			string none = "<head><title>500</title></head><body>The 404 does not exist.</body>";
			return Encoding.UTF8.GetBytes(none);
		}

        public byte[] HttpFunctionGET(string url, HttpListenerContext hlc, Session sesh)
        {
            //Uri controller
            switch (url)
            {
                case "/":
                    return BaseSite(hlc, sesh);

                case "/PostTag":


                //Defaults to static data
                default:
                    byte[] data;
                    string modDat;
                    if (TryGetStatic(hlc, sesh, out data))
                    {
                        return data;
                    }
                    else if (DevMode && TryGetModuel(url, out modDat, sesh))
                    {
                        return Encoding.UTF8.GetBytes(modDat);
                    }
                    else
                    {
                        return FourZeroFour(hlc);
                    }
            }
        }

        public void PostHandler(HttpListenerContext hlc, PostData pd, ref Session session)
        {
            for(int i = 0; i < pd.Length; ++i)
            {
                switch (pd[i])
                {
                    case "SteamID":
                        string steam64;
                        if (UserURLToSteamID64.TryParseSteamID64(pd["SteamID"], out steam64))
                        {
                            lock (sessions)
                            {
                                if(session == null)
                                {
                                    Session sesh = new Session(hlc.Request.RemoteEndPoint.Address);
                                    if(sesh.TrySetSteamID64(steam64))
                                    {
                                        sessions.Add(sesh);
                                        session = sessions[sessions.Count - 1];
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

		public byte[] HttpFunction(HttpListenerContext hlc)
		{
			string url = hlc.Request.Url.AbsolutePath;
			Console.WriteLine("{0}\t{1}", url, hlc.Request.HttpMethod);

            Session currSession = null;
            lock (sessions)
            {
                for(int i = 0; i < sessions.Count; ++i)
                {
                    if (sessions[i].isSession(hlc.Request.RemoteEndPoint.Address))
                    {
                        sessions[i].Accessed();
                        currSession = sessions[i];
                        break;
                    }
                }
            }

            switch (hlc.Request.HttpMethod)
            {
                case "GET":
                    return HttpFunctionGET(url, hlc, currSession);

                case "POST":
                    Console.WriteLine("--==  POST DATA  ==--");

                    string text;

                    //Reads post data from 
                    using (var reader = new StreamReader(hlc.Request.InputStream, hlc.Request.ContentEncoding))
                    {
                        text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }

                    Console.WriteLine("--==END POST DATA==--");

                    PostHandler(hlc, new PostData(text), ref currSession);
                    return HttpFunctionGET(url, hlc, currSession);

                default:
                    return FourZeroFour(hlc);
            }

		}
		/*
		public void Start()
		{
			string customSteamName = "Mister_4_Eyes";
            Session session = new Session(IPAddress.Any);
            
            if (session.TrySetSteamID64(customSteamName))
            {
                Console.WriteLine("Steam id set successfully.");
                session.UpdateTask.Wait();
                foreach (string classKey in session.sessionClassItems.Keys)
                {
                    Console.WriteLine(classKey);
                    foreach(Item item in session.sessionClassItems[classKey])
                    {
                        Console.WriteLine("\t{0}", getWeponName(item.name));
                    }
                }
            }

			Console.ReadKey(true);
		}
		//*/
		//*
        public void UpdateSessions()
        {
            while (running)
            {
                //Setting "current time" to a static varible so all of the updates technically happen at the same time.
                DateTime CurrTime = DateTime.UtcNow;
                
                lock (sessions)
                {
                    for(int i = 0; i < sessions.Count; ++i)
                    {
                        //Checks if session has expired
                        if((CurrTime-sessions[i].lastAccessed).TotalHours < 1)
                        {
                            //Session has not expired. Checking if inventory update is needed
                            if ((CurrTime - sessions[i].lastUpdate).TotalMinutes >= 10)
                            {
                                sessions[i].updateInventory();
                            }
                        }
                        else
                        {
                            //Session has expired. Removing session.
                            sessions.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }

		public void Start()
		{
			bool.TryParse(GeneralFunctions.ParseConfigFile("DevMode"), out DevMode);

			BaseDirectory = GeneralFunctions.getBaseDirectory();
			ClientDirectory = GeneralFunctions.getClientDirectory();
			if (DevMode)
			{
				Console.WriteLine("Development mode engaged.");
			}

			Console.WriteLine("Getting base weapons.");
			foreach(Item i in WeaponGather.RemoveReskins(WeaponGather.getWeapons()))
			{
				foreach(string str in i.used_by_classes)
                {
                    Item changeITM = i;
                    if (changeITM.name.Equals("The B.A.S.E. Jumper") && str.Equals("Demoman"))
                    {
                        changeITM.item_slot = "primary";
                    }
                    generalClassItems[str].Add(i);
				}
			}

			WebServer.WebServer ws = new WebServer.WebServer(GeneralFunctions.ParseConfigFile("URIs").Split(','), HttpFunction);

            //Startup
			ws.Run();
            running = true;

            //Start session updates
            Task upSess = new Task(UpdateSessions);
            upSess.Start();

            //User kill
            Console.WriteLine("Press any key to stop.");
			Console.ReadKey(true);

            //Stop
			ws.Stop();
            running = false;

            //Wait for this to stop
            upSess.Wait();
		}
		//*/
	}
}
