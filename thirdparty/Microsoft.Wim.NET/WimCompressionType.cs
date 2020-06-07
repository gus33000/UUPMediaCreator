// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

namespace Microsoft.Wim
{
    /// <summary>
    /// Represents the compression mode to be used for a newly created image file.
    /// </summary>
    public enum WimCompressionType : uint
    {
        /// <summary>
        /// Capture uses LZMS file compression.
        /// </summary>
        Lzms = 3,

        /// <summary>
        /// Capture uses LZX file compression.
        /// </summary>
        Lzx = 2,

        /// <summary>
        /// Capture does not use file compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// Capture uses XPRESS file compression.
        /// </summary>
        Xpress = 1,
    }
}