// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a callback method to be called to report progress during time-consuming operations.
    /// </summary>
    /// <param name="progress">A <see cref="DismProgress" /> object containing information about the current progress.</param>
    public delegate void DismProgressCallback(DismProgress progress);

    public static partial class DismApi
    {
        /// <summary>
        /// A client-defined callback function that DISM API uses to report progress on time-consuming operations. API functions that report progress accept a pointer to a DismProgressCallback function. DISM_PROGRESS_CALLBACK is a typedef to this function type.
        /// </summary>
        /// <param name="current">The current progress value.</param>
        /// <param name="total">The total progress value.</param>
        /// <param name="userData">User defined custom data. This parameter can be passed to another DISM function that accepts a progress callback and that function will then pass it through to DismProgressCallback.</param>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824734.aspx" />
        /// void DismProgressCallback(_In_ UINT Current, _In_ UINT Total, _In_ PVOID UserData)
        /// </remarks>
        internal delegate void DismProgressCallback(UInt32 current, UInt32 total, IntPtr userData);
    }
}
