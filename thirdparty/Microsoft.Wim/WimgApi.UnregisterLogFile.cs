// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Unregisters a log file for debugging or tracing purposes from the current WIMGAPI session.
        /// </summary>
        /// <param name="logFile">The path to a log file previously specified in a call to the <see cref="RegisterLogFile"/> method.</param>
        /// <exception cref="ArgumentNullException">logFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterLogFile(string logFile)
        {
            // See if logFile is null
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMUnregisterLogFile(logFile))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Unregisters a log file for debugging or tracing purposes from the current WIMGAPI session.
            /// </summary>
            /// <param name="pszLogFile">
            /// The path to a log file previously specified in a call to the WIMRegisterLogFile function. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnregisterLogFile(string pszLogFile);
        }
    }
}