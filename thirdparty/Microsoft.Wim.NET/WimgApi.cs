// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    /// <summary>
    /// Represents the Windows® Imaging API (WIMGAPI) for capturing and applying Windows® images (WIMs).
    /// </summary>
    public static partial class WimgApi
    {
        /// <summary>
        /// The calling convention to use when calling the WIMGAPI.
        /// </summary>
        internal const CallingConvention WimgApiCallingConvention = CallingConvention.Winapi;

        /// <summary>
        /// The character set to use when calling the WIMGAPI.
        /// </summary>
        internal const CharSet WimgApiCharSet = CharSet.Unicode;

        /// <summary>
        /// The name of the assembly containing the Windows® Imaging API (WIMGAPI).
        /// </summary>
        internal const string WimgApiDllName = "WimgApi.dll";

        /// <summary>
        /// Used as an object for locking.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// An instance of the <see cref="WimRegisteredCallbacks"/> class for keeping track of registered callbacks.
        /// </summary>
        private static readonly WimRegisteredCallbacks RegisteredCallbacks = new WimRegisteredCallbacks();
    }
}