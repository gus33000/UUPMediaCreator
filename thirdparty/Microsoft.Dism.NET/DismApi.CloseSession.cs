// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Closes a DISMSession created by <see cref="OpenOfflineSession(string)" /> method. This function does not unmount the image. To unmount the image, use the <see cref="UnmountImage(string, bool)" /> method once all sessions are closed.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> or <see cref="OpenOnlineSession" />method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void CloseSession(DismSession session)
        {
            int hresult = NativeMethods.DismCloseSession(session.DangerousGetHandle());

            DismUtilities.ThrowIfFail(hresult);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Closes a DISMSession created by DismOpenSession Function. This function does not unmount the image. To unmount the image, use the DismUnmountImage Function once all sessions are closed.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <returns>Returns S_OK on success.
            ///
            /// If the DISMSession is performing operations on other threads, those operations will complete before the DISMSession is destroyed. If additional operations are invoked by other threads after DismCloseSession is called, but before DismCloseSession returns, those operations will fail and return a DISMAPI_E_INVALID_DISM_SESSION error.
            ///
            /// The DISMSession handle will become invalid after completion of this call. Operations invoked on the DISMSession after completion of DismCloseSession will fail and return the error E_INVALIDARG.</returns>
            /// <remarks>The DISMSession will be shut down after this call is completed but the image will not be unmounted. To unmount the image, use the DismUnmountImage Function once all sessions are closed.</remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh825839.aspx" />
            /// HRESULT WINAPI DismCloseSession(_In_ DismSession Session);
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismCloseSession(IntPtr session);
        }
    }
}