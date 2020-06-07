// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Adds a single .cab or .msu file to a Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession method.</param>
        /// <param name="packagePath">A relative or absolute path to the .cab or .msu file being added, a folder with one or more packages, or a folder containing the expanded files of a single .cab file.</param>
        /// <param name="ignoreCheck">Specifies whether to ignore the internal applicability checks that are done when a package is added.</param>
        /// <param name="preventPending">Specifies whether to add a package if it has pending online actions.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        /// <exception cref="DismPackageNotApplicableException">When the package is not applicable to the specified session.</exception>
        public static void AddPackage(DismSession session, string packagePath, bool ignoreCheck, bool preventPending)
        {
            AddPackage(session, packagePath, ignoreCheck, preventPending, null);
        }

        /// <summary>
        /// Adds a single .cab or .msu file to a Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession method.</param>
        /// <param name="packagePath">A relative or absolute path to the .cab or .msu file being added, a folder with one or more packages, or a folder containing the expanded files of a single .cab file.</param>
        /// <param name="ignoreCheck">Specifies whether to ignore the internal applicability checks that are done when a package is added.</param>
        /// <param name="preventPending">Specifies whether to add a package if it has pending online actions.</param>
        /// <param name="progressCallback">A DismProgressCallback method to call when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        /// <exception cref="DismPackageNotApplicableException">When the package is not applicable to the specified session.</exception>
        public static void AddPackage(DismSession session, string packagePath, bool ignoreCheck, bool preventPending, Microsoft.Dism.DismProgressCallback progressCallback)
        {
            AddPackage(session, packagePath, ignoreCheck, preventPending, progressCallback, null);
        }

        /// <summary>
        /// Adds a single .cab or .msu file to a Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession method.</param>
        /// <param name="packagePath">A relative or absolute path to the .cab or .msu file being added, a folder with one or more packages, or a folder containing the expanded files of a single .cab file.</param>
        /// <param name="ignoreCheck">Specifies whether to ignore the internal applicability checks that are done when a package is added.</param>
        /// <param name="preventPending">Specifies whether to add a package if it has pending online actions.</param>
        /// <param name="progressCallback">A DismProgressCallback method to call when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        /// <exception cref="DismPackageNotApplicableException">When the package is not applicable to the specified session.</exception>
        public static void AddPackage(DismSession session, string packagePath, bool ignoreCheck, bool preventPending, Microsoft.Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismAddPackage(session, packagePath, ignoreCheck, preventPending, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Adds a single .cab or .msu file to a Windows® image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="packagePath">A relative or absolute path to the .cab or .msu file being added, a folder with one or more packages, or a folder containing the expanded files of a single .cab file.</param>
            /// <param name="ignoreCheck">A Boolean value to specify whether to ignore the internal applicability checks that are done when a package is added.</param>
            /// <param name="preventPending">A Boolean value to specify whether to add a package if it has pending online actions.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>Only .cab files can be added to an online image. Either .cab or .msu files can be added to an offline image.
            ///
            /// This function will return a special error code if the package is not applicable. You can use the DismGetPackageInfo Function to determine if a package is applicable to the target image.
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824788.aspx" />
            /// HRESULT WINAPI DismAddPackage (_In_ DismSession Session, _In_ PCWSTR PackagePath, _In_ BOOL IgnoreCheck, _In_ BOOL PreventPending _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData)
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismAddPackage(DismSession session, string packagePath, [MarshalAs(UnmanagedType.Bool)] bool ignoreCheck, [MarshalAs(UnmanagedType.Bool)] bool preventPending, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}