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
        /// Unregisters a method from being called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <exception cref="ArgumentOutOfRangeException">messageCallback is not registered.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterMessageCallback(WimMessageCallback messageCallback)
        {
            UnregisterMessageCallback(WimHandle.Null, messageCallback);
        }

        /// <summary>
        /// Unregisters a method from being called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <exception cref="ArgumentOutOfRangeException">messageCallback is not registered.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Establish a lock
            lock (WimgApi.LockObject)
            {
                // See if wimHandle was not specified but the message callback was
                if (wimHandle == WimHandle.Null && messageCallback != null)
                {
                    // See if the message callback is registered
                    if (!WimgApi.RegisteredCallbacks.IsCallbackRegistered(messageCallback))
                    {
                        // Throw an ArgumentOutOfRangeException
                        throw new ArgumentOutOfRangeException(nameof(messageCallback), "Message callback is not registered.");
                    }
                }

                // See if the wimHandle and callback were specified
                if (wimHandle != WimHandle.Null && messageCallback != null)
                {
                    // See if the callback is registered
                    if (!WimgApi.RegisteredCallbacks.IsCallbackRegistered(wimHandle, messageCallback))
                    {
                        // Throw an ArgumentOutOfRangeException
                        throw new ArgumentOutOfRangeException(nameof(messageCallback), "Message callback is not registered under this handle.");
                    }
                }

                // See if the message callback is null, meaning the user wants to unregister all callbacks
                bool success = messageCallback == null
                    ? WimgApi.NativeMethods.WIMUnregisterMessageCallback(
                        wimHandle,
                        fpMessageProc: null)
                    : WimgApi.NativeMethods.WIMUnregisterMessageCallback(
                        wimHandle,
                        wimHandle == WimHandle.Null
                            ? WimgApi.RegisteredCallbacks.GetNativeCallback(messageCallback)
                            : WimgApi.RegisteredCallbacks.GetNativeCallback(wimHandle, messageCallback));

                // See if the native call succeeded
                if (!success)
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }

                // See if a single globally registered callback should be removed
                if (wimHandle == WimHandle.Null && messageCallback != null)
                {
                    // Unregister the globally registered callback
                    WimgApi.RegisteredCallbacks.UnregisterCallback(messageCallback);
                }

                // See if a single registered callback by handle should be removed
                if (wimHandle != WimHandle.Null && messageCallback != null)
                {
                    // Unregister the callback for the handle
                    WimgApi.RegisteredCallbacks.UnregisterCallback(wimHandle, messageCallback);
                }

                // See if all registered callbacks for this handle should be removed
                if (wimHandle != WimHandle.Null && messageCallback == null)
                {
                    // Unregister all callbacks for this handle
                    WimgApi.RegisteredCallbacks.UnregisterCallbacks(wimHandle);
                }

                // See if all registered callbacks by handle and all globally registered callbacks should be removed
                if (wimHandle == WimHandle.Null && messageCallback == null)
                {
                    // Unregister all callbacks
                    WimgApi.RegisteredCallbacks.UnregisterCallbacks();
                }
            } // Release lock
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Unregisters a function from being called with imaging-specific data.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="fpMessageProc">
            /// A pointer to the application-defined callback function to unregister. Specify NULL to
            /// unregister all callback functions.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnregisterMessageCallback([Optional] WimHandle hWim, WimgApi.WIMMessageCallback fpMessageProc);
        }
    }
}