using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MediaCreationLib.Planning
{
    public class EditionMappingXML
    {
		[XmlRoot(ElementName = "Edition")]
		public class Edition
		{
			[XmlElement(ElementName = "Name")]
			public string Name { get; set; }
			[XmlElement(ElementName = "ParentEdition")]
			public string ParentEdition { get; set; }
			[XmlAttribute(AttributeName = "virtual")]
			public string Virtual { get; set; }
		}

		[XmlRoot(ElementName = "WindowsEditions")]
		public class WindowsEditions
		{
			[XmlElement(ElementName = "Edition")]
			public List<Edition> Edition { get; set; }
			[XmlAttribute(AttributeName = "e", Namespace = "http://www.w3.org/2000/xmlns/")]
			public string E { get; set; }
		}

		public static WindowsEditions Deserialize(string editionMappingXml)
		{
			if (editionMappingXml == null) return null;

			var xmlSerializer = new XmlSerializer(typeof(WindowsEditions));

			using (var stringReader = new StringReader(editionMappingXml))
			{
				return (WindowsEditions)xmlSerializer.Deserialize(stringReader);
			}
		}
	}
}
