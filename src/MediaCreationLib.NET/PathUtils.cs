using System.Diagnostics;
using System.Linq;

namespace MediaCreationLib
{
    public class PathUtils
    {
        public static string GetExecutableDirectory()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains("\\") ? string.Join("\\", fileName.Split('\\').Reverse().Skip(1).Reverse()) : "";
        }

        public static string GetParentExecutableDirectory()
        {
            var runningDirectory = GetExecutableDirectory();
            return runningDirectory.Contains("\\") ? string.Join("\\", runningDirectory.Split('\\').Reverse().Skip(1).Reverse()) : "";
        }
    }
}