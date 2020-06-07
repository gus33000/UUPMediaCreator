// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents the main entry point into the Deployment Image Servicing and Management (DISM) API.
    /// </summary>
    public static partial class DismApi
    {
        /// <summary>
        /// Set the product key of an image
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="productKey">The product key that is being set.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void SetProductKey(DismSession session, string productKey)
        {
            int hresult = NativeMethods._DismSetProductKey(session, productKey);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismSetProductKey(DismSession session, string productKey);
        }
    }
}
