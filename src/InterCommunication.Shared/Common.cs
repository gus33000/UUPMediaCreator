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
namespace UUPMediaCreator.InterCommunication
{
    public static class Common
    {
        public enum ProcessPhase
        {
            ReadingMetadata,
            PreparingFiles,
            CreatingWindowsInstaller,

            //IntegratingInstallerUpdates,
            ApplyingImage,

            //IntegratingUpdates,
            IntegratingWinRE,

            CapturingImage,
            CreatingISO,
            Error,
            Done
        }

        public enum CompressionType
        {
            XPRESS,
            LZX,
            LZMS
        }

        public class ISOConversion
        {
            public string ISOPath { get; set; }
            public string UUPPath { get; set; }
            public string Edition { get; set; }
            public string LanguageCode { get; set; }
            public bool IntegrateUpdates { get; set; }
            public CompressionType CompressionType { get; set; }
        }

        public class ISOConversionProgress
        {
            public ProcessPhase Phase { get; set; }
            public bool IsIndeterminate { get; set; }
            public int ProgressInPercentage { get; set; }
            public string SubOperation { get; set; }
        }

        public enum InterCommunicationType
        {
            Exit,
            ReportISOConversionProgress,
            StartISOConversionProcess,
            ReportPrivilege
        }

        public class InterCommunication
        {
            public InterCommunicationType InterCommunicationType { get; set; }
            public ISOConversionProgress ISOConversionProgress { get; set; }
            public ISOConversion ISOConversion { get; set; }
        }
    }
}
