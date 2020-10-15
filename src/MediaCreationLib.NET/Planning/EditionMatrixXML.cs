using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MediaCreationLib.Planning
{
    public class EditionMatrixXML
    {
        [XmlRoot(ElementName = "Target")]
        public class Target
        {
            [XmlAttribute(AttributeName = "ID")]
            public string ID { get; set; }
        }

        [XmlRoot(ElementName = "Edition")]
        public class Edition
        {
            [XmlElement(ElementName = "Target")]
            public List<Target> Target { get; set; }

            [XmlAttribute(AttributeName = "ID")]
            public string ID { get; set; }

            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "processorArchitecture")]
            public string ProcessorArchitecture { get; set; }

            [XmlAttribute(AttributeName = "buildType")]
            public string BuildType { get; set; }

            [XmlAttribute(AttributeName = "publicKeyToken")]
            public string PublicKeyToken { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public string Version { get; set; }
        }

        [XmlRoot(ElementName = "TmiMatrix")]
        public class TmiMatrix
        {
            [XmlElement(ElementName = "Edition")]
            public List<Edition> Edition { get; set; }

            [XmlAttribute(AttributeName = "e", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string E { get; set; }
        }

        public static TmiMatrix Deserialize(string editionMatrixXml)
        {
            if (editionMatrixXml == null) return null;

            var xmlSerializer = new XmlSerializer(typeof(TmiMatrix));

            using (var stringReader = new StringReader(editionMatrixXml))
            {
                return (TmiMatrix)xmlSerializer.Deserialize(stringReader);
            }
        }
    }
}