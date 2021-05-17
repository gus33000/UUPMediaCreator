using System;
using System.Collections.Generic;

namespace Cabinet
{
    public static class CabinetExtractor
    {
        public static IReadOnlyCollection<string> EnumCabinetFiles(string InputFile)
        {
            var cabFile = new CabinetFile(InputFile);
            return cabFile.Files;
        }

        /// <summary>
        /// Expands a cabinet file in pure C# (TM)
        /// Because nothing else god damn existed at the time of writing this
        /// and CAB is some archaic format that makes barely any sense in 2021
        /// at least for most people it seems
        /// TODO: Multi part
        /// TODO: CheckSum
        /// Relevant Documentation that might help at 20% only: https://interoperability.blob.core.windows.net/files/Archive_Exchange/%5bMS-CAB%5d.pdf
        /// </summary>
        /// <param name="InputFile">Input cabinet file</param>
        /// <param name="OutputDirectory">Output directory</param>
        public static void ExtractCabinet(string InputFile, string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            var cabFile = new CabinetFile(InputFile);
            cabFile.ExtractAllFiles(OutputDirectory, progressCallBack);
        }

        public static byte[] ExtractCabinetFile(string InputFile, string FileName)
        {
            var cabFile = new CabinetFile(InputFile);
            return cabFile.ReadFile(FileName);
        }
    }
}