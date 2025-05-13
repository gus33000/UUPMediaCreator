using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "phoneInformation", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class PhoneInformation
    {
        [XmlAttribute(AttributeName = "phoneRelease")]
        public string PhoneRelease
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "phoneOwnerType")]
        public string PhoneOwnerType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "phoneOwner")]
        public string PhoneOwner
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "phoneComponent")]
        public string PhoneComponent
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "phoneSubComponent")]
        public string PhoneSubComponent
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "phoneGroupingKey")]
        public string PhoneGroupingKey
        {
            get; set;
        }
    }
}
