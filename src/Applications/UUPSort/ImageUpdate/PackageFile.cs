using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "PackageFile", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class PackageFile
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
        [XmlAttribute(AttributeName = "ID")]
        public string ID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "CPUType")]
        public string CPUType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Optional")]
        public string Optional
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "FeatureIdentifierPackage")]
        public string FeatureIdentifierPackage
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "SOC")]
        public string SOC
        {
            get; set;
        }
        [XmlElement(ElementName = "FeatureIDs", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public FeatureIDs FeatureIDs
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "LicenseFile")]
        public string LicenseFile
        {
            get; set;
        }
    }
}
