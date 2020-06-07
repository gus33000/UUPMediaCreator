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
        /// Commits the changes made to a Windows® image in a mounted .wim or .vhd file.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="discardChanges">true or false to discard changes made to the image.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void CommitImage(DismSession session, bool discardChanges)
        {
            CommitImage(session, discardChanges, null);
        }

        /// <summary>
        /// Commits the changes made to a Windows® image in a mounted .wim or .vhd file.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="discardChanges">true or false to discard changes made to the image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void CommitImage(DismSession session, bool discardChanges, Microsoft.Dism.DismProgressCallback progressCallback)
        {
            CommitImage(session, discardChanges, progressCallback, null);
        }

        /// <summary>
        /// Commits the changes made to a Windows® image in a mounted .wim or .vhd file.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="discardChanges">true or false to discard changes made to the image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void CommitImage(DismSession session, bool discardChanges, Microsoft.Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create the flags
            UInt32 flags = discardChanges ? DISM_DISCARD_IMAGE : DISM_COMMIT_IMAGE;

            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismCommitImage(session, flags, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Commits the changes made to a Windows® image in a mounted .wim or .vhd file. The image must be mounted using the DismMountImage Function.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="flags">The commit flags to use for this operation. For more information about mount flags, see DISM API Constants.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>The DismCommitImage function does not unmount the image.
            /// <para>DismCommitImage can only be used on an image that is mounted within the DISM infrastructure. It does not apply to images mounted by another tool, such as the DiskPart tool, which are serviced using the DismOpenSession Function. You must use the DismMountImage Function to mount an image within the DISM infrastructure.</para>
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh825835.aspx" />
            /// HRESULT WINAPI DismCommitImage(_In_ DismSession Session, _In_ DWORD Flags, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismCommitImage(DismSession session, UInt32 flags, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}