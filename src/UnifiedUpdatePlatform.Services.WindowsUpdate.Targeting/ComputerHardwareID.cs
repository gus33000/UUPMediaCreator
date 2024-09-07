using System;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting
{
    public class ComputerHardwareID
    {
        public static string GenerateDeviceId(string Manufacturer, string Family, string Product, string Sku)
        {
            string inputString = Manufacturer + "&" + Family + "&" + Product + "&" + Sku;
            Guid guidFromString = ComputerHardwareIDProvider.Class5GuidFromString(inputString);
            string guidString = $"{{{guidFromString}}}";
            return guidString;
        }
    }
}
