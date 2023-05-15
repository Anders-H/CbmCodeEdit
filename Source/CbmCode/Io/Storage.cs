using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CbmCode.Io
{
    public static class Storage
    {
        public static void SaveFile(string filename, string text)
        {
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.Write(text);
                sw.Flush();
                sw.Close();
            }
        }

        public static void SaveFile(string filename, IEnumerable<string> lines)
        {
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                foreach (var l in lines)
                    sw.WriteLine(l);

                sw.Flush();
                sw.Close();
            }
        }

        public static string LoadFile(string filename)
        {
            string result;

            using (var sr = new StreamReader(filename, Encoding.UTF8))
            {
                result = sr.ReadToEnd();
                sr.Close();
            }

            return result;
        }
    }
}