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
    class Session
    {
        byte[] ipHash;
        public string steamID64 = null;
        public DateTime lastAccessed;
        public DateTime lastUpdate;
        public Dictionary<string, List<Item>> sessionClassItems;
        public List<string> errors = new List<string>();
        public Task UpdateTask = null;
        public string playerName;

        public Session(IPAddress ip)
        {
            ipHash = ipToBytes(ip);
            Accessed();
            sessionClassItems = GeneralFunctions.InitializeDictonary();
        }

        static byte[] ipToBytes(IPAddress ip)
        {

            using (MD5 md5 = MD5.Create())
            {
                //Here for storage. IP's are NOT stored in session and it allows for all ip's to have a fixed size.
                return md5.ComputeHash(ip.GetAddressBytes());
            }
        }
        
        public bool isSession(IPAddress ip)
        {
            byte[] tIpHash = ipToBytes(ip);
            return GeneralFunctions.CompareBytes(ipHash, tIpHash);
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
                    foreach(PlayerItem pi in playerWeapons)
                    {
                        if(pi.defindex == i.defindex)
                        {
                            foreach (string str in i.used_by_classes)
                            {
                                Item changeITM = i;
                                if (changeITM.name.Equals("The B.A.S.E. Jumper") && str.Equals("Demoman"))
                                {
                                    changeITM = new Item(changeITM);
                                    changeITM.item_slot = "primary";
                                }
                                tempClassItems[str].Add(i);
                            }
                        }
                    }
                }
            }

            lock (sessionClassItems)
            {
                sessionClassItems = tempClassItems;
            }
            lastUpdate = DateTime.UtcNow;
        }
    }
}
