using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SocketApp.ProtocolStack.Util
{
    public static class SaveFile
    {
        // return new path name
        public static string WriteFile(string path, byte[] data)
        {
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, data);
                return path;
            }
            // find out the last contiguous index
            int index;
            do
            {
                //  if postfix
                if (Regex.IsMatch(path, @"^.*\.\d+$"))
                {
                    // get index
                    string indexStr = Regex.Matches(path, @"\d+").Last().Value;
                    index = int.Parse(indexStr);
                    // replace index
                    path = Regex.Replace(path, @"(?<=^.*\.)\d+(?=$)", (index + 1).ToString());
                }
                else
                {
                    index = 0;
                    // attach index
                    path = $"{path}.{index + 1}";
                }
                // check validation
            } while (File.Exists(path));

            File.WriteAllBytes(path, data);
            return path;
        }
    }
}
