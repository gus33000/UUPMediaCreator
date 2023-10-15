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
using DiscUtils;
using DiscUtils.Ntfs;
using DiscUtils.Partitions;
using DiscUtils.Vhd;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnifiedUpdatePlatform.Services.Temp;

namespace VirtualHardDiskLib
{
    public static class VHDUtilities
    {
        /// <summary>
        /// Creates a temporary vhd of the given size in GB.
        /// The created VHD is dynamically allocated and is of type VHD (legacy)
        /// </summary>
        /// <param name="sizeInGB">The size of the VHD in GB</param>
        /// <returns>The path to the created vhd</returns>
        internal static string CreateVirtualDisk(TempManager tempManager, long sizeInGB = 10)
        {
            long diskSize = sizeInGB * 1024 * 1024 * 1024;
            string tempVhd = tempManager.GetTempPath();

            using Stream vhdStream = File.Create(tempVhd);
            using Disk disk = Disk.InitializeDynamic(vhdStream, DiscUtils.Streams.Ownership.Dispose, diskSize);

            BiosPartitionTable table = BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsNtfs);
            PartitionInfo ntfsPartition = table.Partitions[0];
            _ = NtfsFileSystem.Format(ntfsPartition.Open(), "Windows UUP Medium", Geometry.FromCapacity(diskSize), ntfsPartition.FirstSector, ntfsPartition.SectorCount);

            return tempVhd;
        }

        public static string CreateDiffDisk(string OriginalVirtualDisk, TempManager tempManager)
        {
            string tempVhd = tempManager.GetTempPath();
            Disk.InitializeDifferencing(tempVhd, OriginalVirtualDisk).Dispose();
            return tempVhd;
        }

        internal static int MountVirtualDisk(string vhdfile)
        {
            IntPtr handle = IntPtr.Zero;

            // open disk handle
            NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            NativeMethods.VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_VHD,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT
            };

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile,
                NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL,
                NativeMethods.OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);
            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.",
                    openResult));
            }

            // attach disk - permanently
            NativeMethods.ATTACH_VIRTUAL_DISK_PARAMETERS attachParameters = new()
            {
                Version = NativeMethods.ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1
            };
            int attachResult = NativeMethods.AttachVirtualDisk(handle, IntPtr.Zero,
                NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME | NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_NO_DRIVE_LETTER, 0,
                ref attachParameters, IntPtr.Zero);
            if (attachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.",
                    attachResult));
            }

            int num = _findVhdPhysicalDriveNumber(handle);

            // close handle to disk
            _ = NativeMethods.CloseHandle(handle);

            return num;
        }

        internal static void DismountVirtualDisk(string vhdfile)
        {
            IntPtr handle = IntPtr.Zero;

            // open disk handle
            NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            NativeMethods.VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_VHD,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT
            };

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile,
                NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL,
                NativeMethods.OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);
            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.",
                    openResult));
            }

            // detach disk
            int detachResult = NativeMethods.DetachVirtualDisk(handle,
                NativeMethods.DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0);
            if (detachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.",
                    detachResult));
            }

            // close handle to disk
            _ = NativeMethods.CloseHandle(handle);
        }

        private static int _findVhdPhysicalDriveNumber(IntPtr vhdHandle)
        {
            int bufferSize = 260;
            StringBuilder vhdPhysicalPath = new(bufferSize);

            _ = NativeMethods.GetVirtualDiskPhysicalPath(vhdHandle, ref bufferSize, vhdPhysicalPath);
            _ = int.TryParse(Regex.Match(vhdPhysicalPath.ToString(), @"\d+").Value, out int driveNumber);
            return driveNumber;
        }

        private static string _findVhdVolumePath(int vhdPhysicalDrive)
        {
            StringBuilder volumeName = new(260);
            IntPtr findVolumeHandle;
            IntPtr volumeHandle;
            NativeMethods.STORAGE_DEVICE_NUMBER deviceNumber = new();
            uint bytesReturned = 0;
            bool found = false;

            findVolumeHandle = NativeMethods.FindFirstVolume(volumeName, volumeName.Capacity);
            do
            {
                int backslashPos = volumeName.Length - 1;
                if (volumeName[backslashPos] == Path.DirectorySeparatorChar)
                {
                    volumeName.Length--;
                }
                volumeHandle = NativeMethods.CreateFile(volumeName.ToString(), 0, NativeMethods.FILE_SHARE_MODE_FLAGS.FILE_SHARE_READ | NativeMethods.FILE_SHARE_MODE_FLAGS.FILE_SHARE_WRITE,
                    IntPtr.Zero, NativeMethods.CREATION_DISPOSITION_FLAGS.OPEN_EXISTING, 0, IntPtr.Zero);
                if (volumeHandle == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    continue;
                }

                _ = NativeMethods.DeviceIoControl(volumeHandle, NativeMethods.IO_CONTROL_CODE.STORAGE_DEVICE_NUMBER, IntPtr.Zero, 0,
                    ref deviceNumber, (uint)Marshal.SizeOf(deviceNumber), ref bytesReturned, IntPtr.Zero);

                if (deviceNumber.deviceNumber == vhdPhysicalDrive)
                {
                    found = true;
                    break;
                }
            } while (NativeMethods.FindNextVolume(findVolumeHandle, volumeName, volumeName.Capacity));
            _ = NativeMethods.FindVolumeClose(findVolumeHandle);
            return found ? volumeName.ToString() : ""; //when It returns "" then the error occurs
        }

        private static void _mountVhdToDriveLetter(string vhdVolumePath, string mountPoint)
        {
            if (vhdVolumePath[^1] != Path.DirectorySeparatorChar)
            {
                vhdVolumePath += Path.DirectorySeparatorChar;
            }

            if (!NativeMethods.SetVolumeMountPoint(mountPoint, vhdVolumePath))
            {
                throw new Exception("The VHD cannot be accessed [SetVolumeMountPoint failed]");
            }
        }

        internal static void AttachDriveLetterToDiskAndPartitionId(int diskid, int partid, char driveletter)
        {
            RemoveFileExplorerAutoRun(driveletter);
            string volpath = _findVhdVolumePath(diskid);
            _mountVhdToDriveLetter(volpath, driveletter + ":\\");
        }

        /// <summary>
        /// Removing file explorer auto run for the given DriveLetter so that when a vhd is mounted file explorer doesn't open
        /// </summary>
        /// <param name="DriveLetter"></param>
        private static void RemoveFileExplorerAutoRun(char DriveLetter)
        {
            const string KeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";
            RegistryKey AutoRunKey = Registry.CurrentUser.OpenSubKey(KeyPath, true);
            int DriveLetterValue = DriveLetter - 'A';

            if (AutoRunKey != null)
            {
                RemoveFileExplorerAutoRun(AutoRunKey, DriveLetterValue);
            }
            else // create key as it does not exist
            {
                AutoRunKey = Registry.CurrentUser.CreateSubKey(KeyPath);
                RemoveFileExplorerAutoRun(AutoRunKey, DriveLetterValue);
            }
        }

        private static void RemoveFileExplorerAutoRun(RegistryKey AutoRunKey, int DriveLetterValue)
        {
            if (AutoRunKey != null)
            {
                AutoRunKey.SetValue("NoDriveTypeAutoRun", DriveLetterValue);
                AutoRunKey.Close();
            }
        }
    }
}