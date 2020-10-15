namespace UUPMediaCreator.InterCommunication
{
    public class Common
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
            StartISOConversionProcess
        }

        public class InterCommunication
        {
            public InterCommunicationType InterCommunicationType { get; set; }
            public ISOConversionProgress ISOConversionProgress { get; set; }
            public ISOConversion ISOConversion { get; set; }
        }
    }
}