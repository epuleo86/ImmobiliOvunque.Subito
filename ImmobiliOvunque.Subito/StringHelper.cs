using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ImmobiliOvunque.Subito
{
    public static class StringHelper
    {
        public static string ClearEmojy(string txt)
        {
            string str = txt;
            foreach (var a in str)
            {
                byte[] bts = Encoding.UTF32.GetBytes(a.ToString());

                if (bts[0].ToString() == "253" && bts[1].ToString() == "255")
                {
                    str = str.Replace(a.ToString(), "");
                }

            }
            return str;
        }

        public static string ClearText(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return txt;

            string str = ClearEmojy(txt);
            str = Regex.Replace(str, "[^\x00-\x7F]", string.Empty);

            return str;
        }
    }
}
