using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using UUPMediaCreator.InterCommunication;

namespace UUPMediaConverterCli
{
    internal class Program
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

        private static void Main(string[] args)
        {
            Log($"UUPMediaConverterCli {Assembly.GetExecutingAssembly().GetName().Version} - Converts an UUP file set to an usable ISO file");
            Log("Copyright (c) Gustave Monce and Contributors");
            Log("https://github.com/gus33000/UUPMediaCreator");
            Log("");
            Log("This program comes with ABSOLUTELY NO WARRANTY.");
            Log("This is free software, and you are welcome to redistribute it under certain conditions.");
            Log("");

            if (args.Length < 3)
            {
                Log("Usage: MediaConverterCli.exe <UUP File set path> <Destination ISO file> <Language Code> [Edition]");
                return;
            }

            string UUPPath = Path.GetFullPath(args[0]);
            string DestinationISO = args[1];
            string LanguageCode = args[2];
            string Edition = "";
            if (args.Length > 3)
                Edition = args[3];

            Log("WARNING: This tool does NOT currently integrate updates into the finished media file. Any UUP set with updates (KBXXXXX).MSU/.CAB will not have the update integrated.", severity: LoggingLevel.Warning);
            if (!IsAdministrator())
            {
                Log("WARNING: This tool is NOT currently running as administrator. The resulting image will be less clean/proper compared to Microsoft original.", severity: LoggingLevel.Warning);

                if (string.IsNullOrEmpty(Edition))
                {
                    Log("WARNING: You are attempting to create an ISO media with potentially all editions available. Due to the tool not running as administrator, this request might not be fullfilled.", severity: LoggingLevel.Warning);
                }
            }
            else
            {
                string parentDirectory = GetParentExecutableDirectory();
                string toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker", "UUPMediaConverterDismBroker.exe");

                if (!File.Exists(toolpath))
                {
                    parentDirectory = GetExecutableDirectory();
                    toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker", "UUPMediaConverterDismBroker.exe");
                }

                if (!File.Exists(toolpath))
                {
                    parentDirectory = GetExecutableDirectory();
                    toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker.exe");
                }

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
                    if (Debugger.IsAttached)
                        Console.ReadLine();
                    return;
                }
                string progress = IsIndeterminate ? "" : $" [Progress: {ProgressInPercentage}%]";
                Log($"[{phase}]{progress} {SubOperation}");
            }

            try
            {
                if (args.Length > 3)
                {
                    MediaCreationLib.MediaCreator.CreateISOMedia(
                        DestinationISO,
                        UUPPath,
                        Edition,
                        LanguageCode,
                        false,
                        Common.CompressionType.LZX,
                        callback);
                }
                else
                {
                    MediaCreationLib.MediaCreator.CreateISOMediaAdvanced(
                           DestinationISO,
                           UUPPath,
                           LanguageCode,
                           false,
                           Common.CompressionType.LZX,
                           callback);
                }
            }
            catch (Exception ex)
            {
                Log("An error occured!", severity: LoggingLevel.Error);
                Log(ex.ToString(), severity: LoggingLevel.Error);
                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
            Console.WriteLine("The end");
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