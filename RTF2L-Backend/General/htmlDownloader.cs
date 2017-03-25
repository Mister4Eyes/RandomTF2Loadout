using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RandomTF2Loadout.General
{
	class htmlDownloader
	{
		public static string downloadHTML(string url, params Object[] args)
		{
			string formattedURl = string.Format(url, args);
			using(WebClient wc = new WebClient())
			{
				return WebUtility.HtmlDecode(wc.DownloadString(formattedURl));
			}
		}
	}
}
