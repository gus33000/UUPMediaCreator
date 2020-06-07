using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UUPMediaCreator.InterCommunication;

namespace UUPMediaConverterCli
{
    class Program
    {
        public static string GetExecutableDirectory()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains("\\") ? string.Join("\\", fileName.Split('\\').Reverse().Skip(1).Reverse()) : "";
        }

        public static string GetParentExecutableDirectory()
        {
            var runningDirectory = GetExecutableDirectory();
            return runningDirectory.Contains("\\") ? string.Join("\\", runningDirectory.Split('\\').Reverse().Skip(1).Reverse()) : "";
        }

        static void Main(string[] args)
        {
            /*MediaCreationLib.UUPMediaCreator.ProvisionMissingApps();
            Console.ReadLine();
            return;*/

            Log("");
            Log("UUP Media Converter CLI v0.2");
            Log("Copyright (c) 2020");
            Log("");

            /*if (args.Length < 4)
            {
                Log("Usage: MediaConverterCli.exe <UUP File set path> <Destination ISO file> <Edition> <Language Code>");
                return;
            }

            string UUPPath = args[0];
            string DestinationISO = args[1];
            string Edition = args[2];
            string LanguageCode = args[3];*/

            if (args.Length < 3)
            {
                Log("Usage: MediaConverterCli.exe <UUP File set path> <Destination ISO file> <Language Code>");
                return;
            }

            string UUPPath = args[0];
            string DestinationISO = args[1];
            string LanguageCode = args[2];

            Log("WARNING: PRE-RELEASE SOFTWARE WITH NO EXPRESS WARRANTY OF ANY KIND.", severity: LoggingLevel.Warning);
            Log("WARNING: This tool does NOT currently integrate updates into the finished media file. Any UUP set with updates (KBXXXXX).MSU/.CAB will not have the update integrated.", severity: LoggingLevel.Warning);
            if (!IsAdministrator())
                Log("WARNING: This tool is NOT currently running as administrator. The resulting image will be less clean/proper compared to Microsoft original.", severity: LoggingLevel.Warning);
            else
            {
                string parentDirectory = GetExecutableDirectory();
                string toolpath = Path.Combine(parentDirectory, "UUPMediaCreator.DismBroker", "UUPMediaCreator.DismBroker.exe");
                if (!File.Exists(toolpath))
                {
                    Log("ERROR: Could not find: " + toolpath, severity: LoggingLevel.Error);
                    return;
                }
            }


            int prevperc = -1;
            Common.ProcessPhase prevphase = Common.ProcessPhase.ReadingMetadata;
            string prevop = "";

            void callback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
            {
                if (phase == prevphase && prevperc == ProgressInPercentage && SubOperation == prevop)
                    return;

                prevphase = phase;
                prevop = SubOperation;
                prevperc = ProgressInPercentage;

                if (phase == Common.ProcessPhase.Error)
                {
                    Log("An error occured!", severity: LoggingLevel.Error);
                    Log(SubOperation, severity: LoggingLevel.Error);
                    return;
                }
                string progress = IsIndeterminate ? "Indeterminate" : ProgressInPercentage.ToString() + "%";
                Log($"[{phase}] [{SubOperation}] Progress: {progress}");
            }

            try
            {
                /*MediaCreationLib.MediaCreator.CreateISOMedia(
                        DestinationISO,
                        UUPPath,
                        Edition,
                        LanguageCode,
                        false,
                        Common.CompressionType.LZMS,
                        callback);*/
                MediaCreationLib.MediaCreator.CreateISOMediaAdvanced(
                        DestinationISO,
                        UUPPath,
                        LanguageCode,
                        false,
                        Common.CompressionType.LZMS,
                        callback);
            }
            catch (Exception ex)
            {
                Log("An error occured!", severity: LoggingLevel.Error);
                Log(ex.ToString(), severity: LoggingLevel.Error);
            }
            Console.WriteLine("The end");
            Console.ReadLine();
        }

        public enum LoggingLevel
        {
            Information,
            Warning,
            Error
        }

        private static readonly object lockObj = new object();

        public static void Log(string message, LoggingLevel severity = LoggingLevel.Information, bool returnline = true)
        {
            lock (lockObj)
            {
                if (message == "")
                {
                    Console.WriteLine();
                    return;
                }

                var msg = "";

                switch (severity)
                {
                    case LoggingLevel.Warning:
                        msg = "  Warning  ";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LoggingLevel.Error:
                        msg = "   Error   ";
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LoggingLevel.Information:
                        msg = "Information";
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                if (returnline)
                    Console.WriteLine(DateTime.Now.ToString("'['HH':'mm':'ss']'") + "[" + msg + "] " + message);
                else
                    Console.Write("\r" + DateTime.Now.ToString("'['HH':'mm':'ss']'") + "[" + msg + "] " + message);

                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
