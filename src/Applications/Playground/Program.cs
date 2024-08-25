using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using System.Net.Mime;
using UnifiedUpdatePlatform.Services.Composition.Database;

namespace Playground
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //Console.WriteLine(new Windows.Internal.Flighting.PlatformCTAC("WU_OS", "10.0.26058.1000").UriQuery);
            var EasClientDeviceInformation = new EasClientDeviceInformation();
            var test = AnalyticsInfo.VersionInfo;
            string Manufacturer = EasClientDeviceInformation.SystemManufacturer;
            string Family = "";
            string Product = EasClientDeviceInformation.SystemProductName;
            string Sku = EasClientDeviceInformation.SystemSku;


            using var dcomSessionOptions = new DComSessionOptions();
            using var cimSession = CimSession.Create("localhost", dcomSessionOptions);
            var result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT SystemFamily FROM Win32_ComputerSystem").SingleOrDefault();

            Family = (string)result.CimInstanceProperties["SystemFamily"].Value;

            Console.WriteLine("Manufacturer: " + Manufacturer);
            Console.WriteLine("Family: " + Family);
            Console.WriteLine("Product: " + Product);
            Console.WriteLine("Sku: " + Sku);

            string ProductGUID = GenerateDeviceId(Manufacturer, Family, Product, Sku);
            Console.WriteLine("Your lottery entry ticket: " + ProductGUID);

            Console.WriteLine("Playing lottery results...");

            CTAC NewestDriverProductCTAC = new(OSSkuId.Professional, "10.0.26200.2483", MachineType.arm64, "Retail", "Retail", "CB", "ge_release", "Production", false, ContentType: "Mainline");
            string token = string.Empty;

            NewestDriverProductCTAC.Products += $"PN={ProductGUID}_arm64&V=0.0.0.0&Source=SMBIOS;";

            IEnumerable<UpdateData> NewestDriverProductUpdateData = await FE3Handler.GetUpdates(null, NewestDriverProductCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

            if (!NewestDriverProductUpdateData.Any())
            {
                Console.WriteLine("No updates found that matched the specified criteria.");
                return;
            }

            string newestDriverVersion = "0.0.0.0";

            for (int i = 0; i < NewestDriverProductUpdateData.Count(); i++)
            {
                UpdateData update = NewestDriverProductUpdateData.ElementAt(i);

                if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                {
                    continue;
                }

                Console.WriteLine($"{i}: Title: {update.Xml.LocalizedProperties.Title}");
                Console.WriteLine($"{i}: Description: {update.Xml.LocalizedProperties.Description}");

                Console.WriteLine("Gathering update metadata...");

                HashSet<CompDB> compDBs = await update.GetCompDBsAsync();

                newestDriverVersion = compDBs.First().UUPProductVersion;
            }

            Console.WriteLine("The winning number today is: " + newestDriverVersion);
        }

        public static string GenerateDeviceId(string Manufacturer, string Family, string Product, string Sku)
        {
            string inputString = Manufacturer + "&" + Family + "&" + Product + "&" + Sku;
            Guid guidFromString = ComputerHardwareIDProvider.Class5GuidFromString(inputString);
            string guidString = $"{{{guidFromString}}}";
            return guidString;
        }
    }
}