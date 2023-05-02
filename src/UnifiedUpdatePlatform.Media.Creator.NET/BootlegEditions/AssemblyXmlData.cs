using System.Collections.Generic;
using System.Xml.Serialization;

namespace UnifiedUpdatePlatform.Media.Creator.BootlegEditions
{
    [XmlRoot(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class AssemblyIdentity
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "processorArchitecture")]
        public string ProcessorArchitecture { get; set; }

        [XmlAttribute(AttributeName = "language")]
        public string Language { get; set; }

        [XmlAttribute(AttributeName = "buildType")]
        public string BuildType { get; set; }

        [XmlAttribute(AttributeName = "publicKeyToken")]
        public string PublicKeyToken { get; set; }
    }

    [XmlRoot(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Package2
    {
        [XmlElement(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public AssemblyIdentity AssemblyIdentity { get; set; }

        [XmlAttribute(AttributeName = "contained")]
        public string Contained { get; set; }

        [XmlAttribute(AttributeName = "integrate")]
        public string Integrate { get; set; }
    }

    [XmlRoot(ElementName = "update", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Update
    {
        [XmlElement(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Package2 Package { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Package
    {
        [XmlElement(ElementName = "update", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public List<Update> Update { get; set; }

        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }

        [XmlAttribute(AttributeName = "releaseType")]
        public string ReleaseType { get; set; }
    }

    [XmlRoot(ElementName = "assembly", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Assembly
    {
        [XmlElement(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public AssemblyIdentity AssemblyIdentity { get; set; }

        [XmlElement(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Package Package { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        [XmlAttribute(AttributeName = "manifestVersion")]
        public string ManifestVersion { get; set; }

        [XmlAttribute(AttributeName = "copyright")]
        public string Copyright { get; set; }
    }
}
