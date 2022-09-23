// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    /// <summary>
    /// Specifies how a .wim file is treated and what features will be used.
    /// </summary>
    [Flags]
    public enum WimInfoAttributes : uint
    {
        /// <summary>
        /// The .wim file only contains image resources and XML information.
        /// </summary>
        MetadataOnly = WimgApi.WIM_ATTRIBUTE_METADATA_ONLY,

        /// <summary>
        /// The .wim file does not have any other attributes set.
        /// </summary>
        Normal = WimgApi.WIM_ATTRIBUTE_NORMAL,

        /// <summary>
        /// The .wim file is locked and cannot be modified.
        /// </summary>
        ReadOnly = WimgApi.WIM_ATTRIBUTE_READONLY,

        /// <summary>
        /// The .wim file only contains file resources and no images or metadata.
        /// </summary>
        ResourceOnly = WimgApi.WIM_ATTRIBUTE_RESOURCE_ONLY,

        /// <summary>
        /// The .wim file contains one or more images where symbolic link or junction path fix-up is enabled.
        /// </summary>
        RPFix = WimgApi.WIM_ATTRIBUTE_RP_FIX,

        /// <summary>
        /// The .wim file has been split into multiple pieces by the <see cref="WimgApi.SplitFile(WimHandle, string, long)"/> method.
        /// </summary>
        Spanned = WimgApi.WIM_ATTRIBUTE_SPANNED,

        /// <summary>
        /// The .wim file contains integrity data that can be used by the <see cref="WimgApi.CopyFile(string, string, WimCopyFileOptions)"/> or <see cref="WimgApi.CreateFile"/> method.
        /// </summary>
        VerifyData = WimgApi.WIM_ATTRIBUTE_VERIFY_DATA,
    }

    /// <summary>
    /// Represents information about a Windows® image (.wim).
    /// </summary>
    public sealed class WimInfo
    {
        /// <summary>
        /// The native <see cref="WimgApi.WIM_INFO" /> struct that contains information about the .wim file.
        /// </summary>
        private readonly WimgApi.WIM_INFO _wimInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="WimInfo"/> class.
        /// </summary>
        /// <param name="wimInfoPtr">A pointer to a native <see cref="WimgApi.WIM_INFO" /> struct.</param>
        internal WimInfo(IntPtr wimInfoPtr)
            : this((WimgApi.WIM_INFO)Marshal.PtrToStructure(wimInfoPtr, typeof(WimgApi.WIM_INFO)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WimInfo"/> class.
        /// </summary>
        /// <param name="wimInfo">A <see cref="WimgApi.WIM_INFO" /> that contains information about the .wim file.</param>
        internal WimInfo(WimgApi.WIM_INFO wimInfo)
        {
            // Store the WIM_INFO struct
            _wimInfo = wimInfo;
        }

        /// <summary>
        /// Gets a <see cref="WimInfoAttributes" /> value that indicates how the file is treated and what features will be used.
        /// </summary>
        public WimInfoAttributes Attributes => (WimInfoAttributes)_wimInfo.WimAttributes;

        /// <summary>
        /// Gets the index of the bootable image in the .wim file. If this value is zero, then there are no bootable images
        /// available. To set a bootable image, call the WIMSetBootImage function.
        /// </summary>
        public int BootIndex => (int)_wimInfo.BootIndex;

        /// <summary>
        /// Gets a <see cref="WimCompressionType" /> value that indicates the method of compression used to compress resources in
        /// the .wim file.
        /// </summary>
        public WimCompressionType CompressionType => _wimInfo.CompressionType;

        /// <summary>
        /// Gets a <see cref="WimCreateFileOptions" /> value that indicates the options used when the .wim file was created.
        /// </summary>
        public WimCreateFileOptions CreateOptions => (WimCreateFileOptions)_wimInfo.WimFlagsAndAttr;

        /// <summary>
        /// Gets the unique identifier for the Windows® image (.wim) file.
        /// </summary>
        public Guid Guid => _wimInfo.Guid;

        /// <summary>
        /// Gets the number of images contained in the .wim file.
        /// </summary>
        public int ImageCount => (int)_wimInfo.ImageCount;

        /// <summary>
        /// Gets the part number of the current .wim file in a spanned set.  This value should be one, unless the data of the .wim
        /// file was originally split by the <see cref="WimgApi.SplitFile(WimHandle, string, long)" /> method.
        /// </summary>
        public int PartNumber => _wimInfo.PartNumber;

        /// <summary>
        /// Gets the full path to the .wim file.
        /// </summary>
        public string Path => _wimInfo.WimPath;

        /// <summary>
        /// Gets the total number of .wim file parts in a spanned set. This value must be one, unless the data of the .wim file was
        /// originally split via the <see cref="WimgApi.SplitFile(WimHandle, string, long)" /> method.
        /// </summary>
        public int TotalParts => _wimInfo.TotalParts;
    }
}