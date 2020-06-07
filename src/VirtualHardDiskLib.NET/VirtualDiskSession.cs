using System;
using System.IO;

namespace VirtualHardDiskLib
{
    public class VirtualDiskSession : IDisposable
    {
        private int DiskId;
        private char DriveLetter = 'B';
        public string VirtualDiskPath;
        private bool delete;

        public VirtualDiskSession(long sizeInGB = 20, bool delete = true, string existingVHD = null)
        {
            this.delete = delete;
            if (string.IsNullOrEmpty(existingVHD))
            {
                VirtualDiskPath = VHDUtilities.CreateVirtualDisk(sizeInGB);
            }
            else
            {
                VirtualDiskPath = existingVHD;
            }
            DiskId = VHDUtilities.MountVirtualDisk(VirtualDiskPath);
            VHDUtilities.AttachDriveLetterToDiskAndPartitionId(DiskId, 1, DriveLetter);
        }

        public string GetMountedPath()
        {
            return DriveLetter + ":";
        }

        public void Dispose()
        {
            VHDUtilities.DismountVirtualDisk(VirtualDiskPath);
            if (delete)
                File.Delete(VirtualDiskPath);
        }
    }
}