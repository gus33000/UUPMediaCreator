// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Removes the capability from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="capabilityName">The name of the capability that is being removed</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemoveCapability(DismSession session, string capabilityName)
        {
            RemoveCapability(session, capabilityName, null, null);
        }

        /// <summary>
        /// Removes the capability from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="capabilityName">The name of the capability that is being removed</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemoveCapability(DismSession session, string capabilityName, Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismRemoveCapability(session, capabilityName, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Add a capability to an image.
            /// </summary>
            /// <param name="session">A valid DismSession. The DismSession must be associated with an image. You can associate a session with an image by using the DismOpenSession.</param>
            /// <param name="name">The name of the capability that is being removed</param>
            /// <param name="cancelEvent">This is a handle to an event for cancellation.</param>
            /// <param name="progress">Pointer to a client defined callback function to report progress.</param>
            /// <param name="userData">User defined custom data. This will be passed back to the user through the callback.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/mt684925.aspx" />
            /// HRESULT WINAPI DismRemoveCapability(_In_ DismSession Session, _In_ PCWSTR Name, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK  Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismRemoveCapability(DismSession session, string name, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}