/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using CompDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsUpdateLib;

namespace UUPDownload
{
    public static class UpdateUtils
    {
        public static string GetFilenameForCEUIFile(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName.Replace('\\', Path.DirectorySeparatorChar);
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else if (!payloadItems.Any() && filename.Contains("_") && !filename.StartsWith("_") && (!filename.Contains('-') || filename.IndexOf('-') > filename.IndexOf('_')))
            {
                filename = filename.Substring(0, filename.IndexOf('_')) + Path.DirectorySeparatorChar + filename[(filename.IndexOf('_') + 1)..];
                filename = filename.TrimStart(Path.DirectorySeparatorChar);
            }
            return filename;
        }

        public static bool ShouldFileGetDownloaded(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName.Replace('\\', Path.DirectorySeparatorChar);
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path.Replace('\\', Path.DirectorySeparatorChar);

                if (payload.PayloadType.Equals("ExpressCab", StringComparison.InvariantCultureIgnoreCase))
                {
                    // This is a diff cab, skip it
                    return false;
                }
            }

            if (filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) ||
                filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }
    }
}