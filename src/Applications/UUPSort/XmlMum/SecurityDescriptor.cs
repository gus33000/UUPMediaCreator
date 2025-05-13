using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "securityDescriptor", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class SecurityDescriptor
    {
        [XmlAttribute(AttributeName = "name", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public string Name
        {
            get; set;
        }
    }
}
