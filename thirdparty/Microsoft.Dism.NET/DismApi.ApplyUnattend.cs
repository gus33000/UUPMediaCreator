// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Applies an unattended answer file to a Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="unattendFile">A relative or absolute path to the answer file that will be applied to the image.</param>
        /// <param name="singleSession">Specifies whether the packages that are listed in an answer file will be processed in a single session or in multiple sessions.</param>
        public static void ApplyUnattend(DismSession session, string unattendFile, bool singleSession)
        {
            int hresult = NativeMethods.DismApplyUnattend(session, unattendFile, singleSession);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Applies an unattended answer file to a Windows® image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="unattendFile">A relative or absolute path to the answer file that will be applied to the image.</param>
            /// <param name="singleSession">A Boolean value that specifies whether the packages that are listed in an answer file will be processed in a single session or in multiple sessions.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>When you use DISM to apply an answer file to an image, the unattended settings in the offlineServicing configuration pass are applied to the Windows image. For more information, see Unattended Servicing Command-Line Options.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh825840.aspx" />
            /// HRESULT WINAPI DismApplyUnattend (_In_ DismSession Session, _In_ PCWSTR UnattendFile, _In_ BOOL SingleSession);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismApplyUnattend(DismSession session, string unattendFile, [MarshalAs(UnmanagedType.Bool)] bool singleSession);
        }
    }
}