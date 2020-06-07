// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Dism
{
    /// <summary>
    /// Represents a DismSession handle.
    /// </summary>
    public sealed class DismSession : SafeHandleZeroOrMinusOneIsInvalid
    {
        private readonly string _imagePath;
        private readonly string _systemDrive;
        private readonly string _windowsDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismSession" /> class.
        /// </summary>
        /// <param name="imagePath">An absolute or relative path to the root directory of an offline Windows image, an absolute or relative path to the root directory of a mounted Windows image, or DISM_ONLINE_IMAGE to associate with the online Windows installation.</param>
        /// <param name="windowsDirectory">A relative or absolute path to the Windows directory. The path is relative to the mount point.</param>
        /// <param name="systemDrive">The letter of the system drive that contains the boot manager. If SystemDrive is NULL, the default value of the drive containing the mount point is used.</param>
        internal DismSession(string imagePath, string windowsDirectory, string systemDrive)
            : base(true)
        {
            _imagePath = imagePath;
            _windowsDirectory = windowsDirectory;
            _systemDrive = systemDrive;

            Reload();
        }

        /// <summary>
        /// Reloads the session by closing the current session and opening it again.
        /// </summary>
        internal void Reload()
        {
            if (!IsInvalid)
            {
                DismApi.NativeMethods.DismCloseSession(handle);
            }

            int hresult = DismApi.NativeMethods.DismOpenSession(_imagePath, _windowsDirectory, _systemDrive, out IntPtr sessionPtr);

            DismUtilities.ThrowIfFail(hresult);

            SetHandle(sessionPtr);
        }

        /// <summary>
        /// Releases the DismSession handle.
        /// </summary>
        /// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // See if the handle is valid and hasn't already been closed
            if (!IsInvalid)
            {
                // Close the session
                return DismApi.NativeMethods.DismCloseSession(DangerousGetHandle()) == 0;
            }

            // Return true
            return true;
        }
    }
}