// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Dism;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents the main entry point into the Deployment Image Servicing and Management (DISM) API.
    /// </summary>
    public static partial class DismApi
    {
        public static void SetEdition(DismSession session, string editionID)
        {
            SetEdition(session, editionID, null, null);
        }

        public static void SetEdition(DismSession session, string editionID, string productKey)
        {
            SetEdition(session, editionID, productKey, null);
        }

        public static void SetEdition(DismSession session, string editionID, Dism.DismProgressCallback progressCallback)
        {
            SetEdition(session, editionID, null, progressCallback, null);
        }

        public static void SetEdition(DismSession session, string editionID, string productKey, Dism.DismProgressCallback progressCallback)
        {
            SetEdition(session, editionID, productKey, progressCallback, null);
        }

        private static void SetEdition(DismSession session, string editionID, string productKey, Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new(progressCallback, userData);

            int hresult = NativeMethods._DismSetEdition(session, editionID, productKey, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Sets the edition of an image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="editionID">The target edition to upgrade to.</param>
            /// <param name="productKey">The product key that is being set, can be null.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismSetEdition(DismSession session, string editionID, string productKey, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}