using MediaCreationLib.NET;
using Microsoft.Wim;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib.Installer
{
    public class SetupMediaCreator
    {
        private static bool RunsAsAdministrator = IsAdministrator();

        private static bool IsAdministrator()
        {
            if (GetOperatingSystem() == OSPlatform.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                return false;
            }
        }

        public static OSPlatform GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return OSPlatform.FreeBSD;
            }

            throw new Exception("Cannot determine operating system!");
        }

        public static bool CreateSetupMedia(
            string UUPPath,
            string LanguageCode,
            string OutputMediaPath,
            string OutputWindowsREPath,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;
            string BaseESD = null;

            (result, BaseESD) = FileLocator.LocateFilesForSetupMediaCreation(UUPPath, LanguageCode, progressCallback);
            if (!result)
                goto exit;

            WimCompressionType compression = WimCompressionType.None;
            switch (CompressionType)
            {
                case Common.CompressionType.LZMS:
                    compression = WimCompressionType.Lzms;
                    break;

                case Common.CompressionType.LZX:
                    compression = WimCompressionType.Lzx;
                    break;

                case Common.CompressionType.XPRESS:
                    compression = WimCompressionType.Xpress;
                    break;
            }

            //
            // Build installer
            //
            result = WindowsInstallerBuilder.BuildSetupMedia(BaseESD, OutputWindowsREPath, OutputMediaPath, compression, RunsAsAdministrator, LanguageCode, progressCallback);
            if (!result)
                goto exit;

            exit:
            return result;
        }
    }
}