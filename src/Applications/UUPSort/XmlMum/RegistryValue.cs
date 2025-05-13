using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "registryValue", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class RegistryValue
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "value")]
        public string Value
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "valueType")]
        public string ValueType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "mutable")]
        public string Mutable
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "operationHint")]
        public string OperationHint
        {
            get; set;
        } //e.g: replace
    }
}
