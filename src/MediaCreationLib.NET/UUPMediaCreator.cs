﻿/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using Imaging;
using MediaCreationLib.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib
{
    public static class UUPMediaCreator
    {
        private static readonly WIMImaging imagingInterface = new();

        public static string LongestCommonSubstring(IList<string> values)
        {
            string result = string.Empty;

            for (int i = 0; i < values.Count - 1; i++)
            {
                for (int j = i + 1; j < values.Count; j++)
                {
                    if (LongestCommonSubstring(values[i], values[j], out string tmp) > result.Length)
                    {
                        result = tmp;
                    }
                }
            }

            return result;
        }

        // Source: http://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_substring
        public static int LongestCommonSubstring(string str1, string str2, out string sequence)
        {
            sequence = string.Empty;
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            {
                return 0;
            }

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            StringBuilder sequenceBuilder = new();

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                    {
                        num[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                        {
                            num[i, j] = 1;
                        }
                        else
                        {
                            num[i, j] = 1 + num[i - 1, j - 1];
                        }

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(str1[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0; //clear it
                                sequenceBuilder.Append(str1[lastSubsBegin..(i + 1)]);
                            }
                        }
                    }
                }
            }
            sequence = sequenceBuilder.ToString();
            return maxlen;
        }

        public static bool CreateUpgradedEditionFromMountedImage(
            string MountedImagePath,
            string EditionID,
            string OutputInstallImage,
            bool IsVirtual,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            string SourceEdition = DismOperations.GetCurrentEdition(MountedImagePath);

            result = WIMImaging.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wiminfo);
            if (!result)
            {
                goto exit;
            }

            WIMInformationXML.IMAGE srcimage = wiminfo.IMAGE.First(x => x.WINDOWS.EDITIONID.Equals(SourceEdition, StringComparison.InvariantCultureIgnoreCase));
            int index = int.Parse(srcimage.INDEX);

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
            }

            DismOperations.SetTargetEdition(MountedImagePath, EditionID, callback);

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CapturingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            string replaceStr = LongestCommonSubstring(new string[] { srcimage.DISPLAYNAME, SourceEdition });
            string replaceStr2 = LongestCommonSubstring(new string[] { EditionID, SourceEdition });

            if (!string.IsNullOrEmpty(replaceStr2))
            {
                replaceStr2 = EditionID.Replace(replaceStr2, "");
            }
            else
            {
                replaceStr2 = EditionID;
            }

            string name;
            string description;
            string displayname;
            string displaydescription;

            if (string.IsNullOrEmpty(replaceStr))
            {
                name = $"Windows 10 {replaceStr2}";
                description = name;
                displayname = name;
                displaydescription = name;
            }
            else
            {
                name = srcimage.NAME.Replace(replaceStr, replaceStr2, StringComparison.InvariantCultureIgnoreCase);
                description = srcimage.DESCRIPTION.Replace(replaceStr, replaceStr2, StringComparison.InvariantCultureIgnoreCase);
                displayname = srcimage.DISPLAYNAME.Replace(replaceStr, replaceStr2, StringComparison.InvariantCultureIgnoreCase);
                displaydescription = srcimage.DISPLAYDESCRIPTION.Replace(replaceStr, replaceStr2, StringComparison.InvariantCultureIgnoreCase);
            }

            if (Constants.FriendlyEditionNames.Any(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)))
            {
                name = Constants.FriendlyEditionNames.First(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)).Value;
                description = name;
                displayname = name;
                displaydescription = name;
            }

            result = imagingInterface.CaptureImage(
                OutputInstallImage,
                name,
                description,
                EditionID,
                MountedImagePath,
                displayname,
                displaydescription,
                compression,
                progressCallback: callback2,
                UpdateFrom: index);

            if (!result)
            {
                goto exit;
            }

            result = WIMImaging.GetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, out WIMInformationXML.IMAGE tmpImageInfo);
            if (!result)
            {
                goto exit;
            }

            //
            // Set the correct metadata on the image
            //
            string sku = tmpImageInfo.WINDOWS.EDITIONID;

            tmpImageInfo.WINDOWS = srcimage.WINDOWS;
            tmpImageInfo.WINDOWS.EDITIONID = sku;
            tmpImageInfo.FLAGS = sku;

            if (tmpImageInfo.WINDOWS.INSTALLATIONTYPE.EndsWith(" Core", StringComparison.InvariantCultureIgnoreCase) && !tmpImageInfo.FLAGS.EndsWith("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                tmpImageInfo.FLAGS += "Core";
            }

            tmpImageInfo.NAME = name;
            tmpImageInfo.DESCRIPTION = description;
            tmpImageInfo.DISPLAYNAME = displayname;
            tmpImageInfo.DISPLAYDESCRIPTION = displaydescription;

            result = WIMImaging.SetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, tmpImageInfo);
            if (!result)
            {
                goto exit;
            }

            if (IsVirtual)
            {
                File.Delete(Path.Combine(MountedImagePath, "Windows", $"{EditionID}.xml"));
            }
            else
            {
                File.Delete(Path.Combine(MountedImagePath, "Windows", $"{SourceEdition}.xml"));
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
            {
                installimage = Path.Combine(OutputMediaPath, "sources", "install.esd");
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            result = WIMImaging.GetWIMInformation(installimage, out WIMInformationXML.WIM image);
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
            result = CDImage.CDImage.GenerateISOImage(OutputISOPath, OutputMediaPath, label, cdcallback);

        exit:
            return result;
        }
    }
}