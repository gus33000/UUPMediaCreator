// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Dism.NET;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Shuts down DISM API. Shutdown must be called once per process. Other DISM API function calls will fail after Shutdown has been called.
        /// </summary>
        public static void Shutdown()
        {
            lock (InitializeShutDownLock)
            {
                if (_isInitialized)
                {
                    if (CurrentDismGeneration != DismGeneration.NotFound)
                    {
                        DismUtilities.UnloadDismGenerationLibrary();
                        CurrentDismGeneration = DismGeneration.NotFound;
                    }

                    int hresult = NativeMethods.DismShutdown();

                    DismUtilities.ThrowIfFail(hresult);

                    _isInitialized = false;
                }
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Shuts down DISM API. DismShutdown must be called once per process. Other DISM API function calls will fail after DismShutdown has been called.
            /// </summary>
            /// <returns><para>Returns S_OK on success.</para>
            /// <para>Returns DISMAPI_E_DISMAPI_NOT_INITIALIZED if the DismInitialize Function has not been called.</para>
            /// <para>Returns DISMAPI_E_OPEN_SESSION_HANDLES if any open DISMSession have not been closed.</para></returns>
            /// <remarks><para>You must call DismShutdown once per process. Calls to DismShutdown must be matched to an earlier call to the DismInitialize Function. DISM API will serialize concurrent calls to DismShutdown. The first call will succeed and the other calls will fail.</para>
            /// <para>Before calling DismShutdown, you must close all DISMSession using the DismCloseSession Function. If there are open DismSessions when calling DismShutdown, then the DismShutdown call will fail. For more information, see Using the DISM API.</para>
            /// <para>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824773.aspx" />
            /// HRESULT WINAPI DismShutdown( );
            /// </para>
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismShutdown();
        }
    }
}