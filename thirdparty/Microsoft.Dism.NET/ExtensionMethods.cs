// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    /// <summary>
    /// Provides extension methods.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets a <see cref="List{T}" /> of objects at the specified pointer.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <typeparam name="TStruct">The type of the native structure for the item.</typeparam>
        /// <param name="ptr">The <see cref="IntPtr" /> pointing to the data.</param>
        /// <param name="count">The number of items that the pointer points to.</param>
        /// <param name="constructor">A <see cref="Func{T1,TResult}" /> that creates an instance of the item for the given native structure.</param>
        /// <returns>A <see cref="List{T}" /> containing the items.</returns>
        public static List<T> ToList<T, TStruct>(this IntPtr ptr, uint count, Func<TStruct, T> constructor)
            where T : class
        {
            List<T> list = new List<T>((int)count);

            if (ptr != IntPtr.Zero && count > 0)
            {
                int structSize = Marshal.SizeOf(typeof(TStruct));

                // Get the starting point as a long
                long startPtr = ptr.ToInt64();

                for (int i = 0; i < count; i++)
                {
                    IntPtr currentPtr = new IntPtr(startPtr + (i * structSize));

                    TStruct structure = currentPtr.ToStructure<TStruct>();

                    T item = constructor(structure);

                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Marshals data from an unmanaged block of memory to a newly allocated managed object of the specified type.
        /// </summary>
        /// <typeparam name="T">The System.Type of object to be created. This type object must represent a formatted class or a structure.</typeparam>
        /// <param name="ptr">A pointer to an unmanaged block of memory.</param>
        /// <returns>A managed object containing the data pointed to by the ptr parameter.</returns>
        /// <exception cref="ArgumentException">The T parameter layout is not sequential or explicit.
        /// -or-
        /// The T parameter is a generic type.</exception>
        public static T ToStructure<T>(this IntPtr ptr)
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
    }
}