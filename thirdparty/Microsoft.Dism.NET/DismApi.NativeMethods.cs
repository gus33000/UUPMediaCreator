// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Represents native functions called by DismApi.
        /// </summary>
        internal static partial class NativeMethods
        {
            private const CharSet DismCharacterSet = CharSet.Unicode;
            private const string DismDllName = "DismApi";
        }
    }
}