using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RandomTF2Loadout.General;
using RandomTF2Loadout.Steam_Interface;

namespace RandomTF2Loadout.WebServer
{
	class SessionId
	{
		int id = 0;
		public SessionId(IPAddress ip)
		{
			id = BitConverter.ToInt32(ipToBytes(ip), 0);
		}

		public SessionId(CookieCollection cc, int[] usedIDs = null)
		{
			int parsedID;
			if (cc["ID"] != null && int.TryParse(cc["ID"].Value, out parsedID))
			{
				id = parsedID;
			}
			else
			{
				if (usedIDs != null)
				{
					while (true)
					{
						id = new Random().Next();

						foreach(int tid in usedIDs)
						{
							if(tid == id)
							{
								continue;
							}
						}
						break;
					}
				}
			}
		}


		#region OperatorOverloads
		public static bool operator ==(SessionId sid1, SessionId sid2)
		{
			return sid1.GetHashCode() == sid2.GetHashCode();
		}

		public static bool operator !=(SessionId sid1, SessionId sid2)
		{
			return !(sid1 == sid2);
		}
		#endregion
		
		static byte[] ipToBytes(IPAddress ip)
		{

			using (MD5 md5 = MD5.Create())
			{
				//Here for storage. IP's are NOT stored in session and it allows for all ip's to have a fixed size.
				return md5.ComputeHash(ip.GetAddressBytes());
			}
		}

		public override int GetHashCode()
		{
			return id;
		}

		public override bool Equals(object obj)
		{
			if(obj is SessionId)
			{
				return (obj as SessionId).GetHashCode() == id;
			}
			else if (obj is Session)
			{
				return Equals((obj as Session).identification);
			}
			else
			{
				return false;
			}
		}
	}

	class Session
	{
		public const byte SCOUT		= 0;
		public const byte SOLDIER	= 1;
		public const byte PYRO		= 2;
		public const byte DEMOMAN	= 3;
		public const byte HEAVY		= 4;
		public const byte ENGINEER	= 5;
		public const byte SNIPER	= 6;
		public const byte MEDIC		= 7;
		public const byte SPY		= 8;
		public const byte RANDOM	= 9;

		public CookieCollection cookies;
		public SessionId identification;
		public string steamID64 = null;
		public DateTime lastAccessed;
		public DateTime lastUpdate;
		public Dictionary<string, List<Item>> sessionClassItems;
		public List<string> errors = new List<string>();
		public Task UpdateTask = null;
		public string playerName;
		public byte SelectClass;
		public bool clearErrors = false;
		public bool inventoryPulled { get; private set; }
		
		public Session(SessionId sid, CookieCollection cc)
		{
			cookies = cc;
			identification = sid;
			
			if(cookies["ID"] == null)
			{
				cookies.Add(new Cookie("ID", sid.GetHashCode().ToString()));
			}
			if(cookies["steamdID"] != null)
			{
				TrySetSteamID64(cookies["steamID"].Value);
			}
		}

		public bool isSession(Session sesh)
		{
			return isSession(sesh.identification);
		}

		public bool isSession(SessionId seshId)
		{
			return identification == seshId;
		}

		public void Accessed()
		{
			lastAccessed = DateTime.UtcNow;
		}

		//Attempts to set steamId64
		//Once done. It sets some other important varibles.
		public bool TrySetSteamID64(string SteamID64)
		{
			string steamTemp;
			
			//Test for status failure.
			if (!UserURLToSteamID64.TryParseSteamID64(SteamID64, out steamTemp))
			{
				errors.Add("Failure-To-Set-SteamID64");
				return false;
			}

			if(cookies["SteamID"] == null)
			{
				cookies.Add(new Cookie("SteamID", steamTemp));
			}
			else
			{
				cookies["SteamID"].Value = steamTemp;
			}

			steamID64 = steamTemp;
			updateInventory();
			return true;
		}

		public void updateInventory()
		{
			//Sets, runs, and quits if null
			if(UpdateTask == null)
			{
				UpdateTask = new Task(updateInventoryThread);
				UpdateTask.Start();
				return;
			}

			//Waits for task just in case it is still runnig.
			UpdateTask.Wait();

			//Makes a new task
			UpdateTask = new Task(updateInventoryThread);
			UpdateTask.Start();
		}
		public void updateInventoryThread()
		{
			//Kills thread if steamID64 is null
			lock (steamID64)
			{
				if (steamID64 == null)
				{
					return;
				}
			}
			
			Dictionary<string, List<Item>> tempClassItems = GeneralFunctions.InitializeDictonary();

			//If the steamid64 is on here, it's not invalid.
			//Therefor we can get away with directly setting the object.
			PlayerRootObject pro = SteamInventory.GetInventory(steamID64);
			playerName = SteamID64ToName.GetName(steamID64);

			//If a players inventory is not availible to the general public, permission to view the inventory is denied.
			//
			if (pro.result.statusDetail != null && pro.result.statusDetail.Equals("Permission denied"))
			{
				Console.WriteLine("Permission denied for {0}, Setting as default instead.", playerName);
				errors.Add("Permission-Denied");
				foreach (Item i in WeaponGather.RemoveReskins(WeaponGather.getWeapons()))
				{
					GeneralFunctions.InitializeItems(i, tempClassItems);
				}
			}
			else
			{
				inventoryPulled = true;
				List<PlayerItem> playerWeapons = pro.result.items;

				//Goes through all possible items
				foreach (Item i in WeaponGather.getWeapons())
				{
					if (i.name.Contains("Upgradeable"))
					{
						continue;
					}

					//Any items beginning with this are stock.
					//Everyone has stock.
					//EVERYONE!!!
					if (i.name.Contains("TF_WEAPON_"))
					{
						foreach (string str in i.used_by_classes)
						{
							tempClassItems[str].Add(i);
						}
					}
					else
					{
						foreach (PlayerItem pi in playerWeapons)
						{
							if (pi.defindex == i.defindex)
							{
								GeneralFunctions.InitializeItems(i, tempClassItems);
							}
						}
					}
				}
				Console.WriteLine("Got weapons for {0}", playerName);
			}
			if(sessionClassItems != null)
			{
				lock (sessionClassItems)
				{
					sessionClassItems = tempClassItems;
				}
			}
			else
			{
				sessionClassItems = tempClassItems;
			}
			lastUpdate = DateTime.UtcNow;
		}
	}
}
