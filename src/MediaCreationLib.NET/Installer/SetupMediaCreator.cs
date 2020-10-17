using CompDB;
using Microsoft.Cabinet;
using Microsoft.Wim;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            List<string> ReferencePackages = new List<string>();
            List<string> referencePackagesToConvert = new List<string>();
            string BaseESD = null;
            List<CompDBXmlClass.CompDB> compDBs = new List<CompDBXmlClass.CompDB>();

            if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
            {
                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                {
                    IEnumerable<string> potentialFiles = cabinet.Files.Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_") &&
                    x.ToLower().Contains($"_{LanguageCode.ToLower()}") &&
                    !x.ToLower().Contains("lxp") &&
                    !x.ToLower().Contains($"desktoptargetcompdb_{LanguageCode.ToLower()}"));

                    if (potentialFiles.Count() == 0)
                        goto exit;

                    foreach (var file in potentialFiles)
                    {
                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                            {
                                compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                            }
                        }
                    }
                }
            }
            else
            {
                IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x));
                IEnumerable<string> potentialFiles = files.Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_") &&
                    x.ToLower().Contains($"_{LanguageCode.ToLower()}") &&
                    !x.ToLower().Contains("lxp") &&
                    !x.ToLower().Contains($"desktoptargetcompdb_{LanguageCode.ToLower()}"));

                if (potentialFiles.Count() == 0)
                    goto exit;

                foreach (var file in potentialFiles)
                {
                    using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(Path.Combine(UUPPath, file))))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                    }
                }
            }

            if (compDBs.Count == 0)
                goto exit;

            foreach (var firstCompDB in compDBs)
            {
                foreach (CompDBXmlClass.Package feature in firstCompDB.Features.Feature[0].Packages.Package)
                {
                    CompDBXmlClass.Package pkg = firstCompDB.Packages.Package.First(x => x.ID == feature.ID);

                    string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                    if (feature.PackageType == "MetadataESD")
                    {
                        if (!File.Exists(Path.Combine(UUPPath, file)))
                        {
                            file = pkg.Payload.PayloadItem.Path;
                            if (!File.Exists(Path.Combine(UUPPath, file)))
                            {
                                break;
                            }
                        }

                        BaseESD = Path.Combine(UUPPath, file);
                        break;
                    }
                }

                if (BaseESD != null)
                    break;
            }

            if (BaseESD == null)
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