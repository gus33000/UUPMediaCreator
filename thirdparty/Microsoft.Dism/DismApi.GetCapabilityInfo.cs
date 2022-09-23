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
        /// Gets DISM capability info.
        /// </summary>
        /// <param name="session">A valid DismSession. The DismSession must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="capabilityName">The name of the specified capability.</param>
        /// <returns>A <see cref="DismCapabilityInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismCapabilityInfo GetCapabilityInfo(DismSession session, string capabilityName)
        {
            int hresult = NativeMethods.DismGetCapabilityInfo(session, capabilityName, out IntPtr capabilityInfoPtr);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                // Return a new DismCapabilityInfo from the native pointer
                return new DismCapabilityInfo(capabilityInfoPtr);
            }
            finally
            {
                // Clean up the native pointer
                Delete(capabilityInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets DISM capabilities.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="name">The name of the specified capability.</param>
            /// <param name="info">Pointer that will receive the info of capability.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetCapabilityInfo(DismSession session, string name, out IntPtr info);
        }
    }
}
