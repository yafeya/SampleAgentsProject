using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Keysight.KCE.IOSamples
{
    public static class StringExtensions
    {
        public static int GetIndex(this string target, string prifix)
        {
            var regex = GenerateRegex(prifix);
            var match = Regex.Match(target, regex, RegexOptions.None);
            int index = -1;
            if (match.Success)
            {
                var value = match.Groups["index"].Value;
                if (!int.TryParse(value, out index))
                {
                    index = -1;
                }
            }
            return index;
        }
        public static string GenerateNewPrifix(this string prifix, int index)
        {
            var builder = new StringBuilder();
            builder.Append(prifix).Append(index.ToString());
            return builder.ToString();
        }
        private static string GenerateRegex(string prifix)
        {
            var builder = new StringBuilder();
            builder.Append(@"^").Append(prifix).Append(@"(?<index>\d+)");
            return builder.ToString();
        }
    }
}
