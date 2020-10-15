// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

/*
 OVERVIEW:

 The Managed DISM wrapper abstracts the native Win32 DISM API and makes a strong effort to cleanly wrap it's functionality. This class (DismUtilities.cs) exists to provide additional support to a
 DISM API user, which expands upon the native libraries by introducing new functionality that is not provided in the native Win32 API.

 DISM:

 DISM has not always been built-in to Windows, and it's lineage traces back to ImageX, which was the (Microsoft) recommended imaging tool during the Windows Vista era.
 With the release of Windows 7 (and each subsequent Windows build, through 10.0.16299.192 [as of this writing]), DISM in incorporated into Windows(in \System32).

 With respect to the Managed DISM wrapper, initializing the built-in version of DISM can be achieved by use of "DismApi.Initialize()".

 DISM(and other Microsoft tooling for Windows image management and deployment) is provided in the Windows Assessment and Installation Kit(WAIK), which was first made available for
 Windows 7. Starting with Windows 8.0, Microsoft has renamed this package to the Windows Assessment and Deployment Kit (WADK).

 The versions of tooling as of this writing are:

 Operating System             Tooling Name        WinPE Version

 Windows 7                    WAIK                3.0
 Windows 7 Service Pack 1     WAIK                3.1
 Windows 8.0                  WADK                4.0
 Windows 8.1                  WADK                5.0
 Windows 10.x WADK                10.x

 DISM TOOLING:

 -    WAIK (Windows 7) can be installed to provide Windows PE 3.0 (and DISM servicing), OR WAIK Service Pack 1 can be installed for Windows PE 3.1 (and DISM servicing), but both cannot be
      installed at the same time.

 -    WADK (Windows 8.0) can be installed to provide support for Windows PE 4.0 (and DISM servicing), OR WADK (Windows 8.1) can be installed for Windows PE 5.0 (and DISM servicing), but both
      cannot be installed at the same time.

 -    Only one version of WADK (Windows 10.x) can be installed to provide support for Windows PE 10 (and DISM servicing) at the same time.

 Thus, a user could potentially have WAIK 3.0 or 3.1 installed, WADK 4.0 or 5.0 installed, and WADK 10 installed, all at the same time. In doing so, they could create and service Windows PE images
 from Windows 7/7 SP1, 8/8.1, and 10.

 Regardless of what OS you (or a user) has, it is recommended to install the latest version of the WADK (10.x as of this writing), as the latest tooling should contain support for and be able to service older images.
 Therefore, WADK 10.x (and it's DISM libraries) can service images of Windows 7, 8, 8.1, 10.x.

 LOADING A SPECIFIC DISM GENERATIONAL LIBRARY:

 DismApi.InitializeEx() has been provided to make loading (and unloading) generational DISM libraries easy and are modeled after the Managed DISM wrapper. By default, this function looks for the latest DISM generational
 library available on the system and attempts to load it (instead of the default system library.) Calling DismApi.Shutdown() will unload any generational library that was loaded by DismApi.InitializeEx().

 Interested users can use the overloaded version of DismApi.InitializeEx() to load a generational library of their choosing, and/or use or modify the code in this file to assist in recognizing what version(s) of DISM
 are available on a local system and load a generational library selectively.

 PLATFORM AND/OR ARCHITECTURE SPECIFIC:

 DismApi.InitializeEx() supports ANYCPU, X86, and AMD64, and also supports the CLR "Prefer 32-bit" setting. DismApi.Initialize() may not support these settings, as it loads the system-specific version of DISM, which can
 only support the system-specific architecture of the system it's running on.
*/

using System;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Gets or sets the DISM Generational Library initialized for use with the DismApi Wrapper (via InitializeEx()). Returns the specific DismGeneration in use; otherwise, returns DismGeneration.NotFound.
        /// </summary>
        private static DismGeneration CurrentDismGeneration { get; set; } = DismGeneration.NotFound;

        /// <summary>
        /// Initializes DISM API, using the latest installed DISM Generation. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <exception cref="Exception">If an error occurs loading the latest DISM Generational Library.</exception>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void InitializeEx(DismLogLevel logLevel)
        {
            InitializeEx(logLevel, null, null, DismUtilities.GetLatestDismGeneration());
        }

        /// <summary>
        /// Initializes DISM API, using the latest installed DISM Generation. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <param name="logFilePath">A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
        /// <exception cref="Exception">If an error occurs loading the latest DISM Generational Library.</exception>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void InitializeEx(DismLogLevel logLevel, string logFilePath)
        {
            InitializeEx(logLevel, logFilePath, null, DismUtilities.GetLatestDismGeneration());
        }

        /// <summary>
        /// Initializes DISM API, using the latest installed DISM Generation. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <param name="logFilePath">A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
        /// <param name="scratchDirectory">A relative or absolute path to a scratch directory. DISM API will use this directory for internal operations. If null, the default temp directory, \Windows\%Temp%, will be used.</param>
        /// /// <exception cref="Exception">If an error occurs loading the latest DISM Generational Library.</exception>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void InitializeEx(DismLogLevel logLevel, string logFilePath, string scratchDirectory)
        {
            InitializeEx(logLevel, logFilePath, scratchDirectory, DismUtilities.GetLatestDismGeneration());
        }

        /// <summary>
        /// Initializes DISM API, using the specified DISM Generation. Initialize must be called once per process before calling any other DISM API functions.
        /// </summary>
        /// <param name="logLevel">Indicates the level of logging.</param>
        /// <param name="logFilePath">A relative or absolute path to a log file. All messages generated will be logged to this path. If NULL, the default log path, %windir%\Logs\DISM\dism.log, will be used.</param>
        /// <param name="scratchDirectory">A relative or absolute path to a scratch directory. DISM API will use this directory for internal operations. If null, the default temp directory, \Windows\%Temp%, will be used.</param>
        /// <param name="dismGeneration">The DISM Generational Library to be used.</param>
        /// /// <exception cref="Exception">If an error occurs loading the latest DISM Generational Library.</exception>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void InitializeEx(DismLogLevel logLevel, string logFilePath, string scratchDirectory, DismGeneration dismGeneration)
        {
            if (CurrentDismGeneration != DismGeneration.NotFound)
            {
                throw new Exception($"A DISM Generation library is already loaded ({dismGeneration}). Please call Shutdown() first to release the existing library.");
            }

            if (dismGeneration != DismGeneration.NotFound && !DismUtilities.LoadDismGenerationLibrary(dismGeneration))
            {
                throw new Exception($"Loading the latest DISM Generation library ({dismGeneration}) failed.");
            }

            Initialize(logLevel, logFilePath, scratchDirectory);

            CurrentDismGeneration = dismGeneration;
        }
    }
}