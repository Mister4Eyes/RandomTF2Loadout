using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RandomTF2Loadout.WebServer;
using System.Net;
using MimeTypes;
using RandomTF2Loadout.Steam_Interface;
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

		List<int> ids = new List<int>();
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
				case "SHOTGUN_PYRO":
					return "SHOTGUN";
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
				if(session.inventoryPulled)
				{
					lock (session.sessionClassItems)
					{
						classItems = session.sessionClassItems;
					}
				}
				else
				{
					classItems = generalClassItems;
				}
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
			const string LAP = @"#if\s+([\w\d-]+)(?::([\w\d-]+))?\s*?\n([\w\W]*?)(?:#else\s*?\n([\w\W]*?))?#endif";
			
			MatchCollection matches = Regex.Matches(site, LAP);

			/*
			 * LAP has 4 groups
			 * 1 This is the parameter to check.
			 * 2 (Optional) Modifier for the parameter
			 * 3 This is the first resultant text.
			 * 4 (Optional) This is the optional failure text.
			 */
			foreach(Match match in matches)
			{
				bool success = false;
				string sucString = match.Groups[3].Value;
				string falString = (match.Groups[4].Success) ? match.Groups[4].Value : string.Empty;
				
				switch (match.Groups[1].Value)
				{
					case "Session":
						if(session != null && match.Groups[2].Success)
						{
							switch (match.Groups[2].Value)
							{
								case "Inventory-Pulled":
									success = session.inventoryPulled;
									break;
							}
						}
						break;
					case "Error":
						if(session != null && match.Groups[2].Success)
						{
							string testError = match.Groups[2].Value;

							//Checks if any of the errors have been seen in here.
							foreach(string error in session.errors)
							{
								if (error.Equals(testError))
								{
									session.clearErrors = true;//This flag lets some code near the beginning know that errors were successfully pulled and can be clensed.
									success = true;
									goto loopSuccess;
								}
							}
							success = false;
							loopSuccess:;
						}
						break;

					case "HasSession":
						success = session != null;
						break;

					//Defaults to failure
					default:
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
			string selectClass;
			if (session == null || session.SelectClass != Session.RANDOM)
			{
				selectClass = classes[r.Next(classes.Length)];
			}
			else
			{
				selectClass = classes[session.SelectClass];
			}
			return FormatWebpage(str, selectClass, session);
		}

		public string SessionInformation(string str, Session sesh)
		{
			switch (str)
			{
				case "Name":
					return sesh.playerName;
				case "SteamID64":
					return sesh.steamID64;
				case "SelectClass":
					return sesh.SelectClass.ToString();
				default:
					return "";
			}
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
			keyPairs.Add(new FormatKeyPair("SessionInfo", SessionInformation));

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
				WaitForItemPull(sesh);
				return Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(fzf.FullName), sesh));
			}
			return FourZeroFour(hlc);
		}

		public byte[] FourZeroFour(HttpListenerContext hlc)
		{
			hlc.Response.StatusCode = 404;
			hlc.Response.ContentType = "text/html";
			hlc.Response.ContentEncoding = Encoding.UTF8;

			FileInfo fzf = new FileInfo(string.Format(@"{0}404.html", ClientDirectory));
			if (fzf.Exists)
			{
				return Encoding.UTF8.GetBytes(FormatWebpage(File.ReadAllText(fzf.FullName), null));
			}

			//Looks like someone forgot to make a 404 for the website.
			hlc.Response.StatusCode = 500;
			const string none = "<head><title>500</title></head><body>The 404 does not exist.</body>";
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

		public void PostHandler(HttpListenerContext hlc, PostData pd, ref Session session, SessionId sid)
		{
			for(int i = 0; i < pd.Length; ++i)
			{
				switch (pd[i])
				{
					case "SteamID":
						{
							string steam64;
							if (UserURLToSteamID64.TryParseSteamID64(pd["SteamID"], out steam64))
							{
								lock (sessions)
								{
									if (session == null)
									{
										session = InitializeSession(sid, hlc.Request.Cookies);
										session.TrySetSteamID64(steam64);
									}
								}
							}
						}
						break;

					case "StopSession":
						{
							lock (sessions)
							{
								sessions.Remove(session);
								hlc.Response.Cookies["SteamID"].Value = "";

								//Just in case any more comes in requiring a session to be active.
								session = null;
							}
						}
						break;

					case "ChangeID":
						if(session != null)
						{
							string steam64;
							if (UserURLToSteamID64.TryParseSteamID64(pd["ChangeID"], out steam64))
							{
								session.TrySetSteamID64(steam64);
							}
						}
						break;

					case "ChangeClass":
						{
							if(session == null)
							{
								session = InitializeSession(sid, hlc.Request.Cookies);
							}

							string intString = pd["ChangeClass"];
							byte output;
							if (byte.TryParse(intString, out output))
							{
								if (0 <= output && 9 >= output)
								{
									session.SelectClass = output;
								}
							}
						}
						break;
				}
			}
		}

		private Session InitializeSession(SessionId sid, CookieCollection cc)
		{
			Session sesh = new Session(sid, cc);
			sessions.Add(sesh);
			lock (ids)
			{
				ids.Add(sesh.identification.GetHashCode());
			}

			return sesh;
		}

		public void DisplayCookies(CookieCollection cc)
		{
			if (DevMode)
			{
				Console.WriteLine("--==  Cookies  ==--");
				foreach (Cookie c in cc)
				{
					Console.WriteLine(c.ToString());
				}
				Console.WriteLine("--==End Cookies==--");
			}
		}

		public void WaitForItemPull(Session sesh)
		{
			if(sesh != null && sesh.UpdateTask != null)
			{
				sesh.UpdateTask.Wait();
			}
		}

		public byte[] HttpFunction(HttpListenerContext hlc)
		{
			string url = hlc.Request.Url.AbsolutePath;
			Console.WriteLine("{0}\t{1}", url, hlc.Request.HttpMethod);

			DisplayCookies(hlc.Request.Cookies);
			Session currSession = null;
			SessionId sid;

			lock (ids)
			{
				sid = new SessionId(hlc.Request.Cookies, ids.ToArray());
			}


			lock (sessions)
			{
				for(int i = 0; i < sessions.Count; ++i)
				{
					if (sessions[i].isSession(sid))
					{
						sessions[i].Accessed();
						currSession = sessions[i];
						break;
					}
				}
			}

			
			//This clear the errors after a successfull pulling of errors from another function.
			if(currSession != null && currSession.clearErrors)
			{
				currSession.clearErrors = false;
				currSession.errors.Clear();
			}

			if (currSession != null)
			{
				hlc.Response.Cookies = currSession.cookies;
			}
			else if(GeneralFunctions.CookieCollectionHasValue("SteamID", hlc.Request.Cookies))
			{
				currSession = new Session(sid, hlc.Request.Cookies);

				WaitForItemPull(currSession);

				//Sanity check due to how cookies can be edited.
				if (currSession.inventoryPulled)
				{
					lock (ids)
					{
						//Was checked earlier so there is no way there can be a null SID.
						ids.Add(sid.GetHashCode());
					}
					lock (sessions)
					{
						sessions.Add(currSession);
					}
				}
				else { currSession = null; }
			}
			switch (hlc.Request.HttpMethod)
			{
				case "GET":
					return HttpFunctionGET(url, hlc, currSession);

				case "POST":
					{
						if (DevMode)
						{
							Console.WriteLine("--==  POST DATA  ==--");
						}

						string text;

						//Reads post data from 
						using (var reader = new StreamReader(hlc.Request.InputStream, hlc.Request.ContentEncoding))
						{
							text = reader.ReadToEnd();

							if (DevMode)
							{
								Console.WriteLine(text);
							}
						}

						if (DevMode)
						{
							Console.WriteLine("--==END POST DATA==--");
						}

						PostHandler(hlc, new PostData(text), ref currSession, sid);
						hlc.Response.Redirect(hlc.Request.Url.AbsolutePath);

						if (currSession != null)
						{
							hlc.Response.Cookies = currSession.cookies;
						}
						return new byte[0];
					}

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
			DateTime CurrTime = DateTime.UtcNow;
			while (running)
			{
				TimeSpan ts = DateTime.UtcNow - CurrTime;
				//Waits for 10 minutes but can be cancelled at any time
				if ((DateTime.UtcNow - CurrTime).Minutes >= 10)
				{
					//Setting "current time" to a static varible so all of the updates technically happen at the same time.
					CurrTime = DateTime.UtcNow;
					Console.WriteLine("Updating.");
					lock (sessions)
					{
						for (int i = 0; i < sessions.Count; ++i)
						{
							//Checks if session has expired
							Console.WriteLine("{0}'s session was last access {1} hours ago.",sessions[i].playerName,(CurrTime - sessions[i].lastAccessed).TotalHours);

							if ((CurrTime - sessions[i].lastAccessed).Hours < 1)
							{
								//Session has not expired. Checking if inventory update is needed
								if ((CurrTime - sessions[i].lastUpdate).Minutes >= 10)
								{
									sessions[i].updateInventory();
								}
							}
							else
							{
								//Session has expired. Removing session.
								lock (ids)
								{
									ids.Remove(sessions[i].GetHashCode());
								}
								sessions.RemoveAt(i);
								--i;
							}
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
					GeneralFunctions.InitializeItems(i, generalClassItems);
				}
			}

			WebServer.WebServer ws = new WebServer.WebServer(GeneralFunctions.ParseConfigFile("URIs").Split(' '), HttpFunction, DevMode);

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

			//Wait for update thread to stop
			upSess.Wait();
		}
		//*/
	}
}
