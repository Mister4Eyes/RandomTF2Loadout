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

namespace RandomTF2Loadout
{
	class Program
	{
		static void Main(string[] args) => new Program().Start();
		
		public bool TryGetStatic(HttpListenerContext hlc, out byte[] data)
		{
			DirectoryInfo staticDirectory = new DirectoryInfo(General.GeneralFunctions.getBaseDirectory() + @"\static");

			//Checks if there is a static directory
			if (staticDirectory.Exists)
			{
				FileInfo urlFile = new FileInfo(string.Format(@"{0}\static{1}", General.GeneralFunctions.getBaseDirectory(), hlc.Request.Url.AbsolutePath));

				//Checks if file exists
				if (urlFile.Exists)
				{
					hlc.Response.StatusCode = 200;
					hlc.Response.ContentType = MimeTypeMap.GetMimeType(urlFile.Extension);
					hlc.Response.ContentEncoding = Encoding.UTF8;
					data = File.ReadAllBytes(urlFile.FullName);
					return true;
				}
			}
			data = null;
			return false;
		}

		public byte[] BaseSite(HttpListenerContext hlc)
		{
			FileInfo fzf = new FileInfo(string.Format(@"{0}\base.html", General.GeneralFunctions.getClientDirectory()));
			hlc.Response.StatusCode = 200;
			hlc.Response.ContentType = "text/html";
			hlc.Response.ContentEncoding = Encoding.UTF8;
			if (fzf.Exists)
			{
				return File.ReadAllBytes(fzf.FullName);
			}
			return FourZeroFour(hlc);
		}

		public byte[] FourZeroFour(HttpListenerContext hlc)
		{
			FileInfo fzf = new FileInfo(string.Format(@"{0}\404.html",General.GeneralFunctions.getClientDirectory()));

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

					//Defaults to static data
				default:
					byte[] data;
					if(TryGetStatic(hlc, out data))
					{
						return data;
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
			InventoryRootObject inventory = SteamInventory.GetInventory(steamID64);
		}
		//*/
		//*
		public void Start()
		{
			WebServer.WebServer ws = new WebServer.WebServer(new[] { "http://localhost:8080/" }, HttpFunction);
			ws.Run();
			Console.WriteLine("Press any key to stop.");
			Console.ReadKey(true);
			ws.Stop();
		}
		//*/
	}
}
