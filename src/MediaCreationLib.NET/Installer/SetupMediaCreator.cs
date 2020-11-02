using MediaCreationLib.NET;
using Microsoft.Wim;
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
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
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