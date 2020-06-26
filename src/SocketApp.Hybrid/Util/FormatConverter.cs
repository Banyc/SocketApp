using System;

namespace SocketApp.Util
{
    public static class FormatConverter
    {
        public static string ByteSizeToHumanReadable(double lenInByte)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = lenInByte;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public static string SecondToHumanReadable(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string result = "";
            if (t.Hours > 0)
                result = $"{t.Hours}h ";
            if (t.Minutes > 0)
                result = $"{result}{t.Minutes}m ";
            if (t.Seconds > 0)
                result = $"{result}{t.Seconds}s";
            if (result == "")
                result = "0s";
            return result;
        }
    }
}
