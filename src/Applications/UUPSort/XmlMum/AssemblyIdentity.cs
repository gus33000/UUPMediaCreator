using System.Xml.Serialization;

namespace UUPSort.XmlMum
{
    [XmlRoot(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class AssemblyIdentity
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "version")]
        public string Version
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "language")]
        public string Language
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "processorArchitecture")]
        public string ProcessorArchitecture
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "publicKeyToken")]
        public string PublicKeyToken
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "buildType")]
        public string BuildType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "versionScope")]
        public string VersionScope
        {
            get; set;
        }
    }
}
