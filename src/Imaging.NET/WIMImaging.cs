using Microsoft.Wim.NET;
using System.Collections.Generic;

namespace Imaging.NET
{
    public class WimImaging : IImaging
    {
        private readonly IImaging WimgApi = new WimgApiImaging();
        private readonly IImaging WimLib = new WimLibImaging();

        public bool AddFileToImage(string wimFile, int imageIndex, string fileToAdd, string destination, IImaging.ProgressCallback progressCallback = null)
        {
            return WimgApi.AddFileToImage(wimFile, imageIndex, fileToAdd, destination, progressCallback) || WimLib.AddFileToImage(wimFile, imageIndex, fileToAdd, destination, progressCallback);
        }

        public bool ApplyImage(string wimFile, int imageIndex, string OutputDirectory, IEnumerable<string> referenceWIMs = null, bool PreserveACL = true, IImaging.ProgressCallback progressCallback = null)
        {
            return WimgApi.ApplyImage(wimFile, imageIndex, OutputDirectory, referenceWIMs, PreserveACL, progressCallback) || WimLib.ApplyImage(wimFile, imageIndex, OutputDirectory, referenceWIMs, PreserveACL, progressCallback);
        }

        public bool CaptureImage(string wimFile, string imageName, string imageDescription, string imageFlag, string InputDirectory, TempManager.TempManager tempManager, string imageDisplayName = null, string imageDisplayDescription = null, WimCompressionType compressionType = WimCompressionType.Lzx, IImaging.ProgressCallback progressCallback = null, int UpdateFrom = -1, bool PreserveACL = true)
        {
            return WimgApi.CaptureImage(wimFile, imageName, imageDescription, imageFlag, InputDirectory, tempManager, imageDisplayName = null, imageDisplayDescription = null, compressionType, progressCallback, UpdateFrom, PreserveACL) ||
                WimLib.CaptureImage(wimFile, imageName, imageDescription, imageFlag, InputDirectory, tempManager, imageDisplayName = null, imageDisplayDescription = null, compressionType, progressCallback, UpdateFrom, PreserveACL);
        }

        public bool DeleteFileFromImage(string wimFile, int imageIndex, string fileToRemove, IImaging.ProgressCallback progressCallback = null)
        {
            return WimgApi.DeleteFileFromImage(wimFile, imageIndex, fileToRemove, progressCallback) || WimLib.DeleteFileFromImage(wimFile, imageIndex, fileToRemove, progressCallback);
        }

        public bool EnumerateFiles(string wimFile, int imageIndex, string path, out string[] entries)
        {
            return WimgApi.EnumerateFiles(wimFile, imageIndex, path, out entries) || WimLib.EnumerateFiles(wimFile, imageIndex, path, out entries);
        }

        public bool ExportImage(string wimFile, string destinationWimFile, int imageIndex, IEnumerable<string> referenceWIMs = null, WimCompressionType compressionType = WimCompressionType.Lzx, IImaging.ProgressCallback progressCallback = null)
        {
            return WimgApi.ExportImage(wimFile, destinationWimFile, imageIndex, referenceWIMs, compressionType, progressCallback) || WimLib.ExportImage(wimFile, destinationWimFile, imageIndex, referenceWIMs, compressionType, progressCallback);
        }

        public bool ExtractFileFromImage(string wimFile, int imageIndex, string fileToExtract, string destination)
        {
            return WimgApi.ExtractFileFromImage(wimFile, imageIndex, fileToExtract, destination) || WimLib.ExtractFileFromImage(wimFile, imageIndex, fileToExtract, destination);
        }

        public bool GetWIMImageInformation(string wimFile, int imageIndex, out WIMInformationXML.IMAGE image)
        {
            return WimgApi.GetWIMImageInformation(wimFile, imageIndex, out image) || WimLib.GetWIMImageInformation(wimFile, imageIndex, out image);
        }

        public bool GetWIMInformation(string wimFile, out WIMInformationXML.WIM wim)
        {
            return WimgApi.GetWIMInformation(wimFile, out wim) || WimLib.GetWIMInformation(wimFile, out wim);
        }

        public bool MarkImageAsBootable(string wimFile, int imageIndex)
        {
            return WimgApi.MarkImageAsBootable(wimFile, imageIndex) || WimLib.MarkImageAsBootable(wimFile, imageIndex);
        }

        public bool RenameFileInImage(string wimFile, int imageIndex, string sourceFilePath, string destinationFilePath, IImaging.ProgressCallback progressCallback = null)
        {
            return WimgApi.RenameFileInImage(wimFile, imageIndex, sourceFilePath, destinationFilePath) || WimLib.RenameFileInImage(wimFile, imageIndex, sourceFilePath, destinationFilePath);
        }

        public bool SetWIMImageInformation(string wimFile, int imageIndex, WIMInformationXML.IMAGE image)
        {
            return WimgApi.SetWIMImageInformation(wimFile, imageIndex, image) || WimLib.SetWIMImageInformation(wimFile, imageIndex, image);
        }
    }
}
