using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "file", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class File
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "size")]
        public string Size
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "staged")]
        public string Staged
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "compressed")]
        public string Compressed
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "sourcePackage")]
        public string SourcePackage
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "embeddedSign")]
        public string EmbeddedSign
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "cabpath")]
        public string Cabpath
        {
            get; set;
        }
    }
}
