using DiscUtils.Registry;
using System;
using System.IO;
using System.Linq;

namespace MediaCreationLib.Installer
{
    internal class RegistryOperations
    {
        private static void ResetWindowsRootInValue(RegistryHive hive, string key, string value)
        {
            var key2 = hive.Root.OpenSubKey(key);
            ResetWindowsRootInValue(key2, value);
        }

        private static void ResetWindowsRootInValue(RegistryKey key2, string value)
        {
            if (key2 != null && key2.GetValueNames().Any(x => x.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
            {
                switch (key2.GetValueType(value))
                {
                    case RegistryValueType.String:
                        {
                            string og = (string)key2.GetValue(value);
                            if (!og.Contains("X:"))
                                break;
                            og = og.Replace(@"X:", @"X:\$windows.~bt");
                            key2.SetValue(value, og, RegistryValueType.String);
                            break;
                        }
                    case RegistryValueType.ExpandString:
                        {
                            string og = (string)key2.GetValue(value);
                            if (!og.Contains("X:"))
                                break;
                            og = og.Replace(@"X:", @"X:\$windows.~bt");
                            key2.SetValue(value, og, RegistryValueType.ExpandString);
                            break;
                        }
                    case RegistryValueType.MultiString:
                        {
                            var ogvals = (string[])key2.GetValue(value);
                            if (!ogvals.Any(x => x.Contains("X:")))
                                break;
                            ogvals = ogvals.ToList().Select(x => x.Replace(@"X:", @"X:\$windows.~bt")).ToArray();
                            key2.SetValue(value, ogvals, RegistryValueType.MultiString);
                            break;
                        }
                }
            }
        }

        private static void CrawlInRegistryKey(RegistryHive hive, string key)
        {
            var key2 = hive.Root.OpenSubKey(key);
            CrawlInRegistryKey(key2);
        }

        private static void CrawlInRegistryKey(RegistryKey key2)
        {
            if (key2 != null)
            {
                foreach (var subval in key2.GetValueNames())
                {
                    ResetWindowsRootInValue(key2, subval);
                }
                foreach (var subkey in key2.SubKeys)
                {
                    CrawlInRegistryKey(subkey);
                }
            }
        }

        internal static bool ModifyBootGlobalRegistry(string systemHivePath)
        {
            try
            {
                using (var hive = new RegistryHive(
                File.Open(
                    systemHivePath,
                    FileMode.Open,
                    FileAccess.ReadWrite
                ), DiscUtils.Streams.Ownership.Dispose))
                {
                    hive.Root.OpenSubKey(@"ControlSet001").CreateSubKey("CI").SetValue("UMCIDisabled", 1, DiscUtils.Registry.RegistryValueType.Dword);
                    hive.Root.OpenSubKey(@"ControlSet001\Control\CI").SetValue("UMCIAuditMode", 1, DiscUtils.Registry.RegistryValueType.Dword);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static bool ModifyBootIndex1Registry(string systemHivePath, string softwareHivePath)
        {
            try
            {
                using (var hive = new RegistryHive(
                    File.Open(
                        systemHivePath,
                        FileMode.Open,
                        FileAccess.ReadWrite
                    ), DiscUtils.Streams.Ownership.Dispose))
                {
                    var key1 = hive.Root.OpenSubKey(@"ControlSet001\Control\NetDiagFx\Microsoft\HostDLLs\NetCoreHelperClass\HelperClasses\Winsock\Repairs");
                    foreach (var subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Description");
                    }
                    key1 = hive.Root.OpenSubKey(@"ControlSet001\Control\NetDiagFx\Microsoft\HostDLLs\NetCoreHelperClass\HelperClasses\Winsock\RootCauses");
                    foreach (var subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Description");
                    }

                    hive.Root.OpenSubKey(@"ControlSet001\Services\cdrom").SetValue("Start", 1);

                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\LSM\Performance", "Library");
                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\RemoteAccess\Performance", "Library");
                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\WinSock2\Parameters", "AutodialDLL");
                }

                using (var hive = new RegistryHive(
                    File.Open(
                        softwareHivePath,
                        FileMode.Open,
                        FileAccess.ReadWrite
                    ), DiscUtils.Streams.Ownership.Dispose))
                {
                    // Crawling things (string, hex(2), hex(7))
                    CrawlInRegistryKey(hive, "Classes");
                    CrawlInRegistryKey(hive, @"Microsoft\Cryptography");

                    ResetWindowsRootInValue(hive, @"Microsoft\Ole\Extensions", "ole32dll");
                    ResetWindowsRootInValue(hive, @"Microsoft\WBEM\CIMOM", "Autorecover MOFs");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows NT\CurrentVersion", "SystemRoot");

                    /*
                     * [HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\MiniDumpAuxiliaryDlls]
                     * "X:\\$windows.~bt\\Windows\\system32\\jscript9.dll"="X:\\$windows.~bt\\Windows\\System32\\jscript9diag.dll"
                     * "X:\\$windows.~bt\\Windows\\system32\\Chakra.dll"="X:\\$windows.~bt\\Windows\\System32\\Chakradiag.dll"
                     * "X:\\Windows\\system32\\jscript9.dll"=-
                     * "X:\\Windows\\system32\\Chakra.dll"=-
                     */
                    var key1 = hive.Root.OpenSubKey(@"Microsoft\Windows NT\CurrentVersion\MiniDumpAuxiliaryDlls");
                    foreach (var subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }
                    foreach (var subval in key1.GetValueNames())
                    {
                        if (subval != subval.Replace(@"X:", @"X:\$windows.~bt"))
                        {
                            key1.SetValue(subval.Replace(@"X:", @"X:\$windows.~bt"), key1.GetValue(subval));
                            key1.DeleteValue(subval);
                        }
                    }

                    ResetWindowsRootInValue(hive, @"Microsoft\Windows NT\CurrentVersion\SeCEdit", "DefaultTemplate");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows NT\CurrentVersion\WinPE", "InstRoot");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows\CurrentVersion", "CommonFilesDir");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows\CurrentVersion", "ProgramFilesDir");

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\Explorer\Shell Folders");
                    foreach (var subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\Management Infrastructure\ErrorResources");
                    foreach (var subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Directory");
                    }

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\ShellCompatibility\InboxApp");
                    foreach (var subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        internal static bool ModifyBootIndex2Registry(string softwareHivePath)
        {
            try
            {
                using (var hive = new RegistryHive(
                    File.Open(
                        softwareHivePath,
                        FileMode.Open,
                        FileAccess.ReadWrite
                    ), DiscUtils.Streams.Ownership.Dispose))
                {
                    var winpekey = hive.Root.OpenSubKey(@"Microsoft\Windows NT\CurrentVersion\WinPE");
                    winpekey.SetValue("CustomBackground", @"%SystemRoot%\system32\setup.bmp", RegistryValueType.ExpandString);
                    var ockey = winpekey.OpenSubKey("OC");
                    ockey.CreateSubKey("Microsoft-WinPE-Setup");
                    ockey.CreateSubKey("Microsoft-WinPE-Setup-Client");
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
