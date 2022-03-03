﻿/*
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
using System;
using System.IO;
using System.Linq;

namespace VirtualHardDiskLib
{
    public class VirtualDiskSession : IDisposable
    {
        private readonly int DiskId;
        private readonly char DriveLetter;
        public string VirtualDiskPath;
        private readonly bool delete;

        public VirtualDiskSession(long sizeInGB = 20, bool delete = true, string existingVHD = null)
        {
            DriveLetter = GetNextAvailableDriveLetter();

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
            {
                File.Delete(VirtualDiskPath);
            }

            GC.SuppressFinalize(this);
        }

        private static char GetNextAvailableDriveLetter()
        {
            var allocatedLetters = Directory.GetLogicalDrives().Select(d => d.First());
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".First(letter => !allocatedLetters.Contains(letter));
        }
    }
}