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



		public string FormatWebpage(string str)
		{
			//Doubles up the {} so the formatter dosen't get confused
			str = str.Replace("{", "{{").Replace("}", "}}");

			{
				Dictionary<string, Func<string, string>> dict = new Dictionary<string, Func<string, string>>() {/*TODO: add Code function pairs*/};

				foreach(string code in dict.Keys)
				{
					//Checks if the code is there
					if (str.Contains("{{"+code+"}}"))
					{
						str = str.Replace("{{"+code+"}}", "{0}");
						str = string.Format(str, dict[code](code).Replace("}", "}}"));
					}
				}

				Match staticFormat = Regex.Match(str, @"{{([\w\d-]+)}}");

				//Goes through any static moduels
				while (staticFormat.Success)
				{
					string data;
					if(TryGetModuel(staticFormat.Groups[1].Value, out data))
					{
						str = str.Replace(staticFormat.Value, "{0}");
						str = string.Format(str, FormatWebpage(data).Replace("}", "}}"));
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
				FileInfo moduelFile = new FileInfo(string.Format(@"{0}moduels\{1}.html", ClientDirectory, path));

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

			foreach(Item i in items)
			{
				Console.Write("Name:{0}\nSlot:{1}\nUsed by ",i.name,i.item_slot);
				if(i.used_by_classes == null)
				{
					continue;
				}
				foreach(object v in i.used_by_classes)
				{
					Console.Write("{0},", v.ToString());
				}
				Console.Write("\b \n\n");

				Thread.Sleep(250);
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

			WebServer.WebServer ws = new WebServer.WebServer(new[] { "http://localhost:9090/","http://192.168.1.8:9090/" }, HttpFunction);
			ws.Run();
			Console.WriteLine("Press any key to stop.");
			Console.ReadKey(true);
			ws.Stop();
		}
		//*/
	}
}
