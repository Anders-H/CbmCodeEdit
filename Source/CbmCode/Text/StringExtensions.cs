using System.Collections.Generic;
using System.Text;

namespace CbmCode.Text
{
    public static class StringExtensions
    {
        public static string Join(this List<string> me)
        {
            if (me == null)
                return "";

            var s = new StringBuilder();

            foreach (var generatedCodeGeneratedLine in me)
                s.AppendLine(generatedCodeGeneratedLine);

            return s.ToString();
        }

        public static string[] Split(this string me)
        {
            return me.Split('\n', '\r');
        }
    }
}