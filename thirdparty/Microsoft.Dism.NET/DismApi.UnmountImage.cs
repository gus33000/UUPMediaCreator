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
        /// Unmounts a Windows image from a specified location.
        /// </summary>
        /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
        /// <param name="commitChanges">Specifies whether or not the changes to the image should be saved.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void UnmountImage(string mountPath, bool commitChanges)
        {
            UnmountImage(mountPath, commitChanges, null);
        }

        /// <summary>
        /// Unmounts a Windows image from a specified location.
        /// </summary>
        /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
        /// <param name="commitChanges">Specifies whether or not the changes to the image should be saved.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void UnmountImage(string mountPath, bool commitChanges, Dism.DismProgressCallback progressCallback)
        {
            UnmountImage(mountPath, commitChanges, progressCallback, null);
        }

        /// <summary>
        /// Unmounts a Windows image from a specified location.
        /// </summary>
        /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
        /// <param name="commitChanges">Specifies whether or not the changes to the image should be saved.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void UnmountImage(string mountPath, bool commitChanges, Dism.DismProgressCallback progressCallback, object userData)
        {
            // Determine flags
            uint flags = commitChanges ? DISM_COMMIT_IMAGE : DISM_DISCARD_IMAGE;

            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismUnmountImage(mountPath, flags, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Unmounts a Windows image from a specified location.
            /// </summary>
            /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
            /// <param name="flags">v</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns OK on success.</returns>
            /// <remarks>After you use the DismCloseSession Function to end every active DISMSession, you can unmount the image using the DismUnmountImage function.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824802.aspx" />
            /// HRESULT WINAPI DismUnmountImage(_In_ PCWSTR MountPath, _In_ DWORD Flags, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismUnmountImage(string mountPath, UInt32 flags, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}