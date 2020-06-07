using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ManagedWimLib;

namespace WimLib.NET
{
    /// <summary>
    /// Specifies a compression type.
    ///
    /// A WIM file has a default compression type, indicated by its file header.
    /// Normally, each resource in the WIM file is compressed with this compression type.
    /// However, resources may be stored as uncompressed; for example,
    /// wimlib may do so if a resource does not compress to less than its original size. 
    /// In addition, a WIM with the new version number of 3584, or "ESD file",
    /// might contain solid resources with different compression types.
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// No compression.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// The XPRESS compression format.
        /// This format combines Lempel-Ziv factorization with Huffman encoding.
        /// Compression and decompression are both fast. 
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^12 and 2^16, inclusively.
        /// </summary>
        XPRESS = 1,
        /// <summary>
        /// The LZX compression format.
        /// This format combines Lempel-Ziv factorization with Huffman encoding, but with more features and complexity than XPRESS.
        /// Compression is slow to somewhat fast, depending on the settings.
        /// Decompression is fast but slower than XPRESS.
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^15 and 2^21, inclusively.
        /// Note: chunk sizes other than 2^15 are not compatible with the Microsoft implementation.
        /// </summary>
        LZX = 2,
        /// <summary>
        /// The LZMS compression format.
        /// This format combines Lempel-Ziv factorization with adaptive Huffman encoding and range coding.
        /// Compression and decompression are both fairly slow.
        /// 
        /// This format supports chunk sizes that are powers of 2 between 2^15 and 2^30, inclusively.
        /// This format is best used for large chunk sizes.
        /// </summary>
        LZMS = 3,
    }

    [Flags]
    public enum ExportFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// If a single image is being exported, mark it bootable in the destination WIM.
        /// Alternatively, if Wim.AllImages is specified as the image to export,
        /// the image in the source WIM (if any) that is marked as bootable is also
        /// marked as bootable in the destination WIM.
        /// </summary>
        BOOT = 0x00000001,
        /// <summary>
        /// Give the exported image(s) no names. 
        /// Avoids problems with image name collisions.
        /// </summary>
        NO_NAMES = 0x00000002,
        /// <summary>
        /// Give the exported image(s) no descriptions.
        /// </summary>
        NO_DESCRIPTIONS = 0x00000004,
        /// <summary>
        /// This advises the library that the program is finished with the source
        /// WIMStruct and will not attempt to access it after the call to
        /// Wim.ExportImage(), with the exception of the call to Wim.Free().
        /// </summary>
        GIFT = 0x00000008,
        /// <summary>
        /// Mark each exported image as WIMBoot-compatible.
        ///
        /// Note: by itself, this does change the destination WIM's compression type, nor
        /// does it add the file "\Windows\System32\WimBootCompress.ini" in the WIM image.  
        /// </summary>
        /// <remarks>
        /// Before writing the destination WIM, it's recommended to do something like:
        ///
        /// using (Wim wim = ...)
        /// {
        ///     wim.SetOutputCompressionType(wim, CompressType.XPRESS);
        ///     wim.SetOutputChunkSize(wim, 4096);
        ///     wim.AddTree(image, "myconfig.ini", @"\Windows\System32\WimBootCompress.ini", AddFlags.DEFAULT);
        /// }
        /// </remarks>
        WIMBOOT = 0x00000010,
    }

    [Flags]
    public enum AddFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// UNIX-like systems only:
        /// Directly capture an NTFS volume rather than a generic directory.
        /// This requires that wimlib was compiled with support for libntfs-3g.
        ///
        /// This flag cannot be combined with AddFlags.DEREFERENCE or AddFlags.UNIX_DATA.
        ///
        /// Do not use this flag on Windows,
        /// where wimlib already supports all Windows-native filesystems, including NTFS, through the Windows APIs.
        /// </summary>
        NTFS = 0x00000001,
        /// <summary>
        /// Follow symbolic links when scanning the directory tree.
        /// Currently only supported on UNIX-like systems.
        /// </summary>
        DEREFERENCE = 0x00000002,
        /// <summary>
        /// Call the progress function with the message ProgressMsg.SCAN_DENTRY when each directory or file has been scanned.
        /// </summary>
        VERBOSE = 0x00000004,
        /// <summary>
        /// Mark the image being added as the bootable image of the WIM.
        /// This flag is valid only for Wim.AddImage() and Wim.AddImageMultisource().
        ///
        /// Note that you can also change the bootable image of a WIM using Wim.SetWimInfo().
        ///
        /// Note: AddFlags.BOOT does something different from, and independent from, AddFlags.WIMBOOT.
        /// </summary>
        BOOT = 0x00000008,
        /// <summary>
        /// UNIX-like systems only:
        /// Store the UNIX owner, group, mode, and device ID (major and minor number) of each file.
        /// In addition, capture special files such as device nodes and FIFOs. 
        /// Since wimlib v1.11.0, on Linux also capture extended attributes.
        /// See the documentation for the "--unix-data" option to wimcapture for more information.
        /// </summary>
        UNIX_DATA = 0x00000010,
        /// <summary>
        /// Do not capture security descriptors.
        /// Only has an effect in NTFS-3G capture mode, or in Windows native builds.
        /// </summary>
        NO_ACLS = 0x00000020,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory cannot be accessed.  
        /// Only has an effect in Windows native builds.
        /// The default behavior without this flag is to first try omitting the SACL from the security descriptor,
        /// then to try omitting the security descriptor entirely.
        /// </summary>
        STRICT_ACLS = 0x00000040,
        /// <summary>
        /// Call the progress function with the message ProgressMsg.SCAN_DENTRY when a directory or file is excluded from capture.
        /// This is a subset of the messages provided by AddFlags.VERBOSE.
        /// </summary>
        EXCLUDE_VERBOSE = 0x00000080,
        /// <summary>
        /// Reparse-point fixups:
        /// Modify absolute symbolic links (and junctions, in the case of Windows) that point inside the directory
        /// being captured to instead be absolute relative to the directory being captured.
        ///
        /// Without this flag, the default is to do reparse-point fixups if WIM_HDR_FLAG_RP_FIX is set in the WIM header
        /// or if this is the first image being added.
        /// </summary>
        RPFIX = 0x00000100,
        /// <summary>
        /// Don't do reparse point fixups. See AddFlags.RPFIX.
        /// </summary>
        NORPFIX = 0x00000200,
        /// <summary>
        /// Do not automatically exclude unsupported files or directories from capture,
        /// such as encrypted files in NTFS-3G capture mode, or device files and FIFOs on
        /// UNIX-like systems when not also using AddFlags.UNIX_DATA.  
        /// Instead, fail with ErrorCode.UNSUPPORTED_FILE when such a file is encountered.
        /// </summary>
        NO_UNSUPPORTED_EXCLUDE = 0x00000400,
        /// <summary>
        /// Automatically select a capture configuration appropriate for capturing filesystems containing Windows operating systems.
        /// For example, "/pagefile.sys" and "/System Volume Information" will be excluded.
        ///
        /// When this flag is specified, the corresponding config parameter (for Wim.AddImage()) or member (for Wim.UpdateImage()) must be null.
        /// Otherwise, ErrorCode.INVALID_PARAM will be returned.
        ///
        /// Note that the default behavior ---that is, when neither AddFlags.WINCONFIG nor AddFlags.WIMBOOT is specified and config is null---
        /// is to use no capture configuration, meaning that no files are excluded from capture.
        /// </summary>
        WINCONFIG = 0x00000800,
        /// <summary>
        /// Capture image as "WIMBoot compatible". 
        /// In addition, if no capture configuration file is explicitly specified use the capture configuration file
        /// "$SOURCE/Windows/System32/WimBootCompress.ini" if it exists, where "$SOURCE" is the directory being captured;
        /// or, if a capture configuration file is explicitly specified, use it and also place it at
        /// "/Windows/System32/WimBootCompress.ini" in the WIM image.
        ///
        /// This flag does not, by itself, change the compression type or chunk size.
        /// Before writing the WIM file, you may wish to set the compression format to be the same as that used by WIMGAPI and DISM:
        ///
        /// wimlib_set_output_compression_type(wim, CompressType.XPRESS);
        /// wimlib_set_output_chunk_size(wim, 4096);
        ///
        /// However, "WIMBoot" also works with other XPRESS chunk sizes as well as LZX with 32768 byte chunks.
        ///
        /// Note: AddFlags.WIMBOOT does something different from, and independent from, AddFlags.BOOT.
        ///
        /// Since wimlib v1.8.3, AddFlags.WIMBOOT also causes offline WIM-backed files to be added as the "real" files
        /// rather than as their reparse points, provided that their data is already present in the WIM. 
        /// This feature can be useful when updating a backing WIM file in an "offline" state.
        /// </summary>
        WIMBOOT = 0x00001000,
        /// <summary>
        /// If the add command involves adding a non-directory file to a location at which there already exists
        /// a nondirectory file in the image, issue ErrorCode.INVALID_OVERLAY instead of replacing the file.
        /// This was the default behavior before wimlib v1.7.0.
        /// </summary>
        NO_REPLACE = 0x00002000,
        /// <summary>
        /// Send ProgressMsg.TEST_FILE_EXCLUSION messages to the progress function.
        ///
        /// Note: This method for file exclusions is independent from the capture configuration file mechanism.
        /// </summary>
        TEST_FILE_EXCLUSION = 0x00004000,
        /// <summary>
        /// Since wimlib v1.9.0: create a temporary filesystem snapshot of the source directory and add the files from it.
        /// Currently, this option is only supported on Windows, where it uses the Volume Shadow Copy Service (VSS).
        /// Using this option, you can create a consistent backup of the system volume of
        /// a running Windows system without running into problems with locked files.
        /// For the VSS snapshot to be successfully created, your application must be run as an Administrator, 
        /// and it cannot be run in WoW64 mode (i.e. if Windows is 64-bit, then your application must be 64-bit as well).
        /// </summary>
        SNAPSHOT = 0x00008000,
        /// <summary>
        /// Since wimlib v1.9.0: permit the library to discard file paths after the initial scan. 
        /// If the application won't use WriteFlags.SEND_DONE_WITH_FILE_MESSAGES while writing the WIM archive, 
        /// this flag can be used to allow the library to enable optimizations such as opening files by inode number rather than by path.
        /// Currently this only makes a difference on Windows.
        /// </summary>
        FILE_PATHS_UNNEEDED = 0x00010000,
    }

    public enum RefFlags : int
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// For Wim.ReferenceResourceFiles(), enable shell-style filename globbing.
        /// Ignored by Wim.ReferenceResources().
        /// </summary>
        GLOB_ENABLE = 0x00000001,
        /// <summary>
        /// For Wim.ReferenceResourceFiles(), issue an error (ErrorCode.GLOB_HAD_NO_MATCHES) if a glob did not match any files. 
        /// The default behavior without this flag is to issue no error at that point, but then attempt to open
        /// the glob as a literal path, which of course will fail anyway if no file exists at that path. 
        /// No effect if RefFlags.GLOB_ENABLE is not also specified.
        /// Ignored by Wim.ReferenceResources().
        /// </summary>
        GLOB_ERR_ON_NOMATCH = 0x00000002,
    }

    [Flags]
    public enum ExtractFlags : uint
    {
        DEFAULT = 0x00000000,
        /// <summary>
        /// Extract the image directly to an NTFS volume rather than a generic directory.
        /// This mode is only available if wimlib was compiled with libntfs-3g support;
        /// if not, ErrorCode.UNSUPPORTED will be returned.
        /// In this mode, the extraction target will be interpreted as the path to an NTFS volume image
        /// (as a regular file or block device) rather than a directory.
        /// It will be opened using libntfs-3g, and the image will be extracted to the NTFS filesystem's root directory.
        /// Note: this flag cannot be used when Wim.ExtractImage() is called with Wim.AllImages as the image,
        /// nor can it be used with Wim.ExtractPaths() when passed multiple paths.
        /// </summary>
        NTFS = 0x00000001,
        /// <summary>
        /// UNIX-like systems only:
        /// Extract UNIX-specific metadata captured with AddFlags.UNIX_DATA.
        /// </summary>
        UNIX_DATA = 0x00000020,
        /// <summary>
        /// Do not extract security descriptors.
        /// This flag cannot be combined with ExtractFlags.STRICT_ACLS.
        /// </summary>
        NO_ACLS = 0x00000040,
        /// <summary>
        /// Fail immediately if the full security descriptor of any file or directory
        /// cannot be set exactly as specified in the WIM image.
        /// On Windows, the default behavior without this flag when wimlib does not have permission to set the
        /// correct security descriptor is to fall back to setting the security descriptor with the SACL omitted,
        /// then with the DACL omitted, then with the owner omitted, then not at all.
        /// This flag cannot be combined with ExtractFlags.NO_ACLS.
        /// </summary>
        STRICT_ACLS = 0x00000080,
        /// <summary>
        /// This is the extraction equivalent to AddFlags.RPFIX.
        /// This forces reparse-point fixups on, so absolute symbolic links or junction points will
        /// be fixed to be absolute relative to the actual extraction root.
        /// Reparse-point fixups are done by default for Wim.ExtractImage() and Wim.ExtractImageFromPipe()
        /// if WIM_HDR_FLAG_RP_FIX is set in the WIM header.
        /// This flag cannot be combined with ExtractFlags.NORPFIX.
        /// </summary>
        RPFIX = 0x00000100,
        /// <summary>
        /// Force reparse-point fixups on extraction off, regardless of the state of the WIM_HDR_FLAG_RP_FIX flag in the WIM header.
        /// This flag cannot be combined with ExtractFlags.RPFIX.
        /// </summary>
        NORPFIX = 0x00000200,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only:
        /// Extract the paths, each of which must name a regular file, to standard output.
        /// </summary>
        TO_STDOUT = 0x00000400,
        /// <summary>
        /// Instead of ignoring files and directories with names that cannot be represented on the current platform
        /// (note: Windows has more restrictions on filenames than POSIX-compliant systems),
        /// try to replace characters or append junk to the names so that they can be extracted in some form.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        REPLACE_INVALID_FILENAMES = 0x00000800,
        /// <summary>
        /// On Windows, when there exist two or more files with the same case insensitive name but different case sensitive names,
        /// try to extract them all by appending junk to the end of them, rather than arbitrarily extracting only one.
        ///
        /// Note: this flag is unlikely to have any effect when extracting a WIM image that was captured on Windows.
        /// </summary>
        ALL_CASE_CONFLICTS = 0x00001000,
        /// <summary>
        /// Do not ignore failure to set timestamps on extracted files.
        /// This flag currently only has an effect when extracting to a directory on UNIX-like systems.
        /// </summary>
        STRICT_TIMESTAMPS = 0x00002000,
        /// <summary>
        /// Do not ignore failure to set short names on extracted files.
        /// This flag currently only has an effect on Windows.
        /// </summary>
        STRICT_SHORT_NAMES = 0x00004000,
        /// <summary>
        /// Do not ignore failure to extract symbolic links and junctions due to permissions problems.
        /// This flag currently only has an effect on Windows. 
        /// By default, such failures are ignored since the default configuration of Windows 
        /// only allows the Administrator to create symbolic links.
        /// </summary>
        STRICT_SYMLINKS = 0x00008000,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only:
        /// Treat the paths to extract as wildcard patterns ("globs") which may contain the wildcard characters '?' and '*'.
        /// The '?' character matches any non-path-separator character, whereas the '*' character matches zero or more
        /// non-path-separator characters.
        /// Consequently, each glob may match zero or more actual paths in the WIM image.
        ///
        /// By default, if a glob does not match any files, a warning but not an error will be issued.
        /// This is the case even if the glob did not actually contain wildcard characters. 
        /// Use ExtractFlags.STRICT_GLOB to get an error instead.
        /// </summary>
        GLOB_PATHS = 0x00040000,
        /// <summary>
        /// In combination with ExtractFlags.GLOB_PATHS, causes an error (ErrorCode.PATH_DOES_NOT_EXIST)
        /// rather than a warning to be issued when one of the provided globs did not match a file.
        /// </summary>
        STRICT_GLOB = 0x00080000,
        /// <summary>
        /// Do not extract Windows file attributes such as readonly, hidden, etc.
        ///
        /// This flag has an effect on Windows as well as in the NTFS-3G extraction mode.
        /// </summary>
        NO_ATTRIBUTES = 0x00100000,
        /// <summary>
        /// For Wim.ExtractPaths() and Wim.ExtractPathList() only: 
        /// Do not preserve the directory structure of the archive when extracting --- that is,
        /// place each extracted file or directory tree directly in the target directory.
        /// The target directory will still be created if it does not already exist.
        /// </summary>
        NO_PRESERVE_DIR_STRUCTURE = 0x00200000,
        /// <summary>
        /// Windows only: Extract files as "pointers" back to the WIM archive.
        ///
        /// The effects of this option are fairly complex.
        /// See the documentation for the "--wimboot" option of "wimapply" for more information.
        /// </summary>
        WIMBOOT = 0x00400000,
        /// <summary>
        /// Since wimlib v1.8.2 and Windows-only:
        /// compress the extracted files using System Compression, when possible. 
        /// This only works on either Windows 10 or later, or on an older Windows to which Microsoft's wofadk.sys driver has been added.
        /// Several different compression formats may be used with System Compression;
        /// this particular flag selects the XPRESS compression format with 4096 byte chunks.
        /// </summary>
        COMPACT_XPRESS4K = 0x01000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use XPRESS compression with 8192 byte chunks.
        /// </summary>
        COMPACT_XPRESS8K = 0x02000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use XPRESS compression with 16384 byte chunks.
        /// </summary>
        COMPACT_XPRESS16K = 0x04000000,
        /// <summary>
        /// Like ExtractFlags.COMPACT_XPRESS4K, but use LZX compression with 32768 byte chunks.
        /// </summary>
        COMPACT_LZX = 0x08000000,
    }

    public class WIMHelper : IDisposable
    {
        public delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

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

            var runningDirectory = Process.GetCurrentProcess().MainModule.FileName.Contains("\\") ? string.Join("\\", Process.GetCurrentProcess().MainModule.FileName.Split('\\').Reverse().Skip(1).Reverse()) : "";

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

        public WIMHelper()
        {
            InitNativeLibrary();
        }

        public bool ApplyImage(string wimFile, int imageIndex, string Output, IEnumerable<string> referenceWIMs = null, ExtractFlags extractFlags = ExtractFlags.DEFAULT, RefFlags refFlags = RefFlags.DEFAULT, ProgressCallback progressCallback = null)
        {
            string title = $"Applying {wimFile.Split('\\').Last()}...";
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
                        wim.ReferenceResourceFiles(referenceWIMs, (ManagedWimLib.RefFlags)refFlags, OpenFlags.DEFAULT);
                    wim.ExtractImage(imageIndex, Output, (ManagedWimLib.ExtractFlags)extractFlags);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool CaptureImage(string wimFile, string imageName, string imageDescription, string InputDirectory, CompressionType compressionType = CompressionType.LZX, AddFlags addFlags = AddFlags.DEFAULT, ProgressCallback progressCallback = null)
        {
            string title = $"Creating {wimFile.Split('\\').Last()}";
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

                using (Wim wim = Wim.CreateNewWim((ManagedWimLib.CompressionType)compressionType))
                {
                    wim.RegisterCallback(ProgressCallback);
                    wim.AddImage(InputDirectory, imageName, null, (ManagedWimLib.AddFlags)addFlags);
                    if (!string.IsNullOrEmpty(imageDescription))
                        wim.SetImageDescription(1, imageDescription);
                    wim.Write(wimFile, Wim.AllImages, WriteFlags.DEFAULT, Wim.DefaultThreads);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool ExportImage(string wimFile, int imageIndex, string outputWIMFile, CompressionType compressionType = CompressionType.LZX, ExportFlags exportFlags = ExportFlags.DEFAULT, ProgressCallback progressCallback = null)
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

                    using (Wim destWim = Wim.CreateNewWim((ManagedWimLib.CompressionType)compressionType))
                    {
                        destWim.RegisterCallback(ProgressCallback);
                        srcWim.ExportImage(imageIndex, destWim, imageName, null, (ManagedWimLib.ExportFlags)exportFlags);
                        destWim.Write(outputWIMFile, Wim.AllImages, WriteFlags.DEFAULT, Wim.DefaultThreads);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            Wim.GlobalCleanup();
        }
    }
}