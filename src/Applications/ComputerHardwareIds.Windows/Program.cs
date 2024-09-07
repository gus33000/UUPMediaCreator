using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure;
using System.Diagnostics;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;

namespace ComputerHardwareIds
{
    internal class Program
    {
        public static void Main(string[] _)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule MainModule = currentProcess.MainModule!;

            using DComSessionOptions dcomSessionOptions = new DComSessionOptions();
            using CimSession cimSession = CimSession.Create("localhost", dcomSessionOptions);

            CimInstance result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_BIOS").Single();

            string BIOSVendor = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string BIOSVersionString = (string)result.CimInstanceProperties["SMBIOSBIOSVersion"].Value;
            byte SystemBIOSMajorRelease = (byte)result.CimInstanceProperties["SystemBiosMajorVersion"].Value;
            byte SystemBIOSMinorRelease = (byte)result.CimInstanceProperties["SystemBiosMinorVersion"].Value;

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem").Single();

            string SystemManufacturer = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string SystemFamily = (string)result.CimInstanceProperties["SystemFamily"].Value;
            string SystemProductName = (string)result.CimInstanceProperties["Model"].Value;
            string SKUNumber = (string)result.CimInstanceProperties["SystemSKUNumber"].Value;

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_SystemEnclosure").Single();

            ushort SystemEnclosureorChassisType = ((ushort[])result.CimInstanceProperties["ChassisTypes"].Value)[0];

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_BaseBoard").Single();

            string BaseboardManufacturer = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string BaseboardProductName = (string)result.CimInstanceProperties["Product"].Value;

            Console.WriteLine("Using the BIOS to gather information");
            Console.WriteLine();
            Console.WriteLine("Tool Information");
            Console.WriteLine("----------------");
            Console.WriteLine($"{Path.GetFileName(MainModule.FileName)} version: {MainModule.FileVersionInfo.FileVersion}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Computer Information");
            Console.WriteLine("--------------------");
            Console.WriteLine($"BIOS Vendor: {BIOSVendor}");
            Console.WriteLine($"BIOS Version String: {BIOSVersionString}");
            Console.WriteLine($"System BIOS Major Release: {SystemBIOSMajorRelease}");
            Console.WriteLine($"System BIOS Minor Release: {SystemBIOSMinorRelease}");
            Console.WriteLine();
            Console.WriteLine($"System Manufacturer: {SystemManufacturer}");
            Console.WriteLine($"System Family: {SystemFamily}");
            Console.WriteLine($"System Product Name: {SystemProductName}");
            Console.WriteLine($"SKU Number: {SKUNumber}");
            Console.WriteLine();
            Console.WriteLine($"System Enclosure or Chassis Type: {SystemEnclosureorChassisType:X2}h");
            Console.WriteLine();
            Console.WriteLine($"Baseboard Manufacturer: {BaseboardManufacturer}");
            Console.WriteLine($"Baseboard Product Name: {BaseboardProductName}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Hardware IDs");
            Console.WriteLine("------------");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId1(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease.ToString("X").ToLower(), SystemBIOSMinorRelease.ToString("X").ToLower())}    <- Manufacturer + Family + ProductName + SKUNumber + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId2(SystemManufacturer, SystemFamily, SystemProductName, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease.ToString("X").ToLower(), SystemBIOSMinorRelease.ToString("X").ToLower())}    <- Manufacturer + Family + ProductName + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId3(SystemManufacturer, SystemProductName, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease.ToString("X").ToLower(), SystemBIOSMinorRelease.ToString("X").ToLower())}    <- Manufacturer + ProductName + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId4(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber, BaseboardManufacturer, BaseboardProductName)}    <- Manufacturer + Family + ProductName + SKUNumber + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId5(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber)}    <- Manufacturer + Family + ProductName + SKUNumber");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId6(SystemManufacturer, SystemFamily, SystemProductName)}    <- Manufacturer + Family + ProductName");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId7(SystemManufacturer, SKUNumber, BaseboardManufacturer, BaseboardProductName)}    <- Manufacturer + SKUNumber + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId8(SystemManufacturer, SKUNumber)}    <- Manufacturer + SKUNumber");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId9(SystemManufacturer, SystemProductName, BaseboardManufacturer, BaseboardProductName)}    <- Manufacturer + ProductName + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId10(SystemManufacturer, SystemProductName)}    <- Manufacturer + ProductName");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId11(SystemManufacturer, SystemFamily, BaseboardManufacturer, BaseboardProductName)}    <- Manufacturer + Family + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId12(SystemManufacturer, SystemFamily)}    <- Manufacturer + Family");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId13(SystemManufacturer, SystemEnclosureorChassisType.ToString())}    <- Manufacturer + Enclosure Type");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId14(SystemManufacturer, BaseboardManufacturer, BaseboardProductName)}    <- Manufacturer + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{ComputerHardwareID.GenerateHardwareId15(SystemManufacturer)}    <- Manufacturer");
        }
    }
}