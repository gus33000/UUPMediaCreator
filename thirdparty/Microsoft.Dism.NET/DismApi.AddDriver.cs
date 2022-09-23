// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Adds a third party driver (.inf) to an offline Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="driverPath">A relative or absolute path to the driver .inf file.</param>
        /// <param name="forceUnsigned">Indicates whether to accept unsigned drivers to an x64-based image. Unsigned drivers will automatically be added to an x86-based image.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void AddDriver(DismSession session, string driverPath, bool forceUnsigned)
        {
            int hresult = NativeMethods.DismAddDriver(session, driverPath, forceUnsigned);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        /// <summary>
        /// Adds third party drivers (.inf) from the specified directory to an offline Windows® image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="driverDirectory">A relative or absolute path to a directory containing driver .inf files.</param>
        /// <param name="forceUnsigned">Indicates whether to accept unsigned drivers to an x64-based image. Unsigned drivers will automatically be added to an x86-based image.</param>
        /// <param name="recursive"><code>true</code> to search recursively for driver files, otherwise <code>false</code>.</param>
        /// <exception cref="DirectoryNotFoundException">The directory specified by the <paramref name="driverDirectory" /> parameter does not exist.</exception>
        public static void AddDriversEx(DismSession session, string driverDirectory, bool forceUnsigned, bool recursive)
        {
            foreach (string driverPath in Directory.EnumerateFiles(driverDirectory, "*.inf", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                AddDriver(session, driverPath, forceUnsigned);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Adds a third party driver (.inf) to an offline Windows® image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="driverPath">A relative or absolute path to the driver .inf file.</param>
            /// <param name="forceUnsigned">A Boolean value that specifies whether to accept unsigned drivers to an x64-based image. Unsigned drivers will automatically be added to an x86-based image.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824764.aspx" />
            /// CDATA[HRESULT WINAPI DismAddDriver (_In_ DismSession Session, _In_ PCWSTR DriverPath, _In_ BOOL ForceUnsigned);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismAddDriver(DismSession session, string driverPath, [MarshalAs(UnmanagedType.Bool)] bool forceUnsigned);
        }
    }
}