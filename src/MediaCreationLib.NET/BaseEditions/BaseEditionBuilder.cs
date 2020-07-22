using CompDB;
using Imaging;
using Microsoft.Cabinet;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib.BaseEditions
{
    public class BaseEditionBuilder
    {
        private static WIMImaging imagingInterface = new WIMImaging();

        public static bool CreateBaseEdition(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            string InputWindowsREPath,
            string OutputInstallImage,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

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

            List<string> ReferencePackages = new List<string>();
            List<string> referencePackagesToConvert = new List<string>();
            string BaseESD = null;
            CompDBXmlClass.CompDB compDB = null;

            if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
            {
                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                {
                    IEnumerable<string> potentialFiles = cabinet.Files.Where(x => x.ToLower().Contains($"desktoptargetcompdb_{EditionID.ToLower()}_{LanguageCode.ToLower()}"));

                    if (potentialFiles.Count() == 0)
                        goto exit;

                    string file = potentialFiles.First();

                    using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            compDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                        }
                    }
                }
            }
            else
            {
                IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x));
                IEnumerable<string> potentialFiles = files.Where(x => x.ToLower().Contains($"desktoptargetcompdb_{EditionID.ToLower()}_{LanguageCode.ToLower()}"));

                if (potentialFiles.Count() == 0)
                    goto exit;

                string file = potentialFiles.First();

                using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(Path.Combine(UUPPath, file))))
                {
                    string xmlfile = cabinet2.Files.First();
                    using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                    {
                        compDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                    }
                }
            }

            if (compDB == null)
                goto exit;

            foreach (CompDBXmlClass.Package feature in compDB.Features.Feature[0].Packages.Package)
            {
                CompDBXmlClass.Package pkg = compDB.Packages.Package.First(x => x.ID == feature.ID);

                string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    file = pkg.Payload.PayloadItem.Path;
                    if (!File.Exists(Path.Combine(UUPPath, file)))
                    {
                        goto exit;
                    }
                }

                if (feature.PackageType == "MetadataESD")
                {
                    BaseESD = Path.Combine(UUPPath, file);
                    continue;
                }

                if (file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase))
                {
                    ReferencePackages.Add(Path.Combine(UUPPath, file));
                }
                else if (file.EndsWith(".cab", StringComparison.InvariantCultureIgnoreCase))
                {
                    referencePackagesToConvert.Add(Path.Combine(UUPPath, file));
                }
            }

            if (BaseESD == null)
                goto exit;

            progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, true, 0, "Converting Reference Cabinets");

            int counter = 0;
            int total = referencePackagesToConvert.Count;
            foreach (var file in referencePackagesToConvert)
            {
                int progressoffset = (int)Math.Round((double)counter / total * 100);
                int progressScale = (int)Math.Round((double)1 / total * 100);

                string refesd = ConvertCABToESD(Path.Combine(UUPPath, file), progressCallback, progressoffset, progressScale);
                if (string.IsNullOrEmpty(refesd))
                {
                    goto exit;
                }
                ReferencePackages.Add(refesd);
                counter++;
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            WIMInformationXML.IMAGE image;
            imagingInterface.GetWIMImageInformation(BaseESD, 3, out image);

            //
            // Export the install image
            //
            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
            };

            result = imagingInterface.ExportImage(
                BaseESD,
                OutputInstallImage,
                3,
                referenceWIMs: ReferencePackages,
                compressionType: compression,
                progressCallback: callback);
            if (!result)
                goto exit;

            WIMInformationXML.WIM wim;
            imagingInterface.GetWIMInformation(OutputInstallImage, out wim);

            //
            // Set the correct metadata on the image
            //
            image.DISPLAYNAME = image.NAME;
            image.DISPLAYDESCRIPTION = image.NAME;
            image.FLAGS = image.WINDOWS.EDITIONID;
            result = imagingInterface.SetWIMImageInformation(OutputInstallImage, wim.IMAGE.Count, image);
            if (!result)
                goto exit;

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, IsIndeterminate, ProgressPercentage, Operation);
            };

            //
            // Integrate the WinRE image into the installation image
            //
            progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, true, 0, "");

            result = imagingInterface.AddFileToImage(
                OutputInstallImage,
                wim.IMAGE.Count,
                InputWindowsREPath,
                Path.Combine("Windows", "System32", "Recovery", "Winre.wim"),
                callback2);
            if (!result)
                goto exit;

            exit:
            return result;
        }

        private static string ConvertCABToESD(string CABPath, ProgressCallback progressCallback, int progressoffset, int progressscale)
        {
            string CABFileNameWithoutExtension = Path.GetTempPath() + (CABPath.Contains(".") ? string.Join(".", CABPath.Split('.').Reverse().Skip(1).Reverse()) : CABPath).Split('\\').Last();
            if (File.Exists(CABFileNameWithoutExtension + ".ESD"))
                return CABFileNameWithoutExtension + ".ESD";

            progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressoffset, "Unpacking...");

            var tmp = Path.GetTempFileName();
            File.Delete(tmp);

            string tempExtractionPath = Path.Combine(tmp, "Package");
            int progressScaleHalf = progressscale / 2;

            void ProgressCallback(int percent, string file)
            {
                progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressoffset + (int)Math.Round((double)percent / 100 * progressScaleHalf), "Unpacking " + file + "...");
            };

            CabinetHandler.ExpandFiles(CABPath, tempExtractionPath, ProgressCallback);

            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, IsIndeterminate, progressoffset + progressScaleHalf + (int)Math.Round((double)ProgressPercentage / 100 * progressScaleHalf), Operation);
            };

            bool result = imagingInterface.CaptureImage(CABFileNameWithoutExtension + ".ESD", "Metadata ESD", null, null, tempExtractionPath, compressionType: WimCompressionType.None, PreserveACL: false, progressCallback: callback);

            Directory.Delete(tmp, true);

            if (!result)
                return null;

            return CABFileNameWithoutExtension + ".ESD";
        }
    }
}
