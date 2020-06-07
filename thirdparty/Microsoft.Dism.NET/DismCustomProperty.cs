// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Describes the custom properties of a package. Custom properties are any properties that are not found in DismPackage Structure or DismFeature Structure.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824746.aspx" />
        /// typedef struct _DismCustomProperty
        /// {
        ///     PCWSTR Name;
        ///     PCWSTR Value;
        ///     PCWSTR Path;
        /// } DismCustomProperty;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismCustomProperty_
        {
            /// <summary>
            /// The name of the custom property.
            /// </summary>
            public string Name;

            /// <summary>
            /// The value of the custom property.
            /// </summary>
            public string Value;

            /// <summary>
            /// The value of the custom property.
            /// </summary>
            public string Path;
        }
    }

    /// <summary>
    /// Represents the custom properties of a package. Custom properties are any properties that are not found in <see cref="DismPackage" /> or <see cref="DismFeature" />.
    /// </summary>
    public class DismCustomProperty : IEquatable<DismCustomProperty>
    {
        private readonly DismApi.DismCustomProperty_ _customProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismCustomProperty" /> class.
        /// </summary>
        /// <param name="customProperty">A native <see cref="DismApi.DismCustomProperty_" /> struct that holds the data of the custom property</param>
        internal DismCustomProperty(DismApi.DismCustomProperty_ customProperty)
        {
            // Save a reference to the native struct
            _customProperty = customProperty;
        }

        /// <summary>
        /// Gets the name of the custom property.
        /// </summary>
        public string Name => _customProperty.Name;

        /// <summary>
        /// Gets the value of the custom property.
        /// </summary>
        public string Path => _customProperty.Path;

        /// <summary>
        /// Gets the value of the custom property.
        /// </summary>
        public string Value => _customProperty.Value;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismCustomProperty);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismCustomProperty" /> is equal to the current <see cref="DismCustomProperty" />.
        /// </summary>
        /// <param name="other">A <see cref="DismCustomProperty" /> object to compare with the current object.</param>
        /// <returns><code>true</code> if the specified <see cref="DismCustomProperty" /> is equal to the current <see cref="DismCustomProperty" />; otherwise, false.</returns>
        public bool Equals(DismCustomProperty other)
        {
            return other != null
                   && Name == other.Name
                   && Path == other.Path
                   && Value == other.Value;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return (string.IsNullOrEmpty(Name) ? 0 : Name.GetHashCode())
                   ^ (string.IsNullOrEmpty(Path) ? 0 : Path.GetHashCode())
                   ^ (string.IsNullOrEmpty(Value) ? 0 : Value.GetHashCode());
        }
    }
}