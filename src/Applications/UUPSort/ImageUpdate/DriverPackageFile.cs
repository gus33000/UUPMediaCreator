using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "DriverPackageFile", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class DriverPackageFile
    {
        [XmlAttribute(AttributeName = "Path")]
        public string Path
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Name")]
        public string Name
        {
            get; set;
        }
    }
}
