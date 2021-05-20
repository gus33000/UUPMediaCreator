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
using Cabinet;
using Imaging;
using MediaCreationLib.NET;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib.BaseEditions
{
    public static class BaseEditionBuilder
    {
        private static readonly WIMImaging imagingInterface = new();

        public static bool CreateBaseEdition(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            string InputWindowsREPath,
            string OutputInstallImage,
            Common.CompressionType CompressionType,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

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

            HashSet<string> ReferencePackages, referencePackagesToConvert;
            string BaseESD = null;

            (result, BaseESD, ReferencePackages, referencePackagesToConvert) = FileLocator.LocateFilesForBaseEditionCreation(UUPPath, LanguageCode, EditionID, progressCallback);
            if (!result)
            {
                goto exit;
            }

            progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, true, 0, "Converting Reference Cabinets");

            int counter = 0;
            int total = referencePackagesToConvert.Count;
            foreach (string file in referencePackagesToConvert)
            {
                int progressoffset = (int)Math.Round((double)counter / total * 100);
                int progressScale = (int)Math.Round((double)1 / total * 100);

                string refesd = ConvertCABToESD(Path.Combine(UUPPath, file), progressCallback, progressoffset, progressScale, tempManager);
                if (string.IsNullOrEmpty(refesd))
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "CreateBaseEdition -> Reference ESD creation from Cabinet files failed");
                    goto exit;
                }
                ReferencePackages.Add(refesd);
                counter++;
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            result = WIMImaging.GetWIMImageInformation(BaseESD, 3, out WIMInformationXML.IMAGE image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> GetWIMImageInformation failed");
                goto exit;
            }

            //
            // Export the install image
            //
            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            result = imagingInterface.ExportImage(
                BaseESD,
                OutputInstallImage,
                3,
                referenceWIMs: ReferencePackages,
                compressionType: compression,
                progressCallback: callback);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> ExportImage failed");
                goto exit;
            }

            result = WIMImaging.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wim);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> GetWIMInformation failed");
                goto exit;
            }

            //
            // Set the correct metadata on the image
            //
            image.DISPLAYNAME = image.NAME;
            image.DISPLAYDESCRIPTION = image.DESCRIPTION;
            image.NAME = image.NAME;
            image.DESCRIPTION = image.NAME;
            image.FLAGS = image.WINDOWS.EDITIONID;
            if (image.WINDOWS.INSTALLATIONTYPE.EndsWith(" Core", StringComparison.InvariantCultureIgnoreCase) && !image.FLAGS.EndsWith("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                image.FLAGS += "Core";
            }

            if (image.WINDOWS.LANGUAGES == null)
            {
                image.WINDOWS.LANGUAGES = new WIMInformationXML.LANGUAGES()
                {
                    LANGUAGE = LanguageCode,
                    FALLBACK = new WIMInformationXML.FALLBACK()
                    {
                        LANGUAGE = LanguageCode,
                        Text = "en-US"
                    },
                    DEFAULT = LanguageCode
                };
            }
            result = WIMImaging.SetWIMImageInformation(OutputInstallImage, wim.IMAGE.Count, image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> SetWIMImageInformation failed");
                goto exit;
            }

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, IsIndeterminate, ProgressPercentage, Operation);
            }

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
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> AddFileToImage failed");
                goto exit;
            }

        exit:
            return result;
        }

        private static string ConvertCABToESD(string cabFilePath, ProgressCallback progressCallback, int progressoffset, int progressscale, TempManager.TempManager tempManager)
        {
            string esdFilePath = Path.ChangeExtension(cabFilePath, "esd");
            if (File.Exists(esdFilePath))
            {
                return esdFilePath;
            }

            progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressoffset, "Unpacking...");

            string tmp = tempManager.GetTempPath();

            string tempExtractionPath = Path.Combine(tmp, "Package");
            int progressScaleHalf = progressscale / 2;

            void ProgressCallback(int percent, string file)
            {
                progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressoffset + (int)Math.Round((double)percent / 100 * progressScaleHalf), "Unpacking " + file + "...");
            }

            CabinetExtractor.ExtractCabinet(cabFilePath, tempExtractionPath, ProgressCallback);

            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, IsIndeterminate, progressoffset + progressScaleHalf + (int)Math.Round((double)ProgressPercentage / 100 * progressScaleHalf), Operation);
            }

            bool result = imagingInterface.CaptureImage(esdFilePath, "Metadata ESD", null, null, tempExtractionPath, compressionType: WimCompressionType.None, PreserveACL: false, progressCallback: callback);

            Directory.Delete(tmp, true);

            return !result ? null : esdFilePath;
        }
    }
}