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
        /// Registers a function to be called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback function.</param>
        /// <returns>The zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimMessageCallback messageCallback)
        {
            // Call an overload
            return RegisterMessageCallback(messageCallback, null);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <param name="userData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
        /// <returns>-1 if the callback is already registered, otherwise the zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimMessageCallback messageCallback, object userData)
        {
            // Call an overload
            return RegisterMessageCallback(WimHandle.Null, messageCallback, userData);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">An optional <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback function.</param>
        /// <returns>The zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // Call an overload
            return WimgApi.RegisterMessageCallback(wimHandle, messageCallback, null);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">An optional <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <param name="userData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
        /// <returns>-1 if the callback is already registered, otherwise the zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback, object userData)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if messageCallback is null
            if (messageCallback == null)
            {
                // Throw an ArgumentNullException
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // Establish a lock
            lock (WimgApi.LockObject)
            {
                // See if the user wants to register the handler in the global space for all WIMs
                if (wimHandle == WimHandle.Null)
                {
                    // See if the callback is already registered
                    if (WimgApi.RegisteredCallbacks.IsCallbackRegistered(messageCallback))
                    {
                        // Just exit, the callback is already registered
                        return -1;
                    }

                    // Add the callback to the globally registered callbacks
                    if (!WimgApi.RegisteredCallbacks.RegisterCallback(messageCallback, userData))
                    {
                        return -1;
                    }
                }
                else
                {
                    // See if the message callback is already registered
                    if (WimgApi.RegisteredCallbacks.IsCallbackRegistered(wimHandle, messageCallback))
                    {
                        // Just exit, the callback is already registered
                        return -1;
                    }

                    // Add the callback to the registered callbacks by handle
                    WimgApi.RegisteredCallbacks.RegisterCallback(wimHandle, messageCallback, userData);
                }

                // Call the native function
                DWORD hr = WimgApi.NativeMethods.WIMRegisterMessageCallback(wimHandle, wimHandle == WimHandle.Null ? WimgApi.RegisteredCallbacks.GetNativeCallback(messageCallback) : WimgApi.RegisteredCallbacks.GetNativeCallback(wimHandle, messageCallback), IntPtr.Zero);

                // See if the function returned INVALID_CALLBACK_VALUE
                if (hr == WimgApi.INVALID_CALLBACK_VALUE)
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }

                // Return the zero-based index of the callback
                return (int)hr;
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Registers a function to be called with imaging-specific data.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="fpMessageProc">
            /// A pointer to an application-defined callback function. For more information, see the
            /// WIMMessageCallback function.
            /// </param>
            /// <param name="pvUserData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
            /// <returns>
            /// If the function succeeds, then the return value is the zero-based index of the callback. If the function
            /// fails, then the return value is INVALID_CALLBACK_VALUE (0xFFFFFFFF). To obtain extended error information, call the
            /// GetLastError function.
            /// </returns>
            /// <remarks>
            /// If a WIM handle is specified, the callback function receives messages for only that WIM file. If no handle is
            /// specified, then the callback function receives messages for all image handles.
            /// Call the WIMUnregisterMessageCallback function when the callback function is no longer required.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern DWORD WIMRegisterMessageCallback([Optional] WimHandle hWim, WimgApi.WIMMessageCallback fpMessageProc, IntPtr pvUserData);
        }
    }
}
