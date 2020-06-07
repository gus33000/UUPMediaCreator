using Imaging;
using Microsoft.Wim;
using System;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib
{
    public class UUPMediaCreator
    {
        private static WIMImaging imagingInterface = new WIMImaging();

        public static bool CreateUpgradedEditionFromMountedImage(
            string MountedImagePath,
            string EditionID,
            string OutputInstallImage,
            bool IsVirtual,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            string SourceEdition = DismOperations.DismOperations.GetCurrentEdition(MountedImagePath);

            WIMInformationXML.WIM wiminfo;
            result = imagingInterface.GetWIMInformation(OutputInstallImage, out wiminfo);
            if (!result)
                goto exit;

            var srcimage = wiminfo.IMAGE.First(x => x.WINDOWS.EDITIONID.Equals(SourceEdition, StringComparison.InvariantCultureIgnoreCase));
            var index = int.Parse(srcimage.INDEX);

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

            void callback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressInPercentage, SubOperation);
            };

            DismOperations.DismOperations.SetTargetEdition(MountedImagePath, EditionID, callback);

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CapturingImage, IsIndeterminate, ProgressPercentage, Operation);
            };

            string name = $"Windows 10 {EditionID}";
            if (Constants.FriendlyEditionNames.Any(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)))
            {
                name = Constants.FriendlyEditionNames.First(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)).Value;
            }

            result = imagingInterface.CaptureImage(
                OutputInstallImage,
                name,
                name,
                EditionID,
                MountedImagePath,
                name,
                name,
                compression,
                progressCallback: callback2,
                UpdateFrom: index);

            if (!result)
                goto exit;

            WIMInformationXML.IMAGE tmpImageInfo;
            result = imagingInterface.GetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, out tmpImageInfo);
            if (!result)
                goto exit;

            var sku = tmpImageInfo.WINDOWS.EDITIONID;

            tmpImageInfo.WINDOWS = srcimage.WINDOWS;
            tmpImageInfo.WINDOWS.EDITIONID = sku;
            tmpImageInfo.FLAGS = sku;

            tmpImageInfo.NAME = name;
            tmpImageInfo.DESCRIPTION = name;
            tmpImageInfo.DISPLAYNAME = name;
            tmpImageInfo.DISPLAYDESCRIPTION = name;

            result = imagingInterface.SetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, tmpImageInfo);
            if (!result)
                goto exit;

            if (IsVirtual)
            {
                File.Delete(Path.Combine(MountedImagePath, "Windows", $"{EditionID}.xml"));
            }

        exit:
            return result;
        }

        public static bool CreateISO(
            string OutputMediaPath,
            string OutputISOPath,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            string installimage = Path.Combine(OutputMediaPath, "sources", "install.wim");
            if (!File.Exists(installimage))
                installimage = Path.Combine(OutputMediaPath, "sources", "install.esd");

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            WIMInformationXML.WIM image;
            result = imagingInterface.GetWIMInformation(installimage, out image);
            if (!result)
                goto exit;

            string skustr = "CCOMA";

            if (image.IMAGE.Count == 1)
            {
                switch (image.IMAGE[0].WINDOWS.EDITIONID.ToLower())
                {
                    case "core":
                        {
                            skustr = "CCRA";
                            break;
                        }
                    case "coren":
                        {
                            skustr = "CCRNA";
                            break;
                        }
                    case "corecountryspecific":
                        {
                            skustr = "CCHA";
                            break;
                        }
                    case "coresinglelanguage":
                        {
                            skustr = "CSLA";
                            break;
                        }
                    case "ppipro":
                        {
                            skustr = "CPPIA";
                            break;
                        }
                    case "professional":
                        {
                            skustr = "CPRA";
                            break;
                        }
                    case "professionaln":
                        {
                            skustr = "CPRNA";
                            break;
                        }
                    case "professionaleducation":
                        {
                            skustr = "CPEDA";
                            break;
                        }
                    case "professionaleducationn":
                        {
                            skustr = "CPEDNA";
                            break;
                        }
                    case "professionalworkstation":
                        {
                            skustr = "CPWRKA";
                            break;
                        }
                    case "professionalworkstationn":
                        {
                            skustr = "CPWRKNA";
                            break;
                        }
                    case "education":
                        {
                            skustr = "CEDA";
                            break;
                        }
                    case "educationn":
                        {
                            skustr = "CEDNA";
                            break;
                        }
                    case "enterprise":
                        {
                            skustr = "CENA";
                            break;
                        }
                    case "enterprisen":
                        {
                            skustr = "CENNA";
                            break;
                        }
                    case "enterprises":
                        {
                            skustr = "CESA";
                            break;
                        }
                    case "enterprisesn":
                        {
                            skustr = "CESNA";
                            break;
                        }
                    case "enterpriseg":
                        {
                            skustr = "CEGA";
                            break;
                        }
                    case "starter":
                        {
                            skustr = "CSTA";
                            break;
                        }
                    case "cloud":
                        {
                            skustr = "CCLA";
                            break;
                        }
                    case "cloude":
                        {
                            skustr = "CCEA";
                            break;
                        }
                }
            }

            string arch = "";
            switch (image.IMAGE[0].WINDOWS.ARCH)
            {
                case "0":
                    {
                        arch = "X86";
                        break;
                    }
                case "5":
                    {
                        arch = "WOA";
                        break;
                    }
                case "6":
                    {
                        arch = "IA64";
                        break;
                    }
                case "9":
                    {
                        arch = "X64";
                        break;
                    }
                case "12":
                    {
                        arch = "A64";
                        break;
                    }
            }

            string label = $"{skustr}_{arch}FRE_{image.IMAGE[0].WINDOWS.LANGUAGES.DEFAULT.ToUpper()}_DV9";

            void cdcallback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CreatingISO, IsIndeterminate, ProgressPercentage, Operation);
            }

            // TODO proper labelling of the disc image
            result = CDImage.CDImage.GenerateISOImage(OutputISOPath, OutputMediaPath, label, cdcallback);

        exit:
            return result;
        }
    }
}
