// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Describes the language of the image.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824760.aspx" />
        /// typedef struct _DismString
        /// {
        ///     PCWSTR Value;
        /// } DismString;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal class DismLanguage
        {
            /// <summary>
            /// A null-terminated Unicode string.
            /// </summary>
            private string value;

            /// <summary>
            /// Initializes a new instance of the <see cref="DismLanguage" /> class.
            /// </summary>
            public DismLanguage()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DismLanguage" /> class.
            /// </summary>
            /// <param name="value">The language.</param>
            public DismLanguage(string value)
            {
                this.value = value;
            }

            /// <summary>
            /// Converts a <see cref="DismLanguage" /> class to a <see cref="string" /> object
            /// </summary>
            /// <param name="language">A DismLanguage object to convert.</param>
            /// <returns>The string associated with the DismLanguage</returns>
            public static implicit operator string(DismLanguage language)
            {
                return language.value;
            }

            /// <summary>
            /// Converts a <see cref="string" /> object to a <see cref="DismLanguage" /> object.
            /// </summary>
            /// <param name="str">A string to convert.</param>
            /// <returns>A DismLanguage containing the specified string.</returns>
            public static implicit operator DismLanguage(string str)
            {
                return new DismLanguage
                {
                    value = str,
                };
            }
        }
    }
}