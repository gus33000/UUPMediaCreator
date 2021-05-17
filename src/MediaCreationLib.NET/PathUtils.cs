using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MediaCreationLib
{
    public class PathUtils
    {
        public static string GetExecutableDirectory()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, fileName.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse()) : "";
        }

        public static string GetParentExecutableDirectory()
        {
            var runningDirectory = GetExecutableDirectory();
            return runningDirectory.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, runningDirectory.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse()) : "";
        }
    }
}