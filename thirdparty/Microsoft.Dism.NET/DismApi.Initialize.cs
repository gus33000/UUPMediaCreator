// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Used to lock when initializing or shutting down.
        /// </summary>
        private static readonly object InitializeShutDownLock = new object();

        /// <summary>
        /// Used to keep track if DismApi has been initialized.
        /// </summary>
        private static bool _isInitialized;

        /// <summary>
        /// Initializes DISM API. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void Initialize(DismLogLevel logLevel)
        {
            Initialize(logLevel, null);
        }

        /// <summary>
        /// Initializes DISM API. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <param name="logFilePath">A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void Initialize(DismLogLevel logLevel, string logFilePath)
        {
            Initialize(logLevel, logFilePath, null);
        }

        /// <summary>
        /// Initializes DISM API. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <param name="logFilePath">A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
        /// <param name="scratchDirectory">A relative or absolute path to a scratch directory. DISM API will use this directory for internal operations. If null, the default temp directory, \Windows\%Temp%, will be used.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void Initialize(DismLogLevel logLevel, string logFilePath, string scratchDirectory)
        {
            lock (InitializeShutDownLock)
            {
                if (!_isInitialized)
                {
                    int hresult = NativeMethods.DismInitialize(logLevel, logFilePath, scratchDirectory);

                    DismUtilities.ThrowIfFail(hresult);

                    _isInitialized = true;
                }
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Initializes DISM API. DismInitialize must be called once per process before calling any other DISM API functions.
            /// </summary>
            /// <param name="logLevel">A DismLogLevel Enumeration value, such as DismLogErrorsWarnings.</param>
            /// <param name="logFilePath">Optional. A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
            /// <param name="scratchDirectory">Optional. A relative or absolute path to a scratch directory. DISM API will use this directory for internal operations. If NULL, the default temp directory, \Windows\%Temp%, will be used.</param>
            /// <returns>Returns S_OK on success.
            ///
            /// Returns DISMAPI_E_DISMAPI_ALREADY_INITIALIZED if DismInitialize has already been called by the process without a matching call to DismShutdown.
            ///
            /// Returns ERROR_ELEVATION_REQUIRED as an HRESULT if the process is not elevated.</returns>
            /// <remarks>The client code must call DismInitialize once per process. DISM API will serialize concurrent calls to DismInitialize. The first call will succeed and the others will fail. For more information, see Using the DISM API.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824803.aspx" />
            /// HRESULT WINAPI DismInitialize(_In_ DismLogLevel LogLevel, _In_opt_ PCWSTR LogFilePath, _In_opt_ PCWSTR ScratchDirectory);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismInitialize(DismLogLevel logLevel, string logFilePath, string scratchDirectory);
        }
    }
}