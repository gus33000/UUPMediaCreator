// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;
using USHORT = System.UInt16;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Contains a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            /// <summary>
            /// The low-order part of the file time.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
            public DWORD dwLowDateTime;

            /// <summary>
            /// The high-order part of the file time.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
            public DWORD dwHighDateTime;

            /// <summary>
            /// Initializes a new instance of the <see cref="FILETIME"/> struct.
            /// Creates a new instance of the FILETIME struct.
            /// </summary>
            /// <param name="dateTime">A <see cref="DateTime"/> object to copy data from.</param>
            public FILETIME(DateTime dateTime)
            {
                // Get the file time as a long in Utc
                long fileTime = dateTime.ToFileTimeUtc();

                // Copy the low bits
                dwLowDateTime = (DWORD)(fileTime & 0xFFFFFFFF);

                // Copy the high bits
                dwHighDateTime = (DWORD)(fileTime >> 32);
            }

            /// <summary>
            /// Converts a <see cref="FILETIME"/> to a <see cref="System.DateTime"/>
            /// </summary>
            /// <param name="fileTime">The <see cref="FILETIME"/> to convert.</param>
            public static implicit operator DateTime(FILETIME fileTime)
            {
                return fileTime.ToDateTime();
            }

            /// <summary>
            /// Converts a <see cref="System.DateTime"/> to a <see cref="FILETIME"/>.
            /// </summary>
            /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
            public static implicit operator FILETIME(DateTime dateTime)
            {
                return new FILETIME(dateTime);
            }

            /// <summary>
            /// Gets the current FILETIME as a <see cref="DateTime"/> object.
            /// </summary>
            /// <returns>A <see cref="DateTime"/> object that represents the FILETIME.</returns>
            public DateTime ToDateTime()
            {
                // Convert the file time to a long and then to a DateTime
                return DateTime.FromFileTimeUtc((long)dwHighDateTime << 32 | dwLowDateTime);
            }

            /// <summary>
            /// Converts the value of the current FILETIME object to its equivalent string representation.
            /// </summary>
            /// <returns>A string representation of value of the current FILETIME object</returns>
            /// <exception cref="ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
            public override string ToString()
            {
                // Call the DateTime.ToString() method
                return ((DateTime)this).ToString(CultureInfo.CurrentCulture);
            }

            /// <summary>
            /// Converts the value of the current FILETIME object to its equivalent string representation using the specified culture-specific format information.
            /// </summary>
            /// <param name="provider">An object that supplies culture-specific formatting information.</param>
            /// <returns>A string representation of value of the current FILETIME object as specified by provider.</returns>
            /// <exception cref="ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by provider.</exception>
            public string ToString(IFormatProvider provider)
            {
                // Call the DateTime.ToString() method
                return ((DateTime)this).ToString(provider);
            }

            /// <summary>
            /// Converts the value of the current DateTime object to its equivalent string representation using the specified format.
            /// </summary>
            /// <param name="format">A standard or custom date and time format string.</param>
            /// <returns>A string representation of value of the current DateTime object as specified by format.</returns>
            /// <exception cref="FormatException">The length of format is 1, and it is not one of the format specifier characters defined for DateTimeFormatInfo.
            /// -or-
            /// format does not contain a valid custom format pattern.</exception>
            /// <exception cref="ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
            public string ToString(string format)
            {
                // Call the DateTime.ToString() method
                return ((DateTime)this).ToString(format, CultureInfo.CurrentCulture);
            }

            /// <summary>
            /// Converts the value of the current DateTime object to its equivalent string representation using the specified format and culture-specific format information.
            /// </summary>
            /// <param name="format">A standard or custom date and time format string.</param>
            /// <param name="provider">An object that supplies culture-specific formatting information.</param>
            /// <returns>A string representation of value of the current DateTime object as specified by format and provider.</returns>
            /// <exception cref="FormatException">The length of format is 1, and it is not one of the format specifier characters defined for DateTimeFormatInfo.
            /// -or-
            /// format does not contain a valid custom format pattern.</exception>
            /// <exception cref="ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
            public string ToString(string format, IFormatProvider provider)
            {
                // Call the DateTime.ToString() method
                return ((DateTime)this).ToString(format, provider);
            }
        }

        /// <summary>
        /// Contains information retrieved by the WIMGetAttributes function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = WimgApi.WimgApiCharSet)]
        internal struct WIM_INFO
        {
            /// <summary>
            /// Specifies the full path to the .wim file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string WimPath;

            /// <summary>
            /// Specifies a GUID structure containing the unique identifier for the Windows® image (.wim) file.
            /// </summary>
            public Guid Guid;

            /// <summary>
            /// Specifies the number of images contained in the .wim file. This value is also returned by the WIMGetImageCount function.
            /// </summary>
            public DWORD ImageCount;

            /// <summary>
            /// Specifies the method of compression used to compress resources in the .wim file. See the WIMCreateFile function for the initial compression types.
            /// </summary>
            public WimCompressionType CompressionType;

            /// <summary>
            /// Specifies the part number of the current .wim file in a spanned set. This value should be one, unless the data of the .wim file was originally split by the WIMSplitFile function.
            /// </summary>
            public USHORT PartNumber;

            /// <summary>
            /// Specifies the total number of .wim file parts in a spanned set. This value must be one, unless the data of the .wim file was originally split via the WIMSplitFile function.
            /// </summary>
            public USHORT TotalParts;

            /// <summary>
            /// Specifies the index of the bootable image in the .wim file. If this value is zero, then there are no bootable images available. To set a bootable image, call the WIMSetBootImage function.
            /// </summary>
            public DWORD BootIndex;

            /// <summary>
            /// Specifies how the file is treated and what features will be used.
            /// </summary>
            public DWORD WimAttributes;

            /// <summary>
            /// Specifies the flags used during a WIMCreateFile function.
            /// </summary>
            public DWORD WimFlagsAndAttr;
        }

        /// <summary>
        /// Contains information retrieved by the WIMGetMountedImageInfo function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = WimgApi.WimgApiCharSet)]
        internal struct WIM_MOUNT_INFO_LEVEL0
        {
            /// <summary>
            /// Specifies the full path to the .wim file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string WimPath;

            /// <summary>
            /// Specifies the full path to the directory where the image is mounted.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string MountPath;

            /// <summary>
            /// Specifies the image index within the .wim file specified in WimPath.
            /// </summary>
            public DWORD ImageIndex;

            /// <summary>
            /// Specifies if the image was mounted with support for saving changes.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool MountedForRW;
        }

        /// <summary>
        /// Contains information retrieved by the WIMGetMountedImageList function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = WimgApi.WimgApiCharSet, Pack = 4)]
        internal struct WIM_MOUNT_INFO_LEVEL1
        {
            /// <summary>
            /// Specifies the full path to the .wim file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string WimPath;

            /// <summary>
            /// Specifies the full path to the directory where the image is mounted.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string MountPath;

            /// <summary>
            /// Specifies the image index within the .wim file specified in WimPath.
            /// </summary>
            public DWORD ImageIndex;

            /// <summary>
            /// Specifies the current state of the mount point.
            /// </summary>
            public WimMountPointState MountFlags;
        }

        /// <summary>
        /// Contains information retrieved by the WIMGetMountedImages function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = WimgApi.WimgApiCharSet, Pack = 4)]
        internal struct WIM_MOUNT_LIST
        {
            /// <summary>
            /// Specifies the full path to the .wim file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string WimPath;

            /// <summary>
            /// Specifies the full path to the directory where the image is mounted.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string MountPath;

            /// <summary>
            /// Specifies the image index within the .wim file specified in WimPath.
            /// </summary>
            public DWORD ImageIndex;

            /// <summary>
            /// Specifies if the image was mounted with support for saving changes.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool MountedForRW;
        }

        /// <summary>
        /// Contains information about the file that is found by the FindFirstFile, FindFirstFileEx, or FindNextFile function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct WIN32_FIND_DATA
        {
            /// <summary>
            /// The file attributes of a file.
            /// </summary>
            public FileAttributes FileAttributes;

            /// <summary>
            /// A <see cref="System.Runtime.InteropServices.ComTypes.FILETIME"/> structure that specifies when a file or directory was created.
            ///
            /// If the underlying file system does not support creation time, this member is zero.
            /// </summary>
            public FILETIME CreationTime;

            /// <summary>
            /// A <see cref="System.Runtime.InteropServices.ComTypes.FILETIME"/> structure.
            ///
            /// For a file, the structure specifies when the file was last read from, written to, or for executable files, run.
            ///
            /// For a directory, the structure specifies when the directory is created. If the underlying file system does not support last access time, this member is zero.
            ///
            /// On the FAT file system, the specified date for both files and directories is correct, but the time of day is always set to midnight.
            /// </summary>
            public FILETIME LastAccessTime;

            /// <summary>
            /// A <see cref="System.Runtime.InteropServices.ComTypes.FILETIME"/> structure.
            ///
            /// For a file, the structure specifies when the file was last written to, truncated, or overwritten, for example, when WriteFile or SetEndOfFile are used. The date and time are not updated when file attributes or security descriptors are changed.
            ///
            /// For a directory, the structure specifies when the directory is created. If the underlying file system does not support last write time, this member is zero.
            /// </summary>
            public FILETIME LastWriteTime;

            /// <summary>
            /// The high-order DWORD value of the file size, in bytes.
            ///
            /// This value is zero unless the file size is greater than MAXDWORD.
            ///
            /// The size of the file is equal to (FileSizeHigh * (MAXDWORD+1)) + FileSizeLow.
            /// </summary>
            public DWORD FileSizeHigh;

            /// <summary>
            /// The low-order DWORD value of the file size, in bytes.
            /// </summary>
            public DWORD FileSizeLow;

            /// <summary>
            /// If the FileAttributes member includes the FILE_ATTRIBUTE_REPARSE_POINT attribute, this member specifies the re-parse point tag.
            ///
            /// Otherwise, this value is undefined and should not be used.
            /// </summary>
            public DWORD Reserved0;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public DWORD Reserved1;

            /// <summary>
            /// The name of the file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;

            /// <summary>
            /// An alternative name for the file.
            ///
            /// This name is in the classic 8.3 file name format.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;

            /// <summary>
            /// Gets the file size by combining FileSizeLow and FileSizeHigh.
            /// </summary>
            public long FileSize => (FileSizeHigh * ((long)DWORD.MaxValue + 1)) + FileSizeLow;
        }
    }
}