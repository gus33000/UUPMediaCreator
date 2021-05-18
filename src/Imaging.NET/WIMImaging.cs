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
            return fileName.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, fileName.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse()) : "";
        }

        private static void InitNativeLibrary()
        {
            string libDir = Path.Combine(GetExecutableDirectory(), "runtimes");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libDir = Path.Combine(libDir, "win-");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libDir = Path.Combine(libDir, "linux-");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libDir = Path.Combine(libDir, "osx-");

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    libDir += "x86";
                    break;
                case Architecture.X64:
                    libDir += "x64";
                    break;
                case Architecture.Arm:
                    libDir += "arm";
                    break;
                case Architecture.Arm64:
                    libDir += "arm64";
                    break;
            }
            libDir = Path.Combine(libDir, "native");

            string libPath = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libPath = Path.Combine(libDir, "libwim-15.dll");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libPath = Path.Combine(libDir, "libwim.so");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libPath = Path.Combine(libDir, "libwim.dylib");

            if (libPath == null)
            {
                Console.WriteLine($"Unable to find native library.");
                throw new PlatformNotSupportedException($"Unable to find native library.");
            }

            if (!File.Exists(libPath))
            {
                libDir = GetExecutableDirectory();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    libPath = Path.Combine(libDir, "libwim-15.dll");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    libPath = Path.Combine(libDir, "libwim.so");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    libPath = Path.Combine(libDir, "libwim.dylib");

                if (!File.Exists(libPath))
                {
                    Console.WriteLine($"Unable to find native library [{libPath}].");
                    throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");
                }
            }

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
            destination = destination.Replace(Path.DirectorySeparatorChar, '\\');

            string title = $"Adding {destination} to {wimFile.Split(Path.DirectorySeparatorChar).Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.WriteMetadataBegin:
                            break;

                        case ProgressMsg.UpdateBeginCommand:
                            break;

                        case ProgressMsg.UpdateEndCommand:
                            break;

                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.WriteMetadataEnd:
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.WriteAccess))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        UpdateCommand.SetAdd(fileToAdd, destination, null, AddFlags.None),
                        UpdateFlags.SendProgress);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
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
            fileToRemove = fileToRemove.Replace(Path.DirectorySeparatorChar, '\\');

            string title = $"Removing {fileToRemove} from {wimFile.Split(Path.DirectorySeparatorChar).Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.WriteMetadataBegin:
                            break;

                        case ProgressMsg.UpdateBeginCommand:
                            break;

                        case ProgressMsg.UpdateEndCommand:
                            break;

                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.WriteMetadataEnd:
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        UpdateCommand.SetDelete(fileToRemove, DeleteFlags.None),
                        UpdateFlags.SendProgress);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
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
            string title = $"Exporting {wimFile.Split(Path.DirectorySeparatorChar).Last()} - Index {imageIndex}";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.WriteMetadataBegin:
                            break;

                        case ProgressMsg.WriteMetadataEnd:
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim srcWim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    string imageName = srcWim.GetImageName(imageIndex);
                    string imageDescription = srcWim.GetImageDescription(imageIndex);

                    var compression = CompressionType.None;
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
                                compression = CompressionType.None;
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
                        srcWim.ReferenceResourceFiles(referenceWIMs, RefFlags.None, OpenFlags.None);
                    }

                    if (File.Exists(destinationWimFile))
                    {
                        using (Wim destWim = Wim.OpenWim(destinationWimFile, OpenFlags.WriteAccess))
                        {
                            destWim.RegisterCallback(ProgressCallback);

                            if (destWim.IsImageNameInUse(imageName))
                            {
                                srcWim.ExportImage(imageIndex, destWim, imageName + " " + DateTime.UtcNow.ToString(), imageDescription, ExportFlags.None);
                            }
                            else
                            {
                                srcWim.ExportImage(imageIndex, destWim, imageName, imageDescription, ExportFlags.None);
                            }

                            destWim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
                        }
                    }
                    else
                    {
                        using (Wim destWim = Wim.CreateNewWim(compression))
                        {
                            destWim.RegisterCallback(ProgressCallback);
                            srcWim.ExportImage(imageIndex, destWim, imageName, imageDescription, ExportFlags.None);
                            destWim.Write(destinationWimFile, Wim.AllImages, compression == CompressionType.LZMS ? WriteFlags.Solid : WriteFlags.None, Wim.DefaultThreads);
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
            var filename = Path.GetFileName(fileToExtract.Replace('\\', Path.DirectorySeparatorChar));
            var extractDir = Path.GetTempPath();
            fileToExtract = fileToExtract.Replace(Path.DirectorySeparatorChar, '\\');

            try
            {

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.ExtractPath(imageIndex, extractDir, fileToExtract, ExtractFlags.NoPreserveDirStructure);
                }
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                File.Move(Path.Combine(extractDir, filename), destination);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        public bool RenameFileInImage(string wimFile, int imageIndex, string sourceFilePath, string destinationFilePath, IImaging.ProgressCallback progressCallback = null)
        {
            sourceFilePath = sourceFilePath.Replace(Path.DirectorySeparatorChar, '\\');
            destinationFilePath = destinationFilePath.Replace(Path.DirectorySeparatorChar, '\\');

            string title = $"Renaming {sourceFilePath} to {destinationFilePath} in {wimFile.Split(Path.DirectorySeparatorChar).Last()}...";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.WriteMetadataBegin:
                            break;

                        case ProgressMsg.UpdateBeginCommand:
                            break;

                        case ProgressMsg.UpdateEndCommand:
                            break;

                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.WriteMetadataEnd:
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.UpdateImage(
                        imageIndex,
                        UpdateCommand.SetRename(sourceFilePath, destinationFilePath),
                        UpdateFlags.SendProgress);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
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
            string title = $"Applying {wimFile.Split(Path.DirectorySeparatorChar).Last()} - Index {imageIndex}";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ExtractImageBegin:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                            }
                            break;

                        case ProgressMsg.ExtractImageEnd:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                            }
                            break;

                        case ProgressMsg.ExtractFileStructure:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                progressCallback?.Invoke($"Applying file structure ({(int)Math.Round((double)m.CurrentFileCount / m.EndFileCount * 100)}%)", 0, true);
                            }
                            break;

                        case ProgressMsg.ExtractStreams:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.ExtractMetadata:
                            {
                                ExtractProgress m = (ExtractProgress)info;
                                progressCallback?.Invoke($"Applying metadata ({(int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100)}%)", 0, true);
                            }
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.RegisterCallback(ProgressCallback);
                    if (referenceWIMs != null && referenceWIMs.Count() > 0)
                        wim.ReferenceResourceFiles(referenceWIMs, RefFlags.None, OpenFlags.None);
                    wim.ExtractImage(imageIndex, OutputDirectory, PreserveACL ? ExtractFlags.StrictAcls : ExtractFlags.NoAcls);
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
            string title = $"Creating {imageName} ({wimFile.Split(Path.DirectorySeparatorChar).Last()})";
            try
            {
                CallbackStatus ProgressCallback(ProgressMsg msg, object info, object progctx)
                {
                    switch (msg)
                    {
                        case ProgressMsg.ScanBegin:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.ScanDEntry:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.ScanEnd:
                            {
                                ScanProgress m = (ScanProgress)info;
                                progressCallback?.Invoke($"Scanning files ({m.NumBytesScanned} bytes scanned, {m.NumDirsScanned} Directories, {m.NumNonDirsScanned} Files, Current directory: {m.CurPath})", 0, true);
                            }
                            break;

                        case ProgressMsg.WriteMetadataBegin:
                            break;

                        case ProgressMsg.WriteStreams:
                            {
                                WriteStreamsProgress m = (WriteStreamsProgress)info;
                                progressCallback?.Invoke(title, (int)Math.Round((double)m.CompletedBytes / m.TotalBytes * 100), false);
                            }
                            break;

                        case ProgressMsg.WriteMetadataEnd:
                            break;
                    }
                    return CallbackStatus.Continue;
                }

                if (File.Exists(wimFile))
                {
                    using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.WriteAccess))
                    {
                        wim.RegisterCallback(ProgressCallback);
                        wim.AddImage(InputDirectory, imageName, null, PreserveACL ? AddFlags.StrictAcls : AddFlags.NoAcls);
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
                        wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
                    }
                }
                else
                {
                    var compression = CompressionType.None;
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
                                compression = CompressionType.None;
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

                        wim.AddImage(InputDirectory, imageName, configpath, PreserveACL ? AddFlags.StrictAcls : AddFlags.NoAcls);
                        if (!string.IsNullOrEmpty(imageDescription))
                            wim.SetImageDescription((int)wim.GetWimInfo().ImageCount, imageDescription);
                        if (!string.IsNullOrEmpty(imageDisplayName))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYNAME", imageDisplayName);
                        if (!string.IsNullOrEmpty(imageDisplayDescription))
                            wim.SetImageProperty((int)wim.GetWimInfo().ImageCount, "DISPLAYDESCRIPTION", imageDisplayDescription);
                        if (!string.IsNullOrEmpty(imageFlag))
                            wim.SetImageFlags((int)wim.GetWimInfo().ImageCount, imageFlag);
                        wim.Write(wimFile, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);
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
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    int IterateDirTreeCallback(DirEntry dentry, object userData)
                    {
                        fsentries.Add(dentry.FileName);
                        return 0;
                    };
                    wim.IterateDirTree(imageIndex, path, IterateDirTreeFlags.Children, IterateDirTreeCallback);
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
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    wim.SetWimInfo(new ManagedWimLib.WimInfo() { BootIndex = (uint)imageIndex }, ChangeFlags.BootIndex);
                    wim.Overwrite(WriteFlags.None, Wim.DefaultThreads);
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
                    WimgApi.SetTemporaryPath(wimHandle, Path.GetTempPath());

                    try
                    {
                        var wiminfo = WimgApi.GetImageInformationAsString(wimHandle);
                        wim = WIMInformationXML.DeserializeWIM(wiminfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + " - " + ex.ToString());
                    }
                    finally
                    {
                    }
                }
            }
            catch
            {
                return GetWIMInformation2(wimFile, out wim);
            }
            return true;
        }

        public bool GetWIMInformation2(
            string wimFile,
            out WIMInformationXML.WIM wim)
        {
            wim = null;
            try
            {
                using (Wim wiml = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    string xmldata = string.Join("", wiml.GetXmlData().Skip(1));
                    wim = WIMInformationXML.DeserializeWIM(xmldata);
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
                    WimgApi.SetTemporaryPath(wimHandle, Path.GetTempPath());

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
                return GetWIMImageInformation2(wimFile, imageIndex, out image);
            }
            return true;
        }

        public bool GetWIMImageInformation2(
            string wimFile,
            int imageIndex,
            out WIMInformationXML.IMAGE image)
        {
            image = null;
            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.None))
                {
                    string xmldata = string.Join("", wim.GetXmlData().Skip(1));
                    var xml = WIMInformationXML.DeserializeWIM(xmldata);
                    image = xml.IMAGE.First(x => x.INDEX == imageIndex.ToString());
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
                    WimgApi.SetTemporaryPath(wimHandle, Path.GetTempPath());

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
                return SetWIMImageInformation2(wimFile, imageIndex, image);
            }
            return true;
        }

        public bool SetWIMImageInformation2(
            string wimFile,
            int imageIndex,
            WIMInformationXML.IMAGE image)
        {
            image = null;
            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.WriteAccess))
                {
                    string xmldata = string.Join("", wim.GetXmlData().Skip(1));
                    var xml = WIMInformationXML.DeserializeWIM(xmldata);
                    var index = xml.IMAGE.IndexOf(xml.IMAGE.First(x => x.INDEX == imageIndex.ToString()));
                    xml.IMAGE[index] = image;
                    xmldata = WIMInformationXML.SerializeWIM(xml);
                    // TODO
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
                    WimgApi.SetTemporaryPath(wimHandle, Path.GetTempPath());

                    string xmldata = WimgApi.GetImageInformationAsString(wimHandle);
                    var xml = WIMInformationXML.DeserializeWIM(xmldata);
                    xmldata = WIMInformationXML.SerializeWIM(xml);
                    WimgApi.SetImageInformation(wimHandle, xmldata);
                }
            }
            catch
            {
                return ReseatWIMXml2(wimFile);
            }
            return true;
        }

        private bool ReseatWIMXml2(string wimFile)
        {
            try
            {
                using (Wim wim = Wim.OpenWim(wimFile, OpenFlags.WriteAccess))
                {
                    string xmldata = string.Join("", wim.GetXmlData().Skip(1));
                    var xml = WIMInformationXML.DeserializeWIM(xmldata);
                    xmldata = WIMInformationXML.SerializeWIM(xml);
                    // TODO
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