using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "AppXPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class AppXPackages
    {
        [XmlElement(ElementName = "PackageFile", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<PackageFile> PackageFile
        {
            get; set;
        }
    }
}
