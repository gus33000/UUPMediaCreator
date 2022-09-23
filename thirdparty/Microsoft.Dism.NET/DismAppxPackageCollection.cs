// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismAppxPackage" /> objects.
    /// </summary>
    public sealed class DismAppxPackageCollection : ReadOnlyCollection<DismAppxPackage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismAppxPackageCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismAppxPackage_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismAppxPackageCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismAppxPackage_ i) => new DismAppxPackage(i)))
        {
        }
    }
}