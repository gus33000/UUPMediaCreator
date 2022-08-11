using Imaging.NET;
using System;
using System.IO;
using UUPMediaCreator.InterCommunication;

namespace MediaCreationLib.NET.CDImage
{
    public static class DiscImageFactory
    {
        public static bool CreateDiscImageFromWindowsMediaPath(
            string OutputMediaPath,
            string OutputISOPath,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            string installimage = Path.Combine(OutputMediaPath, "sources", "install.wim");
            if (!File.Exists(installimage))
            {
                installimage = Path.Combine(OutputMediaPath, "sources", "install.esd");
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            result = Constants.imagingInterface.GetWIMInformation(installimage, out WIMInformationXML.WIM image);
            if (!result)
            {
                goto exit;
            }

            string skustr = "CCOMA";

            if (image.IMAGE[0].WINDOWS.EDITIONID.Contains("server", StringComparison.InvariantCultureIgnoreCase))
            {
                skustr = "SSS";
            }

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
            result = CDImageWrapper.GenerateISOImage(OutputISOPath, OutputMediaPath, label, cdcallback);

        exit:
            return result;
        }
    }
}
