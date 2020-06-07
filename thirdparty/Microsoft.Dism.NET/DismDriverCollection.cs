// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismDriver" /> objects.
    /// </summary>
    public sealed class DismDriverCollection : ReadOnlyCollection<DismDriver>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismDriverCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismDriver_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismDriverCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismDriver_ i) => new DismDriver(i)))
        {
        }
    }
}