// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

namespace Microsoft.Dism
{
    /// <summary>
    /// Options that control session behavior
    /// </summary>
    public sealed class DismSessionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not an exception will be thrown if a reboot is required.
        /// If this value is set to false, the caller should check the <see cref="DismSession.RebootRequired"/> property to determine if reboot is required.
        /// <para>
        /// The default value is true.
        /// </para>
        /// </summary>
        public bool ThrowExceptionOnRebootRequired { get; set; } = true;
    }
}
