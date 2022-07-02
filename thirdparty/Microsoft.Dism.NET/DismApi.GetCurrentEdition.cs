// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Dism;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        public static string GetCurrentEdition(DismSession Session)
        {
            int hresult = NativeMethods._DismGetCurrentEdition(Session, out IntPtr EditionIdStringBuf);
            DismUtilities.ThrowIfFail(hresult);

            // Get a string from the pointer
            string EditionId = EditionIdStringBuf.ToStructure<DismString>();

            return EditionId;
        }

        internal static partial class NativeMethods
        {
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismGetCurrentEdition(DismSession Session, out IntPtr EditionIdStringBuf);
        }
    }
}