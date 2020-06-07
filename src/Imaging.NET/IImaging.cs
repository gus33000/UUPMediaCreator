using Microsoft.Wim;
using System.Collections.Generic;

namespace Imaging
{
    public interface IImaging
    {
        public delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

        public abstract bool MarkImageAsBootable(string wimFile, int imageIndex);

        public abstract bool ExtractFileFromImage(string wimFile, int imageIndex, string fileToExtract, string destination);

        public bool AddFileToImage(
            string wimFile,
            int imageIndex,
            string fileToAdd,
            string destination,
            ProgressCallback progressCallback = null);

        public abstract bool GetWIMImageInformation(
            string wimFile,
            int imageIndex,
            out WIMInformationXML.IMAGE image);

        public bool DeleteFileFromImage(
            string wimFile,
            int imageIndex,
            string fileToRemove,
            ProgressCallback progressCallback = null);

        public bool RenameFileInImage(
            string wimFile,
            int imageIndex,
            string sourceFilePath,
            string destinationFilePath,
            ProgressCallback progressCallback = null);

        public bool SetWIMImageInformation(
            string wimFile,
            int imageIndex,
            WIMInformationXML.IMAGE image);

        public bool GetWIMInformation(
            string wimFile,
            out WIMInformationXML.WIM wim);

        public bool ExportImage(
            string wimFile,
            string destinationWimFile,
            int imageIndex,
            IEnumerable<string> referenceWIMs = null,
            WimCompressionType compressionType = WimCompressionType.Lzx,
            IImaging.ProgressCallback progressCallback = null);

        public abstract bool ApplyImage(
            string wimFile,
            int imageIndex,
            string OutputDirectory,
            IEnumerable<string> referenceWIMs = null,
            bool PreserveACL = true,
            ProgressCallback progressCallback = null);

        public bool CaptureImage(
            string wimFile,
            string imageName,
            string imageDescription,
            string imageFlag,
            string InputDirectory,
            string imageDisplayName = null,
            string imageDisplayDescription = null,
            WimCompressionType compressionType = WimCompressionType.Lzx,
            ProgressCallback progressCallback = null,
            int UpdateFrom = -1,
            bool PreserveACL = true);
    }
}
