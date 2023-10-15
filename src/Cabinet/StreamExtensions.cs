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
using System.IO;
using System.Runtime.InteropServices;

namespace Cabinet
{
    internal static class StreamExtensions
    {
        internal static T ReadStruct<T>(this Stream stream) where T : struct
        {
            int sz = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[sz];
            _ = stream.Read(buffer, 0, sz);
            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(
                pinnedBuffer.AddrOfPinnedObject(), typeof(T));
            pinnedBuffer.Free();
            return structure;
        }

        internal static string ReadString(this Stream stream)
        {
            byte[] nameBuffer = new byte[256];

            int j = 0;
            for (; j < 256; j++)
            {
                nameBuffer[j] = (byte)stream.ReadByte();
                if (nameBuffer[j] == 0)
                {
                    break;
                }
            }

            return System.Text.Encoding.ASCII.GetString(nameBuffer, 0, j);
        }

        internal static string ReadUTF8tring(this Stream stream)
        {
            byte[] nameBuffer = new byte[256];

            int j = 0;
            for (; j < 256; j++)
            {
                nameBuffer[j] = (byte)stream.ReadByte();
                nameBuffer[j + 1] = (byte)stream.ReadByte();
                _ = stream.Seek(-1, SeekOrigin.Current);
                if (nameBuffer[j] == 0 && nameBuffer[j + 1] == 0)
                {
                    break;
                }
            }

            return System.Text.Encoding.UTF8.GetString(nameBuffer, 0, j);
        }
    }
}
