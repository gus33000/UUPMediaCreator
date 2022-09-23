// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismCapability" /> objects.
    /// </summary>
    public sealed class DismCapabilityCollection : ReadOnlyCollection<DismCapability>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismCapabilityCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismCapability_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismCapabilityCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismCapability_ i) => new DismCapability(i)))
        {
        }
    }
}