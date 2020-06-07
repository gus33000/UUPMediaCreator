// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismDriverPackage" /> objects.
    /// </summary>
    public sealed class DismDriverPackageCollection : ReadOnlyCollection<DismDriverPackage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismDriverPackageCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismAppxPackage_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismDriverPackageCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismDriverPackage_ i) => new DismDriverPackage(i)))
        {
        }
    }
}