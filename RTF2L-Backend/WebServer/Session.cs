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
		byte[] ipHash;
		public SessionId(IPAddress ip)
		{
			ipHash = ipToBytes(ip);
		}

		#region OperatorOverloads
		public static bool operator ==(SessionId sid1, SessionId sid2)
		{
			return sid1.Equals(sid2);
		}

		public static bool operator !=(SessionId sid1, SessionId sid2)
		{
			return !sid1.Equals(sid2);
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

		//There has to be a hash code.
		//However, int isn't big enough to contain an md5 hash so the equality operator uses the whole thing in order to prevent collision.
		//And before you ask, no this is not cryptographicly secure.
		//Hell, this isn't even meant to last until I get the browser to send cookies.
		public override int GetHashCode()
		{
			return BitConverter.ToInt32(ipHash, 0);
		}

		public override bool Equals(object obj)
		{
			if(obj is SessionId)
			{
				return EqualArrayComparison((obj as SessionId).ipHash);
			}
			else if (obj is Session)
			{
				return Equals((obj as Session).identification.ipHash);
			}
			else if (obj is byte[])
			{
				byte[] possHash = obj as byte[];

				if (possHash.Length != ipHash.Length)
				{
					return false;
				}

				return EqualArrayComparison(possHash);
			}
			else
			{
				return false;
			}
		}

		private bool EqualArrayComparison(byte[] hash)
		{
			for (int i = 0; i < ipHash.Length; ++i)
			{
				if (ipHash[i] != hash[i])
				{
					return false;
				}
			}

			return true;
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

		public Session(IPAddress ip)
		{
			inventoryPulled = false;
			identification = new SessionId(ip);
			Accessed();
			sessionClassItems = GeneralFunctions.InitializeDictonary();
			SelectClass = 10;
		}
		
		public bool isSession(IPAddress ip)
		{
			return identification == new SessionId(ip);
		}

		public void Accessed()
		{
			lastAccessed = DateTime.UtcNow;
		}

		//Attempts to set steamId64
		//Once done. It sets some other important varibles.
		public bool TrySetSteamID64(string SteamID64)
		{
			bool status = UserURLToSteamID64.TryParseSteamID64(SteamID64, out steamID64);
			
			//Test for status failure.
			if (!status)
			{
				errors.Add("Failure-To-Set-SteamID64");
				return false;
			}
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

			lock (sessionClassItems)
			{
				sessionClassItems = tempClassItems;
			}
			lastUpdate = DateTime.UtcNow;
		}
	}
}
