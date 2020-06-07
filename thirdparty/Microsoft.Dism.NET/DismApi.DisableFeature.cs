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
        /// Disables a feature in the current image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to disable. To disable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">Optional. The name of the parent package that the feature is a part of.
        ///
        /// This is an optional parameter. If no package is specified, then the default Windows® Foundation package is used.</param>
        /// <param name="removePayload">Specifies whether to remove the files required to enable the feature.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void DisableFeature(DismSession session, string featureName, string packageName, bool removePayload)
        {
            DisableFeature(session, featureName, packageName, removePayload, null);
        }

        /// <summary>
        /// Disables a feature in the current image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to disable. To disable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">Optional. The name of the parent package that the feature is a part of.
        ///
        /// This is an optional parameter. If no package is specified, then the default Windows® Foundation package is used.</param>
        /// <param name="removePayload">Specifies whether to remove the files required to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void DisableFeature(DismSession session, string featureName, string packageName, bool removePayload, Microsoft.Dism.DismProgressCallback progressCallback)
        {
            DisableFeature(session, featureName, packageName, removePayload, progressCallback, null);
        }

        /// <summary>
        /// Disables a feature in the current image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to disable. To disable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">Optional. The name of the parent package that the feature is a part of.
        ///
        /// This is an optional parameter. If no package is specified, then the default Windows® Foundation package is used.</param>
        /// <param name="removePayload">Specifies whether to remove the files required to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void DisableFeature(DismSession session, string featureName, string packageName, bool removePayload, Microsoft.Dism.DismProgressCallback progressCallback, object userData)
        {
            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismDisableFeature(session, featureName, packageName, removePayload, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Disables a feature in the current image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="featureName">The name of the feature that you want to disable. To disable more than one feature, separate each feature name with a semicolon.</param>
            /// <param name="packageName">Optional. The name of the parent package that the feature is a part of.
            ///
            /// This is an optional parameter. If no package is specified, then the default Windows® Foundation package is used.</param>
            /// <param name="removePayload">A Boolean value specifying whether to remove the files required to enable the feature.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User specified data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824766.aspx" />
            /// HRESULT WINAPI DismDisableFeature (_In_ DismSession Session, _In_ PCWSTR FeatureName, _In_opt_ PCWSTR PackageName, _In_ BOOL RemovePayload, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismDisableFeature(DismSession session, string featureName, string packageName, [MarshalAs(UnmanagedType.Bool)] bool removePayload, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}