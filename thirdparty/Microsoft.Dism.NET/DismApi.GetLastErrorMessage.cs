// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Gets the error message in the current thread immediately after a failure.
        /// </summary>
        /// <returns>An error message if one is found, otherwise null.</returns>
        public static string GetLastErrorMessage()
        {
            // Allow this method to be overridden by an internal test hook
            if (GetLastErrorMessageTestHook != null)
            {
                return GetLastErrorMessageTestHook();
            }

            if (NativeMethods.DismGetLastErrorMessage(out IntPtr errorMessagePtr) != ERROR_SUCCESS)
            {
                return null;
            }

            try
            {
                // Get a string from the pointer
                string dismString = errorMessagePtr.ToStructure<DismString>();

                // See if the string has a value
                if (string.IsNullOrEmpty(dismString) == false)
                {
                    // Return the trimmed value
                    return dismString.Trim();
                }
            }
            finally
            {
                // Clean up
                Delete(errorMessagePtr);
            }

            // No error message was found
            return null;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Retrieves the error message in the current thread immediately after a failure.
            /// </summary>
            /// <param name="errorMessage">The detailed error message in the current thread.</param>
            /// <returns>Returns OK on success.</returns>
            /// <remarks>You can retrieve a detailed error message immediately after a DISM API failure. The last error message is maintained on a per-thread basis. An error message on a thread will not overwrite the last error message on another thread.
            ///
            /// DismGetLastErrorMessage does not apply to the DismShutdown function, DismDelete function, or the DismGetLastErrorMessage function.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824754.aspx" />
            /// HRESULT WINAPI DismGetLastErrorMessage(_Out_ DismString** ErrorMessage);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetLastErrorMessage(out IntPtr errorMessage);
        }
    }
}