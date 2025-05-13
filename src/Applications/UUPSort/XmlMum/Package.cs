using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Package
    {
        [XmlElement(ElementName = "customInformation", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public CustomInformation CustomInformation
        {
            get; set;
        }
        [XmlElement(ElementName = "update", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Update Update
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "releaseType")]
        public string ReleaseType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "restart")]
        public string Restart
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "targetPartition")]
        public string TargetPartition
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "binaryPartition")]
        public string BinaryPartition
        {
            get; set;
        }
    }
}
