// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.XPath;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Stores information about an image in the Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of an image returned by the <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/> methods.</param>
        /// <param name="imageInfoXml">An <see cref="IXPathNavigable"/> object that contains information about the volume image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="wimHandle"/> or <paramref name="imageInfoXml"/> is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If the wimHandle parameter is from the <see cref="CreateFile"/> method, then the XML data must be enclosed by &lt;WIM&gt;&lt;/WIM&gt; tags. If the input handle is from the <see cref="LoadImage"/> or <see cref="CaptureImage"/> methods, then the XML data must be enclosed by &lt;IMAGE&gt;&lt;/IMAGE&gt; tags.</remarks>
        public static void SetImageInformation(WimHandle wimHandle, IXPathNavigable imageInfoXml)
        {
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            if (imageInfoXml == null)
            {
                throw new ArgumentNullException(nameof(imageInfoXml));
            }

            SetImageInformation(wimHandle, imageInfoXml.CreateNavigator()?.OuterXml);
        }

        /// <summary>
        /// Stores information about an image in the Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of an image returned by the <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/> methods.</param>
        /// <param name="imageInfoXml">A <see cref="String"/> object that contains information about the volume image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="wimHandle"/> or <paramref name="imageInfoXml"/> is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If the wimHandle parameter is from the <see cref="CreateFile"/> method, then the XML data must be enclosed by &lt;WIM&gt;&lt;/WIM&gt; tags. If the input handle is from the <see cref="LoadImage"/> or <see cref="CaptureImage"/> methods, then the XML data must be enclosed by &lt;IMAGE&gt;&lt;/IMAGE&gt; tags.</remarks>
        public static void SetImageInformation(WimHandle wimHandle, string imageInfoXml)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if imageInfoXml is null
            if (imageInfoXml == null)
            {
                throw new ArgumentNullException(nameof(imageInfoXml));
            }

            // Append a unicode file marker to the xml as a string
            string imageInfo = $"\uFEFF{imageInfoXml}";

            // Allocate enough memory for the info
            IntPtr imageInfoPtr = Marshal.StringToHGlobalUni(imageInfo);

            try
            {
                // Call the native function
                if (!WimgApi.NativeMethods.WIMSetImageInformation(wimHandle, imageInfoPtr, (DWORD)(imageInfo.Length + 1) * 2))
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }
            }
            finally
            {
                // Free the string buffer
                Marshal.FreeHGlobal(imageInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Stores information about an image in the Windows® image (.wim) file.
            /// </summary>
            /// <param name="hImage">A handle returned by the WIMCreateFile, WIMLoadImage, or WIMCaptureImage functions.</param>
            /// <param name="pvImageInfo">A pointer to a buffer that contains information about the volume image.</param>
            /// <param name="cbImageInfo">Specifies the size, in bytes, of the buffer pointed to by the pvImageInfo parameter.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// The data buffer being passed into the function must be the memory representation of a Unicode XML file. Calling this
            /// function replaces any customized image data, so, to preserve existing XML information, call the WIMGetImageInformation
            /// function and append or edit the data.
            /// If the input handle is from the WIMCreateFile function, then the XML data must be enclosed by <WIM></WIM> tags. If the
            /// input handle is from the WIMLoadImage or WIMCaptureImage functions, then the XML data must be enclosed by
            /// <IMAGE></IMAGE> tags.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetImageInformation(WimHandle hImage, IntPtr pvImageInfo, DWORD cbImageInfo);
        }
    }
}