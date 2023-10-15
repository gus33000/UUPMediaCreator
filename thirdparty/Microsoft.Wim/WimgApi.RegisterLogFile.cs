// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Registers a log file for debugging or tracing purposes from the current WIMGAPI session.
        /// </summary>
        /// <param name="logFile">The full file path of the file to receive debug or tracing information.</param>
        /// <exception cref="ArgumentNullException">logFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void RegisterLogFile(string logFile)
        {
            // See if logFile is null
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMRegisterLogFile(logFile, 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Registers a log file for debugging or tracing purposes into the current WIMGAPI session.
            /// </summary>
            /// <param name="pszLogFile">
            /// A pointer to the full file path of the file to receive debug or tracing information. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMRegisterLogFile(string pszLogFile, DWORD dwFlags);
        }
    }
}
