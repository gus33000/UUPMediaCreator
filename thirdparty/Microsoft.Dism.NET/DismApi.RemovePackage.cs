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
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packageName">The package name.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByName(DismSession session, string packageName)
        {
            RemovePackageByName(session, packageName, null);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packageName">The package name.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByName(DismSession session, string packageName, Dism.DismProgressCallback progressCallback)
        {
            RemovePackageByName(session, packageName, progressCallback, null);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packageName">The package name.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByName(DismSession session, string packageName, Dism.DismProgressCallback progressCallback, object userData)
        {
            RemovePackage(session, packageName, DismPackageIdentifier.Name, progressCallback, userData);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packagePath">The package path.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByPath(DismSession session, string packagePath)
        {
            RemovePackageByPath(session, packagePath, null);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByPath(DismSession session, string packagePath, Dism.DismProgressCallback progressCallback)
        {
            RemovePackageByPath(session, packagePath, progressCallback, null);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemovePackageByPath(DismSession session, string packagePath, Dism.DismProgressCallback progressCallback, object userData)
        {
            RemovePackage(session, packagePath, DismPackageIdentifier.Path, progressCallback, userData);
        }

        /// <summary>
        /// Removes a package from an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
        /// <param name="packageIdentifier">A DismPackageIdentifier Enumeration.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        private static void RemovePackage(DismSession session, string identifier, DismPackageIdentifier packageIdentifier, Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismRemovePackage(session, identifier, packageIdentifier, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes a package from an image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
            /// <param name="packageIdentifier">The parameter for a DismPackageIdentifier Enumeration.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>The DismRemovePackage function does not support .msu files.</remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824732.aspx" />
            /// HRESULT WINAPI DismRemovePackage (_In_ DismSession Session, _In_ PCWSTR Identifier, _In_ DismPackageIdentifier PackageIdentifier, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismRemovePackage(DismSession session, string identifier, DismPackageIdentifier packageIdentifier, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}