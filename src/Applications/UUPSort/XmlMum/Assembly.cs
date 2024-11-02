using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "assembly", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Assembly
    {
        [XmlElement(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public AssemblyIdentity AssemblyIdentity
        {
            get; set;
        }

        [XmlArray(ElementName = "registryKeys", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        [XmlArrayItem(ElementName = "registryKey")]
        public List<RegistryKey> RegistryKeys
        {
            get; set;
        }

        [XmlElement(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Package Package
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "manifestVersion")]
        public string ManifestVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "displayName")]
        public string DisplayName
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "company")]
        public string Company
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "copyright")]
        public string Copyright
        {
            get; set;
        }

        //TODO: trustInfo
    }
}
