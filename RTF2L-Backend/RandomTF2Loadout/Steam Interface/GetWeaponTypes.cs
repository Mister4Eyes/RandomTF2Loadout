using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RandomTF2Loadout.Steam_Interface
{
	class WeaponInformation
	{
		public string Name;
		public string Slot;
		public string Class;
		public bool Reskin;
		public WeaponInformation()
		{
			Name	= null;
			Slot	= null;
			Class	= null;
			Reskin = false;
		}

		public WeaponInformation(string str)
		{
			Match m	= Regex.Match(str, @"{(.+)\t(.+)\t(.+)\t(true|false)}");
			if (m.Success)
			{
				Name	= m.Groups[1].Value;
				Class	= m.Groups[2].Value;
				Slot	= m.Groups[3].Value;
				Reskin	= bool.Parse(m.Groups[4].Value);
			}
			else
			{
				Name	= null;
				Class	= null;
				Slot	= null;
				Reskin	= false;
			}
		}

		public WeaponInformation(string n, string s, string c, bool r)
		{
			Name	= n;
			Slot	= s;
			Class	= c;
			Reskin	= r;
		}

		public WeaponInformation[] getWeaponInformation(string str)
		{
			List<WeaponInformation> wi = new List<WeaponInformation>();

			Match m = Regex.Match(str, "{(.+)\t(.+)\t(.+)\t(true|false)}");
			do
			{
				wi.Add(new WeaponInformation(m.Value));
				m = m.NextMatch();
			} while (m.Success);

			return wi.ToArray();
		}

		public override int GetHashCode()
		{
			return (Name.GetHashCode() * Slot.GetHashCode() * Class.GetHashCode()) + Reskin.GetHashCode();
		}
	}

	class GetWeaponTypes
	{
		static string getInfo(HtmlNode baseNode, string checkText)
		{

			List<HtmlNode> ub = baseNode.Descendants().Where(x => (x.Attributes["class"] != null && x.Attributes["class"] != null && x.InnerText.Contains(checkText))).ToList();

			if (ub.Count == 0)
			{
				return null;
			}

			HtmlNode parent = ub[0].ParentNode;

			List<HtmlNode> toftitle = parent.Descendants().Where(x => (x.Name.Equals("a"))).ToList();

			StringBuilder sb = new StringBuilder();
			foreach(HtmlNode node in toftitle)
			{
				sb.Append(string.Format("{0},",node.InnerText));
			}
			
			return sb.ToString().Substring(0, sb.Length-1);
		}
	}
}
