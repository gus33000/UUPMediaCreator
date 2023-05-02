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
using DiscUtils.Registry;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnifiedUpdatePlatform.Media.Creator.NET.Utils;

namespace UnifiedUpdatePlatform.Media.Creator.NET.Installer
{
    internal static class PreinstallationEnvironmentRegistryService
    {
        private static void ResetWindowsRootInValue(RegistryHive hive, string key, string value)
        {
            RegistryKey key2 = hive.Root.OpenSubKey(key);
            ResetWindowsRootInValue(key2, value);
        }

        private static void ResetWindowsRootInValue(RegistryKey key2, string value)
        {
            if (key2?.GetValueNames().Any(x => x.Equals(value, StringComparison.InvariantCultureIgnoreCase)) == true)
            {
                switch (key2.GetValueType(value))
                {
                    case RegistryValueType.String:
                        {
                            string og = (string)key2.GetValue(value);
                            if (!og.Contains("X:"))
                            {
                                break;
                            }

                            og = og.Replace("X:", @"X:\$windows.~bt");
                            key2.SetValue(value, og, RegistryValueType.String);
                            break;
                        }
                    case RegistryValueType.ExpandString:
                        {
                            string og = (string)key2.GetValue(value);
                            if (!og.Contains("X:"))
                            {
                                break;
                            }

                            og = og.Replace("X:", @"X:\$windows.~bt");
                            key2.SetValue(value, og, RegistryValueType.ExpandString);
                            break;
                        }
                    case RegistryValueType.MultiString:
                        {
                            string[] ogvals = (string[])key2.GetValue(value);
                            if (!ogvals.Any(x => x.Contains("X:")))
                            {
                                break;
                            }

                            ogvals = ogvals.ToList().Select(x => x.Replace("X:", @"X:\$windows.~bt")).ToArray();
                            key2.SetValue(value, ogvals, RegistryValueType.MultiString);
                            break;
                        }
                }
            }
        }

        private static void CrawlInRegistryKey(RegistryHive hive, string key)
        {
            RegistryKey key2 = hive.Root.OpenSubKey(key);
            CrawlInRegistryKey(key2);
        }

        private static void CrawlInRegistryKey(RegistryKey key2)
        {
            if (key2 != null)
            {
                foreach (string subval in key2.GetValueNames())
                {
                    ResetWindowsRootInValue(key2, subval);
                }
                foreach (RegistryKey subkey in key2.SubKeys)
                {
                    CrawlInRegistryKey(subkey);
                }
            }
        }

        internal static bool ModifyBootGlobalRegistry(string systemHivePath)
        {
            try
            {
                using RegistryHive hive = new(
                File.Open(
                    systemHivePath,
                    FileMode.Open,
                    FileAccess.ReadWrite
                ), DiscUtils.Streams.Ownership.Dispose);
                hive.Root.OpenSubKey("ControlSet001").CreateSubKey("CI").SetValue("UMCIDisabled", 1, RegistryValueType.Dword);
                hive.Root.OpenSubKey(@"ControlSet001\Control\CI").SetValue("UMCIAuditMode", 1, RegistryValueType.Dword);
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
                using (RegistryHive hive = new(
                    File.Open(
                        systemHivePath,
                        FileMode.Open,
                        FileAccess.ReadWrite
                    ), DiscUtils.Streams.Ownership.Dispose))
                {
                    RegistryKey key1 = hive.Root.OpenSubKey(@"ControlSet001\Control\NetDiagFx\Microsoft\HostDLLs\NetCoreHelperClass\HelperClasses\Winsock\Repairs");
                    foreach (RegistryKey subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Description");
                    }
                    key1 = hive.Root.OpenSubKey(@"ControlSet001\Control\NetDiagFx\Microsoft\HostDLLs\NetCoreHelperClass\HelperClasses\Winsock\RootCauses");
                    foreach (RegistryKey subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Description");
                    }

                    hive.Root.OpenSubKey(@"ControlSet001\Services\cdrom").SetValue("Start", 1);

                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\LSM\Performance", "Library");
                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\RemoteAccess\Performance", "Library");
                    ResetWindowsRootInValue(hive, @"ControlSet001\Services\WinSock2\Parameters", "AutodialDLL");
                }

                using (RegistryHive hive = new(
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
                    RegistryKey key1 = hive.Root.OpenSubKey(@"Microsoft\Windows NT\CurrentVersion\MiniDumpAuxiliaryDlls");
                    foreach (string subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }
                    foreach (string subval in key1.GetValueNames())
                    {
                        if (subval != subval.Replace("X:", @"X:\$windows.~bt"))
                        {
                            key1.SetValue(subval.Replace("X:", @"X:\$windows.~bt"), key1.GetValue(subval));
                            key1.DeleteValue(subval);
                        }
                    }

                    ResetWindowsRootInValue(hive, @"Microsoft\Windows NT\CurrentVersion\SeCEdit", "DefaultTemplate");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows NT\CurrentVersion\WinPE", "InstRoot");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows\CurrentVersion", "CommonFilesDir");
                    ResetWindowsRootInValue(hive, @"Microsoft\Windows\CurrentVersion", "ProgramFilesDir");

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\Explorer\Shell Folders");
                    foreach (string subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\Management Infrastructure\ErrorResources");
                    foreach (RegistryKey subkey in key1.SubKeys)
                    {
                        ResetWindowsRootInValue(subkey, "Directory");
                    }

                    key1 = hive.Root.OpenSubKey(@"Microsoft\Windows\CurrentVersion\ShellCompatibility\InboxApp");
                    foreach (string subval in key1.GetValueNames())
                    {
                        ResetWindowsRootInValue(key1, subval);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static bool ModifyBootIndex2Registry(string softwareHivePath)
        {
            try
            {
                using RegistryHive hive = new(
                    File.Open(
                        softwareHivePath,
                        FileMode.Open,
                        FileAccess.ReadWrite
                    ), DiscUtils.Streams.Ownership.Dispose);
                RegistryKey winpekey = hive.Root.OpenSubKey(@"Microsoft\Windows NT\CurrentVersion\WinPE");
                winpekey.SetValue("CustomBackground", @"%SystemRoot%\system32\setup.bmp", RegistryValueType.ExpandString);
                if (PlatformUtilities.OperatingSystem == OSPlatform.Windows)
                {
                    RegistryKey ockey = winpekey.OpenSubKey("OC");
                    _ = ockey.CreateSubKey("Microsoft-WinPE-Setup");
                    _ = ockey.CreateSubKey("Microsoft-WinPE-Setup-Client");
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