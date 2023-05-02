// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Describes advanced feature information, such as installed state and whether a restart is required after installation.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824793.aspx" />
        /// typedef struct _DismFeatureInfo
        /// {
        ///     PCWSTR FeatureName;
        ///     DismPackageFeatureState FeatureState;
        ///     PCWSTR DisplayName;
        ///     PCWSTR Description;
        ///     DismRestartType RestartRequired;
        ///     DismCustomProperty* CustomProperty;
        ///     UINT CustomPropertyCount;
        /// } DismFeatureInfo;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismFeatureInfo_
        {
            /// <summary>
            /// The name of the feature.
            /// </summary>
            public string FeatureName;

            /// <summary>
            /// A valid DismPackageFeatureState Enumeration value such as DismStateInstalled or 7.
            /// </summary>
            public DismPackageFeatureState FeatureState;

            /// <summary>
            /// The display name of the feature. This is not always unique across all features.
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// The description of the feature.
            /// </summary>
            public string Description;

            /// <summary>
            /// A DismRestartType Enumeration value such as DismRestartPossible.
            /// </summary>
            public DismRestartType RestartRequired;

            /// <summary>
            /// An array of DismCustomProperty Structure.m
            /// </summary>
            public IntPtr CustomProperty;

            /// <summary>
            /// The number of elements in the CustomProperty array.
            /// </summary>
            public uint CustomPropertyCount;
        }
    }

    /// <summary>
    /// Represents advanced feature information, such as installed state and whether a restart is required after installation.
    /// </summary>
    public sealed class DismFeatureInfo : IEquatable<DismFeatureInfo>
    {
        private readonly DismApi.DismFeatureInfo_ _featureInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismFeatureInfo" /> class.
        /// </summary>
        /// <param name="featureInfoPtr">A pointer to a <see cref="DismApi.DismFeatureInfo_" /> struct.</param>
        internal DismFeatureInfo(IntPtr featureInfoPtr)
        {
            _featureInfo = featureInfoPtr.ToStructure<DismApi.DismFeatureInfo_>();

            CustomProperties = new DismCustomPropertyCollection(_featureInfo.CustomProperty, _featureInfo.CustomPropertyCount);
        }

        /// <summary>
        /// Gets a list of custom properties associated with the feature.
        /// </summary>
        public DismCustomPropertyCollection CustomProperties
        {
            get;
        }

        /// <summary>
        /// Gets the description of the feature.
        /// </summary>
        public string Description => _featureInfo.Description;

        /// <summary>
        /// Gets the display name of the feature. This is not always unique across all features.
        /// </summary>
        public string DisplayName => _featureInfo.DisplayName;

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public string FeatureName => _featureInfo.FeatureName;

        /// <summary>
        /// Gets the state of the feature.
        /// </summary>
        public DismPackageFeatureState FeatureState => _featureInfo.FeatureState;

        /// <summary>
        /// Gets a value indicating whether a restart is required when installing or uninstalling the feature.
        /// </summary>
        public DismRestartType RestartRequired => _featureInfo.RestartRequired;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />, otherwise <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return obj != null && Equals(obj as DismFeatureInfo);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismFeatureInfo" /> is equal to the current <see cref="DismFeatureInfo" />.
        /// </summary>
        /// <param name="other">The <see cref="DismFeatureInfo" /> object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="DismFeatureInfo" /> is equal to the current <see cref="DismFeatureInfo" />, otherwise <c>false</c>.</returns>
        public bool Equals(DismFeatureInfo? other)
        {
            return other != null
                   && CustomProperties.SequenceEqual(other.CustomProperties)
                   && Description == other.Description
                   && DisplayName == other.DisplayName
                   && FeatureName == other.FeatureName
                   && FeatureState == other.FeatureState
                   && RestartRequired == other.RestartRequired;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return CustomProperties.GetHashCode()
                   ^ (string.IsNullOrEmpty(Description) ? 0 : Description.GetHashCode())
                   ^ (string.IsNullOrEmpty(DisplayName) ? 0 : DisplayName.GetHashCode())
                   ^ (string.IsNullOrEmpty(FeatureName) ? 0 : FeatureName.GetHashCode())
                   ^ FeatureState.GetHashCode()
                   ^ RestartRequired.GetHashCode();
        }
    }
}