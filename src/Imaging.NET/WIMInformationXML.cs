using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Imaging
{
    public class WIMInformationXML
    {
		[XmlRoot(ElementName = "CREATIONTIME")]
		public class CREATIONTIME
		{
			[XmlElement(ElementName = "HIGHPART")]
			public string HIGHPART { get; set; }
			[XmlElement(ElementName = "LOWPART")]
			public string LOWPART { get; set; }
		}

		[XmlRoot(ElementName = "LASTMODIFICATIONTIME")]
		public class LASTMODIFICATIONTIME
		{
			[XmlElement(ElementName = "HIGHPART")]
			public string HIGHPART { get; set; }
			[XmlElement(ElementName = "LOWPART")]
			public string LOWPART { get; set; }
		}

		[XmlRoot(ElementName = "SERVICINGDATA")]
		public class SERVICINGDATA
		{
			[XmlElement(ElementName = "GDRDUREVISION")]
			public string GDRDUREVISION { get; set; }
			[XmlElement(ElementName = "PKEYCONFIGVERSION")]
			public string PKEYCONFIGVERSION { get; set; }
			[XmlElement(ElementName = "IMAGESTATE")]
			public string IMAGESTATE { get; set; }
		}

		[XmlRoot(ElementName = "LANGUAGES")]
		public class LANGUAGES
		{
			[XmlElement(ElementName = "LANGUAGE")]
			public string LANGUAGE { get; set; }
			[XmlElement(ElementName = "DEFAULT")]
			public string DEFAULT { get; set; }
		}

		[XmlRoot(ElementName = "VERSION")]
		public class VERSION
		{
			[XmlElement(ElementName = "MAJOR")]
			public string MAJOR { get; set; }
			[XmlElement(ElementName = "MINOR")]
			public string MINOR { get; set; }
			[XmlElement(ElementName = "BUILD")]
			public string BUILD { get; set; }
			[XmlElement(ElementName = "SPBUILD")]
			public string SPBUILD { get; set; }
			[XmlElement(ElementName = "SPLEVEL")]
			public string SPLEVEL { get; set; }
			[XmlElement(ElementName = "BRANCH")]
			public string BRANCH { get; set; }
		}

		[XmlRoot(ElementName = "WINDOWS")]
		public class WINDOWS
		{
			[XmlElement(ElementName = "ARCH")]
			public string ARCH { get; set; }
			[XmlElement(ElementName = "PRODUCTNAME")]
			public string PRODUCTNAME { get; set; }
			[XmlElement(ElementName = "EDITIONID")]
			public string EDITIONID { get; set; }
			[XmlElement(ElementName = "INSTALLATIONTYPE")]
			public string INSTALLATIONTYPE { get; set; }
			[XmlElement(ElementName = "SERVICINGDATA")]
			public SERVICINGDATA SERVICINGDATA { get; set; }
			[XmlElement(ElementName = "PRODUCTTYPE")]
			public string PRODUCTTYPE { get; set; }
			[XmlElement(ElementName = "PRODUCTSUITE")]
			public string PRODUCTSUITE { get; set; }
			[XmlElement(ElementName = "LANGUAGES")]
			public LANGUAGES LANGUAGES { get; set; }
			[XmlElement(ElementName = "VERSION")]
			public VERSION VERSION { get; set; }
			[XmlElement(ElementName = "SYSTEMROOT")]
			public string SYSTEMROOT { get; set; }
		}

		[XmlRoot(ElementName = "IMAGE")]
		public class IMAGE
		{
			[XmlElement(ElementName = "DIRCOUNT")]
			public string DIRCOUNT { get; set; }
			[XmlElement(ElementName = "FILECOUNT")]
			public string FILECOUNT { get; set; }
			[XmlElement(ElementName = "TOTALBYTES")]
			public string TOTALBYTES { get; set; }
			[XmlElement(ElementName = "HARDLINKBYTES")]
			public string HARDLINKBYTES { get; set; }
			[XmlElement(ElementName = "CREATIONTIME")]
			public CREATIONTIME CREATIONTIME { get; set; }
			[XmlElement(ElementName = "LASTMODIFICATIONTIME")]
			public LASTMODIFICATIONTIME LASTMODIFICATIONTIME { get; set; }
			[XmlElement(ElementName = "WIMBOOT")]
			public string WIMBOOT { get; set; }
			[XmlElement(ElementName = "WINDOWS")]
			public WINDOWS WINDOWS { get; set; }
			[XmlElement(ElementName = "FLAGS")]
			public string FLAGS { get; set; }
			[XmlElement(ElementName = "DISPLAYNAME")]
			public string DISPLAYNAME { get; set; }
			[XmlElement(ElementName = "DISPLAYDESCRIPTION")]
			public string DISPLAYDESCRIPTION { get; set; }
			[XmlElement(ElementName = "NAME")]
			public string NAME { get; set; }
			[XmlElement(ElementName = "DESCRIPTION")]
			public string DESCRIPTION { get; set; }
			[XmlAttribute(AttributeName = "INDEX")]
			public string INDEX { get; set; }
		}

		[XmlRoot(ElementName = "WIM")]
		public class WIM
		{
			[XmlElement(ElementName = "TOTALBYTES")]
			public string TOTALBYTES { get; set; }
			[XmlElement(ElementName = "IMAGE")]
			public List<IMAGE> IMAGE { get; set; }
		}

		public static string SerializeWIM(WIM wim)
		{
			if (wim == null) return string.Empty;

			var xmlSerializer = new XmlSerializer(typeof(WIM));

			using (var stringWriter = new StringWriter())
			{
				using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
				{ 
					Indent = false, 
					IndentChars = "", 
					NewLineChars = "", 
					NewLineHandling = NewLineHandling.None,
					OmitXmlDeclaration = true,
					NewLineOnAttributes = false
				}))
				{
					xmlSerializer.Serialize(xmlWriter, wim);
					return stringWriter.ToString().Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "")
						.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
				}
			}
		}

		public static WIM DeserializeWIM(string wim)
		{
			if (wim == null) return null;

			var xmlSerializer = new XmlSerializer(typeof(WIM));

			using (var stringReader = new StringReader(wim))
			{
				return (WIM)xmlSerializer.Deserialize(stringReader);
			}
		}

		public static string SerializeIMAGE(IMAGE wim)
		{
			if (wim == null) return string.Empty;

			var xmlSerializer = new XmlSerializer(typeof(IMAGE));

			using (var stringWriter = new StringWriter())
			{
				using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
				{
					Indent = false,
					IndentChars = "",
					NewLineChars = "",
					NewLineHandling = NewLineHandling.None,
					OmitXmlDeclaration = true,
					NewLineOnAttributes = false
				}))
				{
					xmlSerializer.Serialize(xmlWriter, wim);
					return stringWriter.ToString().Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "")
						.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
				}
			}
		}

		public static IMAGE DeserializeIMAGE(string wim)
		{
			if (wim == null) return null;

			var xmlSerializer = new XmlSerializer(typeof(IMAGE));

			using (var stringReader = new StringReader(wim))
			{
				return (IMAGE)xmlSerializer.Deserialize(stringReader);
			}
		}
	}
}
