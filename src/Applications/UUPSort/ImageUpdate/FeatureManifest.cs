using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "FeatureManifest", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class FeatureManifest
    {
        [XmlElement(ElementName = "AppX", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public AppX AppX
        {
            get; set;
        }
        [XmlElement(ElementName = "Drivers", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Drivers Drivers
        {
            get; set;
        }
        [XmlElement(ElementName = "BasePackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public BasePackages BasePackages
        {
            get; set;
        }
        [XmlElement(ElementName = "DeviceLayoutPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public DeviceLayoutPackages DeviceLayoutPackages
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Revision")]
        public string Revision
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "SchemaVersion")]
        public string SchemaVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "ReleaseType")]
        public string ReleaseType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "OwnerType")]
        public string OwnerType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "ID")]
        public string ID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildInfo")]
        public string BuildInfo
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildID")]
        public string BuildID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "OwnerName")]
        public string OwnerName
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "OSVersion")]
        public string OSVersion
        {
            get; set;
        }
    }
}