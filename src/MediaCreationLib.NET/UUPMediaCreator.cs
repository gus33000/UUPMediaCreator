/*
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
using Imaging.NET;
using MediaCreationLib.NET.Settings;
using Microsoft.Wim.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UUPMediaCreator.InterCommunication;

namespace MediaCreationLib.NET
{
    public static class UUPMediaCreator
    {
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
                        num[i, j] = i == 0 || j == 0 ? 1 : 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {//if the current LCS is the same as the last time this block ran
                                _ = sequenceBuilder.Append(str1[i]);
                            }
                            else //this block resets the string builder if a different LCS is found
                            {
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0; //clear it
                                _ = sequenceBuilder.Append(str1[lastSubsBegin..(i + 1)]);
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
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            string SourceEdition = DismOperations.NET.DismOperations.Instance.GetCurrentEdition(MountedImagePath);

            result = Constants.imagingInterface.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wiminfo);
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

            DismOperations.NET.DismOperations.Instance.SetTargetEdition(MountedImagePath, EditionID, callback);

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CapturingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            string replaceStr = LongestCommonSubstring(new string[] { srcimage.DISPLAYNAME, SourceEdition });
            string replaceStr2 = LongestCommonSubstring(new string[] { EditionID, SourceEdition });

            replaceStr2 = !string.IsNullOrEmpty(replaceStr2) ? EditionID.Replace(replaceStr2, "") : EditionID;

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

            if (IniReader.FriendlyEditionNames.Any(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)))
            {
                name = IniReader.FriendlyEditionNames.First(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)).Value;
                description = name;
                displayname = name;
                displaydescription = name;
            }

            result = Constants.imagingInterface.CaptureImage(
                OutputInstallImage,
                name,
                description,
                EditionID,
                MountedImagePath,
                tempManager,
                displayname,
                displaydescription,
                compression,
                progressCallback: callback2,
                UpdateFrom: index);

            if (!result)
            {
                goto exit;
            }

            result = Constants.imagingInterface.GetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, out WIMInformationXML.IMAGE tmpImageInfo);
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

            result = Constants.imagingInterface.SetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, tmpImageInfo);
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
    }
}