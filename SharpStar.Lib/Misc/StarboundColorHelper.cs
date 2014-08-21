using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharpStar.Lib.Misc
{
    public static class StarboundColorHelper
    {

        public static string StripColors(this string text)
        {
            return Regex.Replace(text, "\\^[#a-zA-Z0-9]+;?", String.Empty, RegexOptions.IgnoreCase);
        }

    }
}
