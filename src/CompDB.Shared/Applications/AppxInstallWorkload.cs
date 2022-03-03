using System;

namespace MediaCreationLib.Planning.Applications
{
    public class AppxInstallWorkload
    {
        public string AppXPath { get; set; } = "";
        public string LicensePath { get; set; } = "";
        public string[] DependenciesPath { get; set; } = Array.Empty<string>();
        public string StubPackageOption { get; set; } = "";

        public override string ToString()
        {
            return $"AppX: {AppXPath}\n" +
                   $"License: {LicensePath}\n" +
                   $"Stub Package Option: {StubPackageOption}\n" +
                   $"Dependencies: \n" + 
                   string.Join("\n\t", DependenciesPath);
        }
    }
}
