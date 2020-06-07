// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
#pragma warning disable SA1401 // Fields must be private
                              /// <summary>
                              /// Allows tests to override the functionality of the <see cref="GetLastErrorMessage" /> method.
                              /// </summary>
        internal static Func<string> GetLastErrorMessageTestHook = null;
#pragma warning restore SA1401 // Fields must be private
    }
}
