// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a collection of <see cref="DismCustomProperty" /> objects.
    /// </summary>
    public sealed class DismCustomPropertyCollection : ReadOnlyCollection<DismCustomProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DismCustomPropertyCollection" /> class.
        /// </summary>
        /// <param name="pointer">A pointer to the array of <see cref="DismApi.DismCustomProperty_" /> objects.</param>
        /// <param name="count">The number of objects in the array.</param>
        internal DismCustomPropertyCollection(IntPtr pointer, uint count)
            : base(pointer.ToList(count, (DismApi.DismCustomProperty_ i) => new DismCustomProperty(i)))
        {
        }
    }
}