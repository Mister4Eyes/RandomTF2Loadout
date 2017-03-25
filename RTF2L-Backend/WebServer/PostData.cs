using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RandomTF2Loadout.WebServer
{
	class PostData
	{
		Dictionary<string, string> PostDataDictionary = new Dictionary<string, string>();
		public PostData(string raw)
		{
			string[] rawPost = raw.Split('&');
			foreach(string item in rawPost)
			{
				AddPair(item);
			}
		}

		public bool Contains(string key)
		{
			return PostDataDictionary.ContainsKey(key);
		}

		public int Length
		{
			get
			{
				return PostDataDictionary.Keys.Count;
			}
		}

		public string this[string key]
		{
			get
			{
				return GetValue(key);
			}
			set
			{
				SetValue(key, value);
			}
		}

		public string this[int index]
		{
			get
			{
				return PostDataDictionary.Keys.ElementAt(index);
			}
		}
		public string GetValue(string key)
		{
			if (PostDataDictionary.ContainsKey(key))
			{
				return PostDataDictionary[key];
			}
			return null;
		}

		public bool SetValue(string key, string value)
		{
			if (PostDataDictionary.ContainsKey(key))
			{
				PostDataDictionary[key] = value;
				return true;
			}
			return false;
		}

		public bool AddPair(string pair)
		{
			string[] items = pair.Split('=');
			if(items.Length == 2)
			{
				AddPair(items[0], items[1]);
				return true;
			}
			return false;
		}

		public void AddPair(string key, string value)
		{
			PostDataDictionary.Add(HttpUtility.UrlDecode(key), HttpUtility.UrlDecode(value));
		}

		public bool RemovePair(string key)
		{
			if (PostDataDictionary.ContainsKey(key))
			{
				PostDataDictionary.Remove(key);
				return true;
			}
			return false;
		}
	}
}
