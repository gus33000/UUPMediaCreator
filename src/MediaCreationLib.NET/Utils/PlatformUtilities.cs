using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace MediaCreationLib.NET.Utils
{
    public static class PlatformUtilities
    {
        public static readonly OSPlatform OperatingSystem = GetOperatingSystem();
        public static readonly bool RunsAsAdministrator = IsAdministrator();
        public static readonly string CurrentRunningDirectory = GetCurrentRunningDirectory();

        private static bool IsAdministrator()
        {
            if (OperatingSystem == OSPlatform.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                return false;
            }
        }

        private static OSPlatform GetOperatingSystem()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? OSPlatform.OSX
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? OSPlatform.Linux
                : RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? OSPlatform.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                ? OSPlatform.FreeBSD
                : throw new Exception("Cannot determine operating system!");
        }

        private static string GetCurrentRunningDirectory()
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            return FolderUtilities.GetParentPath(fileName);
        }
    }
}
