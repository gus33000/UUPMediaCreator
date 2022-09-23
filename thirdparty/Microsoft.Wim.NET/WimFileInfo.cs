// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    /// <summary>
    /// Provides properties of files contained in a Windows® image (.wim). This class cannot be inherited.
    /// </summary>
    public sealed class WimFileInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WimFileInfo"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to the file or directory.</param>
        /// <param name="findData">A <see cref="WimgApi.WIN32_FIND_DATA"/> containing information about the file or directory.</param>
        internal WimFileInfo(string fullPath, WimgApi.WIN32_FIND_DATA findData)
        {
            // Save the full name
            FullName = fullPath;

            // Determine the name
            Name = Path.GetFileName(FullName);

            // Copy other data from the WIN32_FIND_DATA struct
            Attributes = findData.FileAttributes;
            CreationTimeUtc = findData.CreationTime.ToDateTime();
            LastAccessTimeUtc = findData.LastAccessTime.ToDateTime();
            LastWriteTimeUtc = findData.LastWriteTime.ToDateTime();

            // Determine the file size
            Length = ((long)findData.FileSizeHigh << 32) | findData.FileSizeLow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WimFileInfo"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to the file or directory.</param>
        /// <param name="findDataPtr">A pointer to a <see cref="WimgApi.WIN32_FIND_DATA"/> containing information about the file or directory.</param>
        internal WimFileInfo(string fullPath, IntPtr findDataPtr)
            : this(fullPath, (WimgApi.WIN32_FIND_DATA)Marshal.PtrToStructure(findDataPtr, typeof(WimgApi.WIN32_FIND_DATA)))
        {
        }

        /// <summary>
        /// Gets the attributes for the current file or directory.
        /// </summary>
        public FileAttributes Attributes { get; }

        /// <summary>
        /// Gets the creation time of the current file or directory.
        /// </summary>
        public DateTime CreationTime => CreationTimeUtc.ToLocalTime();

        /// <summary>
        /// Gets the creation time, in coordinated universal time (UTC), of the current file or directory.
        /// </summary>
        public DateTime CreationTimeUtc { get; }

        /// <summary>
        /// Gets a string representing the directory's full path.
        /// </summary>
        public string DirectoryName => Path.GetDirectoryName(FullName);

        /// <summary>
        /// Gets the string representing the extension part of the file.
        /// </summary>
        public string Extension => Path.GetExtension(Name);

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the time the current file or directory was last accessed.
        /// </summary>
        public DateTime LastAccessTime => LastAccessTimeUtc.ToLocalTime();

        /// <summary>
        /// Gets the time, in coordinated universal time (UTC), that the current file or directory was last accessed.
        /// </summary>
        public DateTime LastAccessTimeUtc { get; }

        /// <summary>
        /// Gets the time the current file or directory was last written to.
        /// </summary>
        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        /// <summary>
        /// Gets the time, in coordinated universal time (UTC), that the current file or directory was last written to.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Gets the size, in bytes, of the current file.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name { get; }
    }
}