// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Closes an open Windows® imaging (.wim) file or image handle.
        /// </summary>
        /// <param name="handle">A <see cref="WimHandle"/> to an open, image-based object.</param>
        /// <returns><c>true</c> if the handle was successfully closed, otherwise <c>false</c>.</returns>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        internal static bool CloseHandle(IntPtr handle)
        {
            // Call the native function
            if (!WimgApi.NativeMethods.WIMCloseHandle(handle))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            return true;
        }

        /// <summary>
        /// Contains declarations for external native functions.
        /// </summary>
        internal static partial class NativeMethods
        {
            /// <summary>
            /// Closes an open Windows® imaging (.wim) file or image handle.
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd851955.aspx">WIMCloseHandle</a>
            /// </summary>
            /// <param name="hObject">The handle to an open, image-based object.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>
            /// The WIMCloseHandle function closes handles to the following objects:
            /// A .wim file
            /// A volume image
            /// If there are any open volume image handles, closing a .wim file fails.
            /// Use the WIMCloseHandle function to close handles returned by calls to the WIMCreateFile, WIMLoadImage, and
            /// WIMCaptureImage functions.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMCloseHandle(IntPtr hObject);
        }
    }

    /// <summary>
    /// Represents a handle to a Windows® image (.wim) file or an image inside of a .wim file.
    /// </summary>
    public sealed class WimHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Represents a <c>null</c> handle.
        /// </summary>
        public static readonly WimHandle Null = new WimHandle();

        /// <summary>
        /// Initializes a new instance of the <see cref="WimHandle"/> class.
        /// </summary>
        internal WimHandle()
            : base(true)
        {
            // Default to a null handle
            handle = IntPtr.Zero;
        }

        /// <inheritdoc cref="SafeHandle.ReleaseHandle"/>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return !IsInvalid && WimgApi.CloseHandle(handle);
        }
    }
}