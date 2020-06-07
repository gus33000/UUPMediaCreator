// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// DISM API functions that return strings wrap the heap allocated PCWSTR in a DismString structure.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824782.aspx" />
        /// typedef struct _DismString
        /// {
        ///     PCWSTR Value;
        /// } DismString;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal class DismString
        {
            /// <summary>
            /// A null-terminated Unicode string.
            /// </summary>
            private string value;

            /// <summary>
            /// Converts a <see cref="DismString" /> class to a <see cref="string" /> object
            /// </summary>
            /// <param name="dismString">The <see cref="DismString" /> object to convert.</param>
            /// <returns>The current <see cref="DismString" /> as a <see cref="string" />.</returns>
            public static implicit operator string(DismString dismString)
            {
                return dismString.value;
            }

            /// <summary>
            /// Converts a <see cref="string" /> object to a <see cref="DismString" /> object.
            /// </summary>
            /// <param name="str">The string to convert.</param>
            /// <returns>The current <see cref="string" /> as a <see cref="DismString" /> object.</returns>
            public static implicit operator DismString(string str)
            {
                return new DismString
                {
                    value = str,
                };
            }
        }
    }
}