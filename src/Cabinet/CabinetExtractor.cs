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
using System.Collections.Generic;
using System.IO;

namespace Cabinet
{
    public static class CabinetExtractor
    {
        public static IReadOnlyCollection<CabinetFile> EnumCabinetFiles(string InputFile)
        {
            using FileStream strm = File.OpenRead(InputFile);
            Cabinet cabFile = new(strm);
            return EnumCabinetFiles(cabFile);
        }

        public static IReadOnlyCollection<CabinetFile> EnumCabinetFiles(Cabinet cabFile)
        {
            return cabFile.Files;
        }

        /// <summary>
        /// Expands a cabinet file in pure C# (TM)
        /// Because nothing else god damn existed at the time of writing this
        /// and CAB is some archaic format that makes barely any sense in 2021
        /// at least for most people it seems
        /// TODO: Multi part
        /// TODO: CheckSum
        /// Relevant Documentation that might help at 20% only: https://interoperability.blob.core.windows/files/Archive_Exchange/%5bMS-CAB%5d.pdf
        /// </summary>
        /// <param name="InputFile">Input cabinet file</param>
        /// <param name="OutputDirectory">Output directory</param>
        public static void ExtractCabinet(string InputFile, string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            using FileStream strm = File.OpenRead(InputFile);
            Cabinet cabFile = new(strm);
            ExtractCabinet(cabFile, OutputDirectory, progressCallBack);
        }

        /// <summary>
        /// Expands a cabinet file in pure C# (TM)
        /// Because nothing else god damn existed at the time of writing this
        /// and CAB is some archaic format that makes barely any sense in 2021
        /// at least for most people it seems
        /// TODO: Multi part
        /// TODO: CheckSum
        /// Relevant Documentation that might help at 20% only: https://interoperability.blob.core.windows/files/Archive_Exchange/%5bMS-CAB%5d.pdf
        /// </summary>
        /// <param name="InputFile">Input cabinet file</param>
        /// <param name="OutputDirectory">Output directory</param>
        public static void ExtractCabinet(Cabinet cabFile, string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            cabFile.ExtractAllFiles(OutputDirectory, progressCallBack);
        }

        public static byte[] ExtractCabinetFile(string InputFile, string FileName)
        {
            using FileStream strm = File.OpenRead(InputFile);
            Cabinet cabFile = new(strm);
            return ExtractCabinetFile(cabFile, FileName);
        }

        public static byte[] ExtractCabinetFile(Cabinet cabFile, string FileName)
        {
            return cabFile.ReadFile(FileName);
        }

        public static IReadOnlyCollection<CabinetFile> EnumCabinetFiles(Stream InputFile)
        {
            Cabinet cabFile = new(InputFile);
            return EnumCabinetFiles(cabFile);
        }

        /// <summary>
        /// Expands a cabinet file in pure C# (TM)
        /// Because nothing else god damn existed at the time of writing this
        /// and CAB is some archaic format that makes barely any sense in 2021
        /// at least for most people it seems
        /// TODO: Multi part
        /// TODO: CheckSum
        /// Relevant Documentation that might help at 20% only: https://interoperability.blob.core.windows/files/Archive_Exchange/%5bMS-CAB%5d.pdf
        /// </summary>
        /// <param name="InputFile">Input cabinet file</param>
        /// <param name="OutputDirectory">Output directory</param>
        public static void ExtractCabinet(Stream InputFile, string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            Cabinet cabFile = new(InputFile);
            ExtractCabinet(cabFile, OutputDirectory, progressCallBack);
        }

        public static byte[] ExtractCabinetFile(Stream InputFile, string FileName)
        {
            Cabinet cabFile = new(InputFile);
            return ExtractCabinetFile(cabFile, FileName);
        }
    }
}