// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Gets information about an image within the .wim (Windows image) file.
        /// </summary>
        /// <param name="wimHandle">Either a <see cref="WimHandle"/> returned from <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/>.</param>
        /// <returns>An <see cref="IXPathNavigable"/> object containing XML information about the volume image.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static IXPathNavigable GetImageInformation(WimHandle wimHandle)
        {
            string xml = GetImageInformationAsString(wimHandle);

            if (xml == null)
            {
                return null;
            }

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            };

            using (StringReader stringReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    return new XPathDocument(xmlReader);
                }
            }
        }

        /// <summary>
        /// Gets information about an image within the .wim (Windows image) file.
        /// </summary>
        /// <param name="wimHandle">Either a <see cref="WimHandle"/> returned from <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/>.</param>
        /// <returns>A <see cref="String"/> object containing XML information about the volume image.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static string GetImageInformationAsString(WimHandle wimHandle)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Stores the native pointer to the Unicode xml
            IntPtr imageInfoPtr = IntPtr.Zero;

            try
            {
                // Call the native function
                if (!NativeMethods.WIMGetImageInformation(wimHandle, out imageInfoPtr, out DWORD _))
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }

                // Marshal the buffer as a Unicode string and remove the Unicode file marker
                return Marshal.PtrToStringUni(imageInfoPtr)?.Substring(1);
            }
            finally
            {
                // Free the native pointer
                Marshal.FreeHGlobal(imageInfoPtr);
            }
        }

        /// <summary>
        /// Gets information about an image within the .wim (Windows image) file.
        /// </summary>
        /// <param name="wimHandle">Either a <see cref="WimHandle"/> returned from <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/>.</param>
        /// <returns>AN <see cref="XDocument"/> object containing XML information about the volume image.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static XDocument GetImageInformationAsXDocument(WimHandle wimHandle)
        {
            string xml = GetImageInformationAsString(wimHandle);

            return xml == null ? null : XDocument.Parse(xml);
        }

        /// <summary>
        /// Gets information about an image within the .wim (Windows image) file.
        /// </summary>
        /// <param name="wimHandle">Either a <see cref="WimHandle"/> returned from <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/>.</param>
        /// <returns>AN <see cref="XDocument"/> object containing XML information about the volume image.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static XmlDocument GetImageInformationAsXmlDocument(WimHandle wimHandle)
        {
            string xml = GetImageInformationAsString(wimHandle);

            if (xml == null)
            {
                return null;
            }

            XmlDocument xmlDocument = new XmlDocument
            {
                XmlResolver = null
            };

            using (StringReader stringReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = new XmlTextReader(stringReader)
                {
                    DtdProcessing = DtdProcessing.Prohibit
                })
                {
                    xmlDocument.Load(xmlReader);
                }
            }

            return xmlDocument;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns information about an image within the .wim (Windows image) file.
            /// </summary>
            /// <param name="hImage">A handle returned by the WIMCreateFile, WIMLoadImage, or WIMCaptureImage function.</param>
            /// <param name="ppvImageInfo">
            /// A pointer to a buffer that receives the address of the XML information about the volume
            /// image. When the function returns, this value contains the address of an allocated buffer, containing XML information
            /// about the volume image.
            /// </param>
            /// <param name="pcbImageInfo">
            /// A pointer to a variable that specifies the size, in bytes, of the buffer pointed to by the
            /// value of the ppvImageInfo parameter.
            /// </param>
            /// <returns><code>true</code> if the function succeeded, otherwise <code>false</code>.</returns>
            /// <remarks>
            /// When the function succeeds, then the data describing the image is in Unicode XML format. Use the LocalFree
            /// function to free the memory pointed to by the ppvImageInfo parameter when no longer needed.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMGetImageInformation(WimHandle hImage, out IntPtr ppvImageInfo, out DWORD pcbImageInfo);
        }
    }
}