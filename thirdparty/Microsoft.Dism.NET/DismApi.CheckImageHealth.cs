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
        /// Checks whether the image can be serviced or whether it is corrupted.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="scanImage">Specifies whether to scan the image or just check for flags from a previous scan.</param>
        /// <returns>A <see cref="DismImageHealthState" /> indicating the health state of the image.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismImageHealthState CheckImageHealth(DismSession session, bool scanImage)
        {
            return CheckImageHealth(session, scanImage, null);
        }

        /// <summary>
        /// Checks whether the image can be serviced or whether it is corrupted.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="scanImage">Specifies whether to scan the image or just check for flags from a previous scan.</param>
        /// <param name="progressCallback">A DismProgressCallback method to call when progress is made.</param>
        /// <returns>A <see cref="DismImageHealthState" /> indicating the health state of the image.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static DismImageHealthState CheckImageHealth(DismSession session, bool scanImage, Microsoft.Dism.DismProgressCallback progressCallback)
        {
            return CheckImageHealth(session, scanImage, progressCallback, null);
        }

        /// <summary>
        /// Checks whether the image can be serviced or whether it is corrupted.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="scanImage">Specifies whether to scan the image or just check for flags from a previous scan.</param>
        /// <param name="progressCallback">A DismProgressCallback method to call when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <returns>A <see cref="DismImageHealthState" /> indicating the health state of the image.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static DismImageHealthState CheckImageHealth(DismSession session, bool scanImage, Microsoft.Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismCheckImageHealth(session, scanImage, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero, out DismImageHealthState imageHealthState);

            DismUtilities.ThrowIfFail(hresult, session);

            return imageHealthState;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Checks whether the image can be serviced or whether it is corrupted.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="scanImage">A Boolean value that specifies whether to scan the image or just check for flags from a previous scan.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <param name="imageHealth">A pointer to the DismImageHealthState Enumeration. The enumeration value is set during this operation.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>If ScanImage is set to True, this function will take longer to finish.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824769.aspx" />
            /// HRESULT WINAPI DismCheckImageHealth(_In_ DismSession Session, _In_ BOOL ScanImage, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData, _Out_ DismImageHealthState* ImageHealth);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismCheckImageHealth(DismSession session, [MarshalAs(UnmanagedType.Bool)] bool scanImage, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData, out DismImageHealthState imageHealth);
        }
    }
}