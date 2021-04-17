// Copyright (c) Gustave Monce and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using CompDB;
using Microsoft.Cabinet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WindowsUpdateLib;

namespace UUPDownload
{
    public static class UpdateUtils
    {
        public static string GetFilenameForCEUIFile(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName;
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path;
            }
            return filename;
        }

        public static bool ShouldFileGetDownloaded(CExtendedUpdateInfoXml.File file2, string OutputFolder, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName;
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path;

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

            string filenameonly = Path.GetFileName(filename);
            string outputPath = filename.Replace(filenameonly, "");

            if (File.Exists(Path.Combine(OutputFolder, outputPath, filenameonly + ".decrypted")))
            {
                Logging.Log("File " + Path.Combine(outputPath, filenameonly) + " (encrypted) already exists. Verifying if it's matching expectations.");
                byte[] expectedHash = Convert.FromBase64String(file2.Digest);

                if (file2.DigestAlgorithm.Equals("sha1", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logging.Log("Computing SHA1 hash...");
                    using SHA1 SHA1 = SHA1.Create();
                    byte[] hash;
                    using FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly + ".decrypted"));
                    hash = SHA1.ComputeHash(fileStream);

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                    {
                        Logging.Log("Hash matches! Skipping file");
                        return false;
                    }
                    else
                    {
                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly + ".decrypted"));
                        Logging.Log("File deleted");
                    }
                }
                else if (file2.DigestAlgorithm.Equals("sha256", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logging.Log("Computing SHA256 hash...");

                    using SHA256 SHA256 = SHA256.Create();
                    byte[] hash;
                    using FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly + ".decrypted"));
                    hash = SHA256.ComputeHash(fileStream);

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                    {
                        Logging.Log("Hash matches! Skipping file");
                        return false;
                    }
                    else
                    {
                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly + ".decrypted"));
                        Logging.Log("File deleted");
                    }
                }
            }
            else if (File.Exists(Path.Combine(OutputFolder, outputPath, filenameonly)))
            {
                Logging.Log("File " + Path.Combine(outputPath, filenameonly) + " already exists. Verifying if it's matching expectations.");
                byte[] expectedHash = Convert.FromBase64String(file2.Digest);

                if (file2.DigestAlgorithm.Equals("sha1", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logging.Log("Computing SHA1 hash...");
                    using SHA1 SHA1 = SHA1.Create();
                    byte[] hash;
                    using FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly));
                    hash = SHA1.ComputeHash(fileStream);

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                    {
                        Logging.Log("Hash matches! Skipping file");
                        return false;
                    }
                    else
                    {
                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                        Logging.Log("File deleted");
                    }
                }
                else if (file2.DigestAlgorithm.Equals("sha256", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logging.Log("Computing SHA256 hash...");

                    using SHA256 SHA256 = SHA256.Create();
                    byte[] hash;
                    using FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly));
                    hash = SHA256.ComputeHash(fileStream);

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                    {
                        Logging.Log("Hash matches! Skipping file");
                        return false;
                    }
                    else
                    {
                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                        Logging.Log("File deleted");
                    }
                }
            }

            return true;
        }

        public static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }
    }
}