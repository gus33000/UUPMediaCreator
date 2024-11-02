using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "update", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Update
    {
        [XmlElement(ElementName = "component", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Component Component
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get; set;
        }
    }
}
