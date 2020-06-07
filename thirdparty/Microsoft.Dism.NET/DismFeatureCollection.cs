// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismFeature" /> objects.
    /// </summary>
    public sealed class DismFeatureCollection : ReadOnlyCollection<DismFeature>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismFeatureCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismFeature_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismFeatureCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismFeature_ i) => new DismFeature(i)))
        {
        }
    }
}