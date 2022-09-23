// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismPackage" /> objects.
    /// </summary>
    public sealed class DismPackageCollection : ReadOnlyCollection<DismPackage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismPackage_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismPackageCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismPackage_ i) => new DismPackage(i)))
        {
        }
    }
}