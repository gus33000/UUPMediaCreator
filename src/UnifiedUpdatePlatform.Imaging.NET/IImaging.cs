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
using ManagedWimLib;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnifiedUpdatePlatform.Imaging.NET
{
    public interface IImaging
    {
        public delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

        public bool AddFileToImage(string wimFile, int imageIndex, string fileToAdd, string destination, ProgressCallback progressCallback = null);

        public bool UpdateFilesInImage(string wimFile, int imageIndex, IEnumerable<(string fileToAdd, string destination)> fileList, ProgressCallback progressCallback = null);

        public bool DeleteFileFromImage(string wimFile, int imageIndex, string fileToRemove, ProgressCallback progressCallback = null);

        public bool ExportImage(string wimFile, string destinationWimFile, int imageIndex, IEnumerable<string> referenceWIMs = null, WimCompressionType compressionType = WimCompressionType.Lzx, ProgressCallback progressCallback = null, ExportFlags exportFlags = ExportFlags.None);

        public bool ExtractFileFromImage(string wimFile, int imageIndex, string fileToExtract, string destination);

        public bool RenameFileInImage(string wimFile, int imageIndex, string sourceFilePath, string destinationFilePath, ProgressCallback progressCallback = null);

        public bool ApplyImage(string wimFile, int imageIndex, string OutputDirectory, IEnumerable<string> referenceWIMs = null, bool PreserveACL = true, ProgressCallback progressCallback = null);

        public bool CaptureImage(
             string wimFile,
             string imageName,
             string imageDescription,
             string imageFlag,
             string InputDirectory,
             TempManager.TempManager tempManager,
             string imageDisplayName = null,
             string imageDisplayDescription = null,
             WimCompressionType compressionType = WimCompressionType.Lzx,
             ProgressCallback progressCallback = null,
             int UpdateFrom = -1,
             bool PreserveACL = true);

        public bool EnumerateFiles(string wimFile, int imageIndex, string path, out string[] entries);

        public bool MarkImageAsBootable(string wimFile, int imageIndex);

        public bool GetWIMInformation(
             string wimFile,
             out WIMInformationXML.WIM wimInformationObject);

        public bool GetWIMImageInformation(
             string wimFile,
             int imageIndex,
             out WIMInformationXML.IMAGE image);

        public bool SetWIMImageInformation(
             string wimFile,
             int imageIndex,
             WIMInformationXML.IMAGE image);
    }
}