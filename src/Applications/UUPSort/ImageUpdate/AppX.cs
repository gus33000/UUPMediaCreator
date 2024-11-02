using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "AppX", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class AppX
    {
        [XmlElement(ElementName = "AppXPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public AppXPackages AppXPackages
        {
            get; set;
        }
    }
}
