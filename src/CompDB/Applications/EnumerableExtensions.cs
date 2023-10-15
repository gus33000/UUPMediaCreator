/*
 * Copyright (c) The UUP Media Creator authors and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedUpdatePlatform.Services.Composition.Database.Applications
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Gets all combinations of a given list
        /// From https://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array</typeparam>
        /// <param name="source">The source collection</param>
        /// <returns>All combinations of a given list</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source)
        {
            return null == source ? throw new ArgumentNullException(nameof(source)) : source.ToArray().Combinations();
        }

        public static IEnumerable<T[]> Combinations<T>(this T[] data)
        {
            return null == data
                ? throw new ArgumentNullException(nameof(data))
                : (IEnumerable<T[]>)Enumerable
              .Range(0, 1 << data.Length)
              .Select(index => data
                 .Where((v, i) => (index & (1 << i)) != 0)
                 .ToArray()).ToArray();
        }
    }
}
