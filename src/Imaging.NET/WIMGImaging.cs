using ManagedWimLib;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Imaging
{
    public class WIMGImaging : IImaging
    {
        private WIMImaging WIMLibImaging = new WIMImaging();

        public bool ExtractFileFromImage(string wimFile, int imageIndex, string fileToExtract, string destination)
        {
            return WIMLibImaging.ExtractFileFromImage(wimFile, imageIndex, fileToExtract, destination);
            using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Read,
                    WimCreationDisposition.OpenExisting,
                    WimCreateFileOptions.Chunked,
                    WimCompressionType.None))
            {
                // Always set a temporary path
                //
                WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, imageIndex))
                    {
                        WimgApi.ExtractImagePath(imageHandle, fileToExtract, destination);
                    }
                }
                catch (Exception ex)
                {
                    // A priv is not held by the client
                    if (ex.HResult == -2147467259)
                    {
                        return WIMLibImaging.ExtractFileFromImage(wimFile, imageIndex, fileToExtract, destination);
                    }

                    return false;
                }
            }
            return true;
        }

        // WIMLib
        public bool AddFileToImage(
            string wimFile,
            int imageIndex,
            string fileToAdd,
            string destination,
            IImaging.ProgressCallback progressCallback = null)
        {
            return WIMLibImaging.AddFileToImage(wimFile, imageIndex, fileToAdd, destination, progressCallback);
        }

        // WIMLib
        public bool RenameFileInImage(
            string wimFile,
            int imageIndex,
            string sourceFilePath,
            string destinationFilePath,
            IImaging.ProgressCallback progressCallback = null)
        {
            return WIMLibImaging.RenameFileInImage(wimFile, imageIndex, sourceFilePath, destinationFilePath, progressCallback);
        }

        // WIMLib
        public bool DeleteFileFromImage(
            string wimFile,
            int imageIndex,
            string fileToRemove,
            IImaging.ProgressCallback progressCallback = null)
        {
            return WIMLibImaging.DeleteFileFromImage(wimFile, imageIndex, fileToRemove, progressCallback);
        }

        public bool ExportImage(
            string wimFile,
            string destinationWimFile,
            int imageIndex,
            IEnumerable<string> referenceWIMs = null,
            WimCompressionType compressionType = WimCompressionType.Lzx,
            IImaging.ProgressCallback progressCallback = null)
        {
            return WIMLibImaging.ExportImage(wimFile,
                destinationWimFile,
                imageIndex,
                referenceWIMs,
                compressionType,
                progressCallback);
            string title = $"Exporting {wimFile.Split('\\').Last()} - Index {imageIndex}";
            try
            {
                WimMessageResult callback2(WimMessageType messageType, object message, object userData)
                {
                    switch (messageType)
                    {
                        case WimMessageType.Progress:
                            {
                                WimMessageProgress progressMessage = (WimMessageProgress)message;
                                progressCallback?.Invoke($"{title} (Estimated time remaining: {progressMessage.EstimatedTimeRemaining})", progressMessage.PercentComplete, false);
                                break;
                            }
                    }

                    return WimMessageResult.Success;
                }

                using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Read,
                    WimCreationDisposition.OpenExisting,
                    WimCreateFileOptions.Chunked,
                    WimCompressionType.None))
                {
                    // Always set a temporary path
                    //
                    WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                    if (referenceWIMs != null)
                    {
                        foreach (var referenceFile in referenceWIMs)
                        {
                            WimgApi.SetReferenceFile(wimHandle, referenceFile, WimSetReferenceMode.Append, WimSetReferenceOptions.Chunked);
                        }
                    }

                    // Register a method to be called while actions are performed by WIMGAPi for this .wim file
                    //
                    WimgApi.RegisterMessageCallback(wimHandle, callback2);

                    using (var newWimHandle = WimgApi.CreateFile(
                        destinationWimFile,
                        WimFileAccess.Write,
                        WimCreationDisposition.OpenAlways,
                        compressionType == WimCompressionType.Lzms ? WimCreateFileOptions.Chunked : WimCreateFileOptions.None,
                        compressionType))
                    {
                        // Always set a temporary path
                        //
                        WimgApi.SetTemporaryPath(newWimHandle, Environment.GetEnvironmentVariable("TEMP"));

                        // Register a method to be called while actions are performed by WIMGAPi for this .wim file
                        //
                        WimgApi.RegisterMessageCallback(newWimHandle, callback2);

                        try
                        {
                            using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, imageIndex))
                            {
                                var wiminfo = WimgApi.GetImageInformationAsString(imageHandle);
                                var wiminfoclass = WIMInformationXML.DeserializeIMAGE(wiminfo);
                                string imagename = wiminfoclass.DISPLAYNAME == null ? wiminfoclass.NAME : wiminfoclass.DISPLAYNAME;

                                title = $"Exporting {imagename} ({wimFile.Split('\\').Last()} - Index {imageIndex})";

                                // Export the image contents
                                // This call is blocking but WIMGAPI will be calling MyCallbackMethod() during the process
                                //
                                WimgApi.ExportImage(imageHandle, newWimHandle, WimExportImageOptions.AllowDuplicates);
                            }
                        }
                        finally
                        {
                            // Be sure to unregister the callback method
                            //
                            WimgApi.UnregisterMessageCallback(wimHandle, callback2);

                            // Be sure to unregister the callback method
                            //
                            WimgApi.UnregisterMessageCallback(newWimHandle, callback2);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool ApplyImage(
            string wimFile,
            int imageIndex,
            string OutputDirectory,
            IEnumerable<string> referenceWIMs = null,
            bool PreserveACL = true,
            IImaging.ProgressCallback progressCallback = null)
        {
            return WIMLibImaging.ApplyImage(wimFile, imageIndex, OutputDirectory, referenceWIMs, PreserveACL, progressCallback);
            string title = $"Applying {wimFile.Split('\\').Last()} - Index {imageIndex}";
            try
            {
                WimMessageResult callback2(WimMessageType messageType, object message, object userData)
                {
                    switch (messageType)
                    {
                        case WimMessageType.Progress:
                            {
                                WimMessageProgress progressMessage = (WimMessageProgress)message;
                                progressCallback?.Invoke($"{title} (Estimated time remaining: {progressMessage.EstimatedTimeRemaining})", progressMessage.PercentComplete, false);
                                break;
                            }
                    }

                    return WimMessageResult.Success;
                }

                using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Read,
                    WimCreationDisposition.OpenExisting,
                    WimCreateFileOptions.Chunked,
                    WimCompressionType.None))
                {
                    // Always set a temporary path
                    //
                    WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                    if (referenceWIMs != null)
                    {
                        foreach (var referenceFile in referenceWIMs)
                        {
                            WimgApi.SetReferenceFile(wimHandle, referenceFile, WimSetReferenceMode.Append, WimSetReferenceOptions.Chunked);
                        }
                    }

                    // Register a method to be called while actions are performed by WIMGAPi for this .wim file
                    //
                    WimgApi.RegisterMessageCallback(wimHandle, callback2);

                    try
                    {
                        using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, imageIndex))
                        {
                            var wiminfo = WimgApi.GetImageInformationAsString(imageHandle);
                            var wiminfoclass = WIMInformationXML.DeserializeIMAGE(wiminfo);
                            string imagename = wiminfoclass.DISPLAYNAME == null ? wiminfoclass.NAME : wiminfoclass.DISPLAYNAME;

                            title = $"Applying {imagename} ({wimFile.Split('\\').Last()} - Index {imageIndex})";

                            // Apply the image contents
                            // This call is blocking but WIMGAPI will be calling MyCallbackMethod() during the process
                            //
                            WimgApi.ApplyImage(imageHandle, OutputDirectory, PreserveACL ? WimApplyImageOptions.None : WimApplyImageOptions.DisableDirectoryAcl | WimApplyImageOptions.DisableFileAcl);
                        }
                    }
                    finally
                    {
                        // Be sure to unregister the callback method
                        //
                        WimgApi.UnregisterMessageCallback(wimHandle, callback2);
                    }
                }
            }
            catch (Exception ex)
            {
                // A priv is not held by the client
                if (ex.HResult == -2147467259)
                {
                    return WIMLibImaging.ApplyImage(wimFile, imageIndex, OutputDirectory, referenceWIMs, PreserveACL, progressCallback);
                }

                return false;
            }
            return true;
        }

        public bool GetWIMImageInformation(
            string wimFile,
            int imageIndex,
            out WIMInformationXML.IMAGE image)
        {
            image = null;
            try
            {
                using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Read,
                    WimCreationDisposition.OpenExisting,
                    WimCreateFileOptions.Chunked,
                    WimCompressionType.None))
                {
                    // Always set a temporary path
                    //
                    WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                    try
                    {
                        using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, imageIndex))
                        {
                            var wiminfo = WimgApi.GetImageInformationAsString(imageHandle);
                            image = WIMInformationXML.DeserializeIMAGE(wiminfo);
                        }
                    }
                    finally
                    {
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool SetWIMImageInformation(
            string wimFile,
            int imageIndex,
            WIMInformationXML.IMAGE image)
        {
            try
            {
                using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Write,
                    WimCreationDisposition.OpenExisting,
                    WimCreateFileOptions.Chunked,
                    WimCompressionType.None))
                {
                    // Always set a temporary path
                    //
                    WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                    try
                    {
                        using (WimHandle imageHandle = WimgApi.LoadImage(wimHandle, imageIndex))
                        {
                            string img = WIMInformationXML.SerializeIMAGE(image);
                            WimgApi.SetImageInformation(imageHandle, img);
                        }
                    }
                    finally
                    {
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool MarkImageAsBootable(string wimFile, int imageIndex)
        {
            using (var wimHandle = WimgApi.CreateFile(
                        wimFile,
                        WimFileAccess.Write,
                        WimCreationDisposition.OpenExisting,
                        WimCreateFileOptions.None,
                        WimCompressionType.None))
            {
                // Always set a temporary path
                //
                WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                try
                {
                    WimgApi.SetBootImage(wimHandle, imageIndex);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public bool CaptureImage(
            string wimFile,
            string imageName,
            string imageDescription,
            string imageFlag,
            string InputDirectory,
            string imageDisplayName = null,
            string imageDisplayDescription = null,
            WIMInformationXML.WINDOWS windows = null,
            WimCompressionType compressionType = WimCompressionType.Lzx,
            WimCaptureImageOptions addFlags = WimCaptureImageOptions.None,
            IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Creating {imageName} ({wimFile.Split('\\').Last()})";
            try
            {
                int directoriesScanned = 0;
                int filesScanned = 0;

                WimMessageResult callback2(WimMessageType messageType, object message, object userData)
                {
                    switch (messageType)
                    {
                        case WimMessageType.Process:
                            {
                                WimMessageProcess processMessage = (WimMessageProcess)message;
                                if (processMessage.Path.StartsWith(Path.Combine(InputDirectory.EndsWith(":") ? InputDirectory + @"\" : InputDirectory, @"System Volume Information"), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    processMessage.Process = false;
                                }
                                break;
                            }
                        case WimMessageType.Progress:
                            {
                                WimMessageProgress progressMessage = (WimMessageProgress)message;
                                progressCallback?.Invoke($"{title} (Estimated time remaining: {progressMessage.EstimatedTimeRemaining})", progressMessage.PercentComplete, false);
                                break;
                            }
                        case WimMessageType.Scanning:
                            {
                                WimMessageScanning scanningMessage = (WimMessageScanning)message;

                                switch (scanningMessage.CountType)
                                {
                                    case WimMessageScanningType.Directories:
                                        {
                                            directoriesScanned = scanningMessage.Count;
                                            break;
                                        }
                                    case WimMessageScanningType.Files:
                                        {
                                            filesScanned = scanningMessage.Count;
                                            break;
                                        }
                                }

                                progressCallback?.Invoke($"Scanning objects ({filesScanned} files, {directoriesScanned} directories scanned)", 0, true);
                                break;
                            }
                    }

                    return WimMessageResult.Success;
                }

                using (var wimHandle = WimgApi.CreateFile(
                    wimFile,
                    WimFileAccess.Write,
                    WimCreationDisposition.OpenAlways,
                    compressionType == WimCompressionType.Lzms ? WimCreateFileOptions.Chunked : WimCreateFileOptions.None,
                    compressionType))
                {
                    // Always set a temporary path
                    //
                    WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));

                    // Register a method to be called while actions are performed by WIMGAPi for this .wim file
                    //
                    WimgApi.RegisterMessageCallback(wimHandle, callback2);

                    try
                    {
                        using (var imagehandle = WimgApi.CaptureImage(wimHandle, InputDirectory, addFlags))
                        {
                            var wiminfo = WimgApi.GetImageInformationAsString(imagehandle);
                            var wiminfoclass = WIMInformationXML.DeserializeIMAGE(wiminfo);
                            if (!string.IsNullOrEmpty(imageFlag))
                                wiminfoclass.FLAGS = imageFlag;
                            if (!string.IsNullOrEmpty(imageName))
                                wiminfoclass.NAME = imageName;
                            if (!string.IsNullOrEmpty(imageDescription))
                                wiminfoclass.DESCRIPTION = imageDescription;
                            if (!string.IsNullOrEmpty(imageDisplayName))
                                wiminfoclass.DISPLAYNAME = imageDisplayName;
                            if (!string.IsNullOrEmpty(imageDisplayDescription))
                                wiminfoclass.DISPLAYDESCRIPTION = imageDisplayDescription;
                            if (windows != null)
                                wiminfoclass.WINDOWS = windows;

                            wiminfo = WIMInformationXML.SerializeIMAGE(wiminfoclass);
                            WimgApi.SetImageInformation(imagehandle, wiminfo);
                        }
                    }
                    finally
                    {
                        // Be sure to unregister the callback method
                        //
                        WimgApi.UnregisterMessageCallback(wimHandle, callback2);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}