using RandomTF2Loadout.Steam_Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RandomTF2Loadout.General
{
	class GeneralFunctions
	{
		const bool UP	= true; //I am a parent and I'm checking my parents
		const bool DOWN	= false;//I am a child and I'm checking my children

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
                {"Pyro",    new List<Item>() },
                {"Demoman", new List<Item>() },
                {"Heavy",   new List<Item>() },
                {"Engineer",new List<Item>() },
                {"Medic",   new List<Item>() },
                {"Sniper",  new List<Item>() },
                {"Spy",     new List<Item>() }
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

            //Nothing failed, therefor they are the same.
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

			Queue<Tuple<bool, DirectoryInfo>> directoriesToBeChecked = new Queue<Tuple<bool, DirectoryInfo>>();
			directoriesToBeChecked.Enqueue(new Tuple<bool, DirectoryInfo>(UP, new DirectoryInfo(getBaseDirectory())));

			do
			{
				bool direction = directoriesToBeChecked.Peek().Item1;
				DirectoryInfo checkDirectory = directoriesToBeChecked.Dequeue().Item2;

				if(checkDirectory != null)
				{
					Console.WriteLine("{0}\t{1}", directoriesToBeChecked.Count + 1, checkDirectory.Name);
					//Found directory
					if (checkDirectory.Name.Equals(clientDirectoryName))
					{
						return checkDirectory.FullName+"\\";
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

		public static string ParseConfigFile(string key)
		{
			string path = string.Format("{0}\\config.cfg", getBaseDirectory());
			Match m;
			if (RegexFunctions.matches(File.ReadAllText(path), string.Format(@"{0}=(.+)",key), out m))
			{
				try
				{
					return m.Groups[1].Value.Trim();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}
			}

			//I feel that searching through the entire goddamn computer for 1 directory deserves an exception.
			throw new DirectoryNotFoundException(string.Format("Could not find {0}."));
		}
	}
}
