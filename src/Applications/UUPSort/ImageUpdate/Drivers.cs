using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "Drivers", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Drivers
    {
        [XmlElement(ElementName = "BaseDriverPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public BaseDriverPackages BaseDriverPackages
        {
            get; set;
        }
    }
}
