using System;

namespace MediaCreationLib.Dism
{
    public class AppxInstallWorkload
    {
        public string AppXPath { get; set; } = "";
        public string LicensePath { get; set; } = "";
        public string[] DependenciesPath { get; set; } = Array.Empty<string>();
        public string StubPackageOption { get; set; } = "";

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public static AppxInstallWorkload FromString(string s)
        {
            return System.Text.Json.JsonSerializer.Deserialize<AppxInstallWorkload>(s);
        }
    }
}
