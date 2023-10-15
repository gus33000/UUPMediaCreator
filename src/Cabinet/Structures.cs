/*
 * Copyright (c) Gustave Monce
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
using System.Runtime.InteropServices;

namespace Cabinet
{
    internal struct CFHEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal byte[] signature;
        internal uint reserved1;
        internal uint cbCabinet;
        internal uint reserved2;
        internal uint coffFiles;
        internal uint reserved3;
        internal byte versionMinor;
        internal byte versionMajor;
        internal ushort cFolders;
        internal ushort cFiles;
        internal Options flags;
        internal ushort setID;
        internal ushort iCabinet;

        [Flags]
        internal enum Options : ushort
        {
            PreviousCabinet = 0x001,
            NextCabinet = 0x002,
            ReservePresent = 0x004
        }
    }

    internal struct CFFOLDER
    {
        internal uint firstDataBlockOffset;
        internal ushort dataBlockCount;
        internal CFTYPECOMPRESS typeCompress;
        internal byte typeCompressOption;

        internal enum CFTYPECOMPRESS : byte
        {
            TYPE_NONE = 0,
            TYPE_MSZIP = 1,
            TYPE_QUANTUM = 2,
            TYPE_LZX = 3,
        }
    }

    internal struct CFFILE
    {
        /// <summary>
        /// Specifies the uncompressed size of this file, in bytes.
        /// </summary>
        internal uint cbFile;

        /// <summary>
        /// Specifies the uncompressed offset, in bytes, of the start of this file's data. For the
        /// first file in each folder, this value will usually be zero.Subsequent files in the folder will have offsets
        /// that are typically the running sum of the cbFile field values.
        /// </summary>
        internal uint uoffFolderStart;

        internal ushort iFolder;
        internal ushort date;
        internal ushort time;
        internal CFFILEATTRIBUTES attribs;

        [Flags]
        internal enum CFFILEATTRIBUTES : ushort
        {
            A_RDONLY = 1 << 1,
            A_HIDDEN = 1 << 2,
            A_SYSTEM = 1 << 3,
            A_ARCH = 1 << 6,
            A_EXEC = 1 << 7,
            A_NAME_IS_UTF = 1 << 8,
        }

        internal readonly DateTime GetDateTime()
        {
            return new DateTime(((date >> 9) & 0b1111_111) + 1980, (date >> 5) & 0b1111, date & 0b1111_1, (time >> 11) & 0b1111_1, (time >> 5) & 0b1111_11, (time & 0b1111_1) * 2);
        }

        internal readonly FileAttributes GetFileAttributes()
        {
            FileAttributes a = 0;

            if ((attribs & CFFILEATTRIBUTES.A_ARCH) != 0)
            {
                a |= FileAttributes.Archive;
            }

            if ((attribs & CFFILEATTRIBUTES.A_HIDDEN) != 0)
            {
                a |= FileAttributes.Hidden;
            }

            if ((attribs & CFFILEATTRIBUTES.A_RDONLY) != 0)
            {
                a |= FileAttributes.ReadOnly;
            }

            if ((attribs & CFFILEATTRIBUTES.A_SYSTEM) != 0)
            {
                a |= FileAttributes.System;
            }

            return a;
        }

        internal readonly bool IsFileNameUTF8()
        {
            return (attribs & CFFILEATTRIBUTES.A_NAME_IS_UTF) != 0;
        }

        internal readonly bool ShouldBeExecutedAfterExtraction()
        {
            return (attribs & CFFILEATTRIBUTES.A_EXEC) != 0;
        }
    }

    internal struct CFDATA
    {
        internal uint csum;
        internal ushort cbData;
        internal ushort cbUncomp;
    }
}
