using ManagedWimLib;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Imaging
{
    public class WIMImaging //: IImaging
    {
        private static string GetExecutableDirectory()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains("\\") ? string.Join("\\", fileName.Split('\\').Reverse().Skip(1).Reverse()) : "";
        }

        private static void InitNativeLibrary()
        {
            string arch = null;
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }

            var runningDirectory = GetExecutableDirectory();

            string libPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libPath = Path.Combine(runningDirectory, arch, "libwim-15.dll");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libPath = Path.Combine(runningDirectory, arch, "libwim.so");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libPath = Path.Combine(runningDirectory, arch, "libwim.dylib");

            if (libPath == null || !File.Exists(libPath))
                throw new PlatformNotSupportedException();

            try
            {
                Wim.GlobalInit(libPath);
            }
            catch (InvalidOperationException)
            {
                // Lib already initialized
            }
        }

        public WIMImaging()
        {
            InitNativeLibrary();
        }

        public bool AddFileToImage(string wimFile, int imageIndex, string fileToAdd, string destination, IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Adding {destination} to {wimFile.Split('\\').Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.SCAN_BEGIN:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.SCAN_END:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_BEGIN:
                            break;
                        case ProgressMsg.UPDATE_BEGIN_COMMAND:
                            break;
                        case ProgressMsg.UPDATE_END_COMMAND:
                            break;
                        case ProgressMsg.WRITE_STREAMS:
                            {
                                ProgressInfo_WriteStreams m = (ProgressInfo_WriteStreams)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_END:
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        new UpdateCommand()
                        {
                            Add = new UpdateCommand.UpdateAdd(fileToAdd, destination, null, AddFlags.DEFAULT),
                            AddFlags = AddFlags.DEFAULT
                        },
                        UpdateFlags.SEND_PROGRESS);
                    wim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                }
            }
            catch
            {
                return false;
            }
            return ReseatWIMXml(wimFile);
        }
        
        public bool DeleteFileFromImage(string wimFile, int imageIndex, string fileToRemove, IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Removing {fileToRemove} from {wimFile.Split('\\').Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.SCAN_BEGIN:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.SCAN_END:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_BEGIN:
                            break;
                        case ProgressMsg.UPDATE_BEGIN_COMMAND:
                            break;
                        case ProgressMsg.UPDATE_END_COMMAND:
                            break;
                        case ProgressMsg.WRITE_STREAMS:
                            {
                                ProgressInfo_WriteStreams m = (ProgressInfo_WriteStreams)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_END:
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        new UpdateCommand()
                        {
                            Delete = new UpdateCommand.UpdateDelete(fileToRemove, DeleteFlags.DEFAULT),
                            DeleteFlags = DeleteFlags.DEFAULT
                        },
                        UpdateFlags.SEND_PROGRESS);
                    wim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                }
            }
            catch
            {
                return false;
            }
            return ReseatWIMXml(wimFile);
        }

        public bool ExportImage(string wimFile, string destinationWimFile, int imageIndex, IEnumerable<string> referenceWIMs = null, WimCompressionType compressionType = WimCompressionType.Lzx, IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Exporting {wimFile.Split('\\').Last()} - Index {imageIndex}";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.WRITE_STREAMS:
                            {
                                ProgressInfo_WriteStreams m = (ProgressInfo_WriteStreams)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_BEGIN:
                            break;
                        case ProgressMsg.WRITE_METADATA_END:
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                using (Wim srcWim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    string imageName = srcWim.GetImageName(imageIndex);
                    string imageDescription = srcWim.GetImageDescription(imageIndex);

                    var compression = CompressionType.NONE;
                    switch (compressionType)
                    {
                        case WimCompressionType.Lzms:
                            {
                                compression = CompressionType.LZMS;
                                break;
                            }
                        case WimCompressionType.Lzx:
                            {
                                compression = CompressionType.LZX;
                                break;
                            }
                        case WimCompressionType.None:
                            {
                                compression = CompressionType.NONE;
                                break;
                            }
                        case WimCompressionType.Xpress:
                            {
                                compression = CompressionType.XPRESS;
                                break;
                            }
                    }

                    if (referenceWIMs != null && referenceWIMs.Count() > 0)
                    {
                        srcWim.ReferenceResourceFiles(referenceWIMs, RefFlags.DEFAULT, OpenFlags.DEFAULT);
                    }

                    if (File.Exists(destinationWimFile))
                    {
                        using (Wim destWim = Wim.OpenWim(destinationWimFile, OpenFlags.WRITE_ACCESS))
                        {
                            destWim.RegisterCallback(ProgressCallback);

                            if (destWim.IsImageNameInUse(imageName))
                            {
                                srcWim.ExportImage(imageIndex, destWim, imageName + " " + DateTime.UtcNow.ToString(), imageDescription, ExportFlags.DEFAULT);
                            }
                            else
                            {
                                srcWim.ExportImage(imageIndex, destWim, imageName, imageDescription, ExportFlags.DEFAULT);
                            }

                            destWim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                        }
                    }
                    else
                    {
                        using (Wim destWim = Wim.CreateNewWim(compression))
                        {
                            destWim.RegisterCallback(ProgressCallback);
                            srcWim.ExportImage(imageIndex, destWim, imageName, imageDescription, ExportFlags.DEFAULT);
                            destWim.Write(destinationWimFile, Wim.AllImages, compression == CompressionType.LZMS ? WriteFlags.SOLID : WriteFlags.DEFAULT, Wim.DefaultThreads);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return ReseatWIMXml(destinationWimFile);
        }

        public bool ExtractFileFromImage(string wimFile, int imageIndex, string fileToExtract, string destination)
        {
            try
            {
                var filename = Path.GetFileName(fileToExtract);
                var extractDir = Environment.GetEnvironmentVariable("TEMP");

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.ExtractPath(imageIndex, extractDir, fileToExtract, ExtractFlags.NO_PRESERVE_DIR_STRUCTURE);
                }
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                File.Move(Path.Combine(extractDir, filename), destination);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool RenameFileInImage(string wimFile, int imageIndex, string sourceFilePath, string destinationFilePath, IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Renaming {sourceFilePath} to {destinationFilePath} in {wimFile.Split('\\').Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.SCAN_BEGIN:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.SCAN_END:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_BEGIN:
                            break;
                        case ProgressMsg.UPDATE_BEGIN_COMMAND:
                            break;
                        case ProgressMsg.UPDATE_END_COMMAND:
                            break;
                        case ProgressMsg.WRITE_STREAMS:
                            {
                                ProgressInfo_WriteStreams m = (ProgressInfo_WriteStreams)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_END:
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        new UpdateCommand()
                        {
                            Rename = new UpdateCommand.UpdateRename(sourceFilePath, destinationFilePath)
                        },
                        UpdateFlags.SEND_PROGRESS);
                    wim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                }
            }
            catch
            {
                return false;
            }
            return ReseatWIMXml(wimFile);
        }

        public bool ApplyImage(string wimFile, int imageIndex, string OutputDirectory, IEnumerable<string> referenceWIMs = null, bool PreserveACL = true, IImaging.ProgressCallback progressCallback = null)
        {
            string title = $"Applying {wimFile.Split('\\').Last()} - Index {imageIndex}";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.EXTRACT_IMAGE_BEGIN:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                            }
                            break;
                        case ProgressMsg.EXTRACT_IMAGE_END:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                            }
                            break;
                        case ProgressMsg.EXTRACT_FILE_STRUCTURE:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                progressCallback?.Invoke($"Applying file structure ({(int)Math.Round((double)m.CurrentFileCount / m.EndFileCount * 100)}%)", 0, true);
                            }
                            break;
                        case ProgressMsg.EXTRACT_STREAMS:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.EXTRACT_METADATA:
                            {
                                ProgressInfo_Extract m = (ProgressInfo_Extract)info;
                                progressCallback?.Invoke($"Applying metadata ({(int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100)}%)", 0, true);
                            }
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.RegisterCallback(ProgressCallback);
                    if (referenceWIMs != null && referenceWIMs.Count() > 0)
                        wim.ReferenceResourceFiles(referenceWIMs, RefFlags.DEFAULT, OpenFlags.DEFAULT);
                    wim.ExtractImage(imageIndex, OutputDirectory, PreserveACL ? ExtractFlags.STRICT_ACLS : ExtractFlags.NO_ACLS);
                }
            }
            catch (Exception ex)
            {
                return false;
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
            WimCompressionType compressionType = WimCompressionType.Lzx, 
            IImaging.ProgressCallback progressCallback = null,
            int UpdateFrom = -1,
            bool PreserveACL = true)
        {
            string title = $"Creating {imageName} ({wimFile.Split('\\').Last()})";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.SCAN_BEGIN:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.SCAN_DENTRY:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.SCAN_END:
                            {
                                ProgressInfo_Scan m = (ProgressInfo_Scan)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_BEGIN:
                            break;
                        case ProgressMsg.WRITE_STREAMS:
                            {
                                ProgressInfo_WriteStreams m = (ProgressInfo_WriteStreams)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;
                        case ProgressMsg.WRITE_METADATA_END:
                            break;
                    }
                    return CallbackStatus.CONTINUE;
                }

                if (File.Exists(wimFile))
                {
                    using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.WRITE_ACCESS))
                    {
                        wim.RegisterCallback(ProgressCallback);
                        wim.AddImage(InputDirectory, imageName, null, PreserveACL ? AddFlags.STRICT_ACLS : AddFlags.NO_ACLS);
                        if (!string.IsNullOrEmpty(imageDescription))
                            wim.SetImageDescription((int)wim.GetWimInfo().ImageCount, imageDescription);
                        if (!string.IsNullOrEmpty(imageDisplayName))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYNAME", imageDisplayName);
                        if (!string.IsNullOrEmpty(imageDisplayDescription))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYDESCRIPTION", imageDisplayDescription);
                        if (!string.IsNullOrEmpty(imageFlag))
                            wim.SetImageFlags((int)wim.GetWimInfo().ImageCount, imageFlag);
                        if (UpdateFrom != -1)
                            wim.ReferenceTemplateImage((int)wim.GetWimInfo().ImageCount, UpdateFrom);
                        wim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                    }
                }
                else
                {
                    var compression = CompressionType.NONE;
                    switch (compressionType)
                    {
                        case WimCompressionType.Lzms:
                            {
                                compression = CompressionType.LZMS;
                                break;
                            }
                        case WimCompressionType.Lzx:
                            {
                                compression = CompressionType.LZX;
                                break;
                            }
                        case WimCompressionType.None:
                            {
                                compression = CompressionType.NONE;
                                break;
                            }
                        case WimCompressionType.Xpress:
                            {
                                compression = CompressionType.XPRESS;
                                break;
                            }
                    }

                    using (Wim wim = Wim.CreateNewWim(compression))
                    {
                        wim.RegisterCallback(ProgressCallback);


                        string config = @"[ExclusionList]
\$ntfs.log
\hiberfil.sys
\pagefile.sys
\swapfile.sys
\System Volume Information";

                        var configpath = Path.GetTempFileName();
                        File.Delete(configpath);
                        File.WriteAllText(configpath, config);

                        wim.AddImage(InputDirectory, imageName, configpath, PreserveACL ? AddFlags.STRICT_ACLS : AddFlags.NO_ACLS);
                        if (!string.IsNullOrEmpty(imageDescription))
                            wim.SetImageDescription((int)wim.GetWimInfo().ImageCount, imageDescription);
                        if (!string.IsNullOrEmpty(imageDisplayName))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYNAME", imageDisplayName);
                        if (!string.IsNullOrEmpty(imageDisplayDescription))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYDESCRIPTION", imageDisplayDescription);
                        if (!string.IsNullOrEmpty(imageFlag))
                            wim.SetImageFlags((int)wim.GetWimInfo().ImageCount, imageFlag);
                        wim.Write(wimFile, Wim.AllImages, WriteFlags.DEFAULT, Wim.DefaultThreads);
                        File.Delete(configpath);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return ReseatWIMXml(wimFile);
        }

        public bool EnumerateFiles(string wimFile, int imageIndex, string path, out string[] entries)
        {
            List<string> fsentries = new List<string>();
            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    CallbackStatus IterateDirTreeCallback(DirEntry dentry, object userData)
                    {
                        fsentries.Add(dentry.FileName);
                        return CallbackStatus.CONTINUE;
                    };
                    wim.IterateDirTree(imageIndex, path, IterateFlags.CHILDREN, IterateDirTreeCallback);
                }
            }
            catch
            {
                entries = fsentries.ToArray();
                return false;
            }
            entries = fsentries.ToArray();
            return true;
        }

        public bool MarkImageAsBootable(string wimFile, int imageIndex)
        {
            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.DEFAULT))
                {
                    wim.SetWimInfo(new ManagedWimLib.WimInfo() { BootIndex = (uint)imageIndex }, ChangeFlags.BOOT_INDEX);
                    wim.Overwrite(WriteFlags.DEFAULT, Wim.DefaultThreads);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool GetWIMInformation(
            string wimFile,
            out WIMInformationXML.WIM wim)
        {
            wim = null;
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
                        var wiminfo = WimgApi.GetImageInformationAsString(wimHandle);
                        wim = WIMInformationXML.DeserializeWIM(wiminfo);
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

        private bool ReseatWIMXml(string wimFile)
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

                    string xmldata = WimgApi.GetImageInformationAsString(wimHandle);
                    var xml = WIMInformationXML.DeserializeWIM(xmldata);
                    xmldata = WIMInformationXML.SerializeWIM(xml);
                    WimgApi.SetImageInformation(wimHandle, xmldata);
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
