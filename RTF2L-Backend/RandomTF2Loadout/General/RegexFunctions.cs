using System.Text.RegularExpressions;

namespace RandomTF2Loadout.General
{
	class RegexFunctions
	{
		public static bool matches(string input, string pattern)
		{
			return Regex.Match(input, pattern).Success;
		}
		public static bool matches(string input, string pattern, out Match m)
		{
			m = Regex.Match(input, pattern);
			return m.Success;
		}
	}
}
