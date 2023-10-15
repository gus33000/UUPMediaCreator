using IniParser;
using IniParser.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnifiedUpdatePlatform.Media.Creator.DismOperations;

namespace UnifiedUpdatePlatform.Media.Creator.Settings
{
    public static class IniReader
    {
        private static IniData GetData()
        {
            return GetIniData();
        }

        private static IniData GetIniData()
        {
            string parentDirectory = PathUtils.GetParentExecutableDirectory();
            string iniPath = Path.Combine(parentDirectory, "Settings", "Settings.ini");

            if (!File.Exists(iniPath))
            {
                parentDirectory = PathUtils.GetExecutableDirectory();
                iniPath = Path.Combine(parentDirectory, "Settings", "Settings.ini");
            }

            if (!File.Exists(iniPath))
            {
                parentDirectory = PathUtils.GetExecutableDirectory();
                iniPath = Path.Combine(parentDirectory, "Settings.ini");
            }

            FileIniDataParser parser = new();
            return parser.ReadFile(iniPath);
        }

        public static IEnumerable<string> SetupFilesToBackport => GetData()["PEFiles"].Select(x => x.KeyName.Replace('\\', Path.DirectorySeparatorChar));

        public static Dictionary<string, string> FriendlyEditionNames => new(GetData()["FriendlyEditionNames"].Select(x => new KeyValuePair<string, string>(x.KeyName, x.Value)));
    }
}
