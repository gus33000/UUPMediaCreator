// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismImageInfo" /> objects.
    /// </summary>
    public sealed class DismImageInfoCollection : ReadOnlyCollection<DismImageInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismImageInfoCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismImageInfo_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismImageInfoCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismImageInfo_ i) => new DismImageInfo(i)))
        {
        }
    }
}