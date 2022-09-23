// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Gets the count of callback routines currently registered by the imaging library.
        /// </summary>
        /// <returns>The number of message callback functions currently registered.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        public static int GetMessageCallbackCount()
        {
            return GetMessageCallbackCount(WimHandle.Null);
        }

        /// <summary>
        /// Gets the count of callback routines currently registered by the imaging library.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <returns>The number of message callback functions currently registered.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        public static int GetMessageCallbackCount(WimHandle wimHandle)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Return the value from the native function
            return (int)WimgApi.NativeMethods.WIMGetMessageCallbackCount(wimHandle);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns the count of callback routines currently registered by the imaging library.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <returns>The return value is the number of message callback functions currently registered.</returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern DWORD WIMGetMessageCallbackCount(WimHandle hWim);
        }
    }
}