// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismMountedImageInfo" /> objects.
    /// </summary>
    public sealed class DismMountedImageInfoCollection : ReadOnlyCollection<DismMountedImageInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismMountedImageInfoCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismMountedImageInfo_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismMountedImageInfoCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismMountedImageInfo_ i) => new DismMountedImageInfo(i)))
        {
        }
    }
}