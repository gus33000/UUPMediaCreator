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
        /// Gets DISM capabilities.
        /// </summary>
        /// <param name="session">A valid DismSession. The DismSession must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <returns>A <see cref="DismCapabilityCollection" /> object containing a collection of <see cref="DismCapability" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismCapabilityCollection GetCapabilities(DismSession session)
        {
            int hresult = NativeMethods.DismGetCapabilities(session, out IntPtr capabilityPtr, out UInt32 capabilityCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismCapabilityCollection(capabilityPtr, capabilityCount);
            }
            finally
            {
                Delete(capabilityPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets DISM capabilities.
            /// </summary>
            /// <param name="session">A valid DismSession. The DismSession must be associated with an image. You can associate a session with an image by using the DismOpenSession.</param>
            /// <param name="capability">Pointer that will receive the info of capability.</param>
            /// <param name="count">The number of DismCapability structures that were returned.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetCapabilities(DismSession session, out IntPtr capability, out UInt32 count);
        }
    }
}