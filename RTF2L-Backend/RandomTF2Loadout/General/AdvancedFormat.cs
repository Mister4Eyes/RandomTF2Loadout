using RandomTF2Loadout.WebServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RandomTF2Loadout.General
{
    class FormatKeyPair
    {
        string key;
        string value = null;
        Func<string, Session, string> stringProcessing = null;

        public FormatKeyPair(string Key, string Value)
        {
            key = Key;
            value = Value;
        }
        public FormatKeyPair(string Key, Func<string, Session, string> StringProcessing)
        {
            key = Key;
            stringProcessing = StringProcessing;
        }


        public bool HasKey(string text)
        {
            string matchString;

            //Changes format based on key type
            if(stringProcessing == null)
            {
                matchString = string.Format("{{{0}}}", key);
            }
            else
            {
                matchString = string.Format(@"{{{0}:([\w\d-]+)}}", key);
            }

            return Regex.Match(text, matchString).Success;
        }

        public string Replace(string text, Session session)
        {
            //Changes processing based on string type
            if (stringProcessing == null)
            {
                string matchString = string.Format("{{{0}}}", key);
                return text.Replace(matchString, value);

            }
            else
            {
                string matchString = string.Format(@"{{{0}:([\w\d-]+)}}", key);

                MatchCollection mc = Regex.Matches(text, matchString);
                foreach(Match m in mc)
                {
                    try
                    {
                        text = text.Replace(m.Value, stringProcessing(m.Groups[1].Value, session));
                    }
                    //Anything can come through and we don't want erros happening.
                    //But we want them to know whats happening so we add in the error message in the output.
                    //This is designed for html. However I may change this in the future.
                    catch(Exception e)
                    {
                        text = string.Format("--==Error on formating. Message:{0} Input:{1}==--",e.Message, m.Groups[1].Value);
                    }
                }
            }

            return text;
        }
    }

    class AdvancedFormat
    {
        public static string Format(string str, Session session, FormatKeyPair pair)
        {
            if (pair.HasKey(str))
            {
                str = pair.Replace(str, session);
            }

            return str;
        }
        public static string Format(string str, Session session,FormatKeyPair[] strings)
        {
            foreach(FormatKeyPair pair in strings)
            {
                str = Format(str, session, pair);
            }
            return str;
        }
    }
}
