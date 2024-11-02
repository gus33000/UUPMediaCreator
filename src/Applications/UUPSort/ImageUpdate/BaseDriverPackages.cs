using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "BaseDriverPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class BaseDriverPackages
    {
        [XmlElement(ElementName = "DriverPackageFile", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<DriverPackageFile> DriverPackageFile
        {
            get; set;
        }
    }
}
