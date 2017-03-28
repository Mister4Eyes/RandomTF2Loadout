using RandomTF2Loadout.Steam_Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RandomTF2Loadout.General
{
	class GeneralFunctions
	{
		const bool UP	= true; //I am a parent and I'm checking my parents
		const bool DOWN	= false;//I am a child and I'm checking my children

		public static bool CookieCollectionHasValue(string value, CookieCollection cc)
		{
			Cookie c;//Not used
			return TryGetCookieValue(value, cc, out c);
		}
		public static bool TryGetCookieValue(string value, CookieCollection cookieCollection, out Cookie cookie)
		{
			cookie = null;
			foreach(Cookie c in cookieCollection)
			{
				if (c.Name.Equals(value))
				{
					cookie = c;
					return true;
				}
			}
			return false;
		}
		public static void InitializeItems(Item i, Dictionary<string, List<Item>> tempClassItems)
		{
			foreach (string str in i.used_by_classes)
			{
				Item changeITM = i;
				if (changeITM.name.Equals("The B.A.S.E. Jumper") && str.Equals("Demoman"))
				{
					changeITM = new Item(i);
					changeITM.item_slot = "primary";
				}
				tempClassItems[str].Add(changeITM);
			}
		}

		public static Dictionary<string, List<Item>> InitializeDictonary()
		{
			return new Dictionary<string, List<Item>>()
			{
				{"Scout",   new List<Item>() },
				{"Soldier", new List<Item>() },
				{"Pyro",	new List<Item>() },
				{"Demoman", new List<Item>() },
				{"Heavy",   new List<Item>() },
				{"Engineer",new List<Item>() },
				{"Medic",   new List<Item>() },
				{"Sniper",  new List<Item>() },
				{"Spy",	 new List<Item>() }
			};
		}
		
		public static bool CompareBytes(byte[] b1, byte[] b2)
		{
			//Test #1
			//Checks if lengths are correct size
			if(b1.Length != b2.Length)
			{
				return false;
			}

			//Test #2
			//Loops through array to look for unequal bytes.
			for(int i = 0; i < b1.Length; ++i)
			{
				if(b1[i] != b2[i])
				{
					return false;
				}
			}
			
			return true;
		}

		public static string getBaseDirectory()
		{
			FileInfo fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
			return fi.Directory.FullName;
		}

		//The ref is for insurance
		static void addChildren(ref Queue<Tuple<bool, DirectoryInfo>> queue, DirectoryInfo di)
		{
			try
			{
				foreach (DirectoryInfo child in di.GetDirectories())
				{
					queue.Enqueue(new Tuple<bool, DirectoryInfo>(DOWN, child));
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		static void addParent(ref Queue<Tuple<bool, DirectoryInfo>> queue, DirectoryInfo di)
		{
			queue.Enqueue(new Tuple<bool, DirectoryInfo>(UP, di.Parent));
		}

		public static string getClientDirectory()
		{
			string clientDirectoryName = ParseConfigFile("ClientDirectoryName");

			return FindDirectory(clientDirectoryName);
		}

		public static string FindDirectory(string name, DirectoryInfo start = null)
		{
			if (start == null)
			{
				start = new DirectoryInfo(getBaseDirectory());
			}

			Queue<Tuple<bool, DirectoryInfo>> directoriesToBeChecked = new Queue<Tuple<bool, DirectoryInfo>>();
			directoriesToBeChecked.Enqueue(new Tuple<bool, DirectoryInfo>(UP, start));

			do
			{
				bool direction = directoriesToBeChecked.Peek().Item1;
				DirectoryInfo checkDirectory = directoriesToBeChecked.Dequeue().Item2;

				if (checkDirectory != null)
				{
					//Found Directory
					if (checkDirectory.Name.Equals(name))
					{
						return checkDirectory.FullName + "\\";
					}

					if (direction)
					{
						addParent(ref directoriesToBeChecked, checkDirectory);
						addChildren(ref directoriesToBeChecked, checkDirectory);
					}
					else
					{
						addChildren(ref directoriesToBeChecked, checkDirectory);
					}
				}
			}
			while (directoriesToBeChecked.Count > 0);

			//I've checked everywhere! It can't be found
			//To be honest, I would expect an out of memory exception before this happens.
			//But a situaion like this could be possible...
			return null;
		}

		public static string FindFile(string name, DirectoryInfo start = null)
		{
			if(start == null)
			{
				start = new DirectoryInfo(getBaseDirectory());
			}

			Queue<Tuple<bool, DirectoryInfo>> directoriesToBeChecked = new Queue<Tuple<bool, DirectoryInfo>>();
			directoriesToBeChecked.Enqueue(new Tuple<bool, DirectoryInfo>(UP, start));

			do
			{
				bool direction = directoriesToBeChecked.Peek().Item1;
				DirectoryInfo checkDirectory = directoriesToBeChecked.Dequeue().Item2;

				if (checkDirectory != null)
				{
					//Found File
					foreach(FileInfo file in checkDirectory.EnumerateFiles())
					{
						if (file.Name.Equals(name))
						{
							return file.FullName;
						}
					}

					if (direction)
					{
						addParent(ref directoriesToBeChecked, checkDirectory);
						addChildren(ref directoriesToBeChecked, checkDirectory);
					}
					else
					{
						addChildren(ref directoriesToBeChecked, checkDirectory);
					}
				}
			}
			while (directoriesToBeChecked.Count > 0);

			//I've checked everywhere! It can't be found
			//To be honest, I would expect an out of memory exception before this happens.
			//But a situaion like this could be possible...
			return null;
		}

		//This function should be used sparingly in the first part of initialization due to how it searches for the config file.
		//It should NOT be used in any sort of loop unless that loop is part of initialization and not while the web server is running.
		public static string ParseConfigFile(string key)
		{
			string path = FindFile("config.cfg");
			
			if(path == null)
			{
				throw new FileNotFoundException("Could not find the config file.");
			}

			Match m;
			if (RegexFunctions.matches(File.ReadAllText(path), string.Format(@"{0}=(.+)",key), out m))
			{
				return m.Groups[1].Value.Trim();
			}

			return null;
		}
	}
}
