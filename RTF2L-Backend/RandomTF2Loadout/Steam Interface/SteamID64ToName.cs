using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomTF2Loadout.Steam_Interface
{
    public class Player
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public int lastlogoff { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public int personastate { get; set; }
        public string realname { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
        public string locstatecode { get; set; }
        public int loccityid { get; set; }
    }

    public class NameResponse
    {
        public List<Player> players { get; set; }
    }

    public class NameRootObject
    {
        public NameResponse response { get; set; }
    }
    class SteamID64ToName
    {
        public static string GetName(string steamID64)
        {
            const string playerApi = @"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}";
            NameRootObject nro = JsonConvert.DeserializeObject<NameRootObject>(General.htmlDownloader.downloadHTML(string.Format(playerApi, SteamKEY.key, steamID64)));
            return nro.response.players[0].personaname;
        }
    }
}
