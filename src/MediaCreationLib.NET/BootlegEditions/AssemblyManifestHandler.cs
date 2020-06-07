using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaCreationLib.BootlegEditions
{
    public class AssemblyManifestHandler
	{
		public static void RemoveNonLTSBPackages(string manifestPath)
        {
			string content = File.ReadAllText(manifestPath);
			var assembly = Deserialize(content);
			assembly.Package.Update.RemoveAll(x => x.Name.Contains("Not-Supported-On-LTSB"));
			File.WriteAllText(manifestPath, Serialize(assembly));
		}

		internal static void RemoveWOW64Package(string manifestPath, string v)
		{
			string content = File.ReadAllText(manifestPath);
			var assembly = Deserialize(content);
			assembly.Package.Update.RemoveAll(x => x.Name.ToLower().Contains(v.ToLower()));
			File.WriteAllText(manifestPath, Serialize(assembly));
		}

		public static Assembly Deserialize(string Xml)
		{
			if (Xml == null) return null;

			var xmlSerializer = new XmlSerializer(typeof(Assembly));

			using (var stringReader = new StringReader(Xml))
			{
				return (Assembly)xmlSerializer.Deserialize(stringReader);
			}
		}

		public static string Serialize(Assembly assembly)
		{
			if (assembly == null) return string.Empty;

			var xmlSerializer = new XmlSerializer(typeof(Assembly));

			using (var stringWriter = new StringWriter())
			{
				using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = false,
					NewLineOnAttributes = false,
					Encoding = Encoding.UTF8
				}))
				{
					xmlSerializer.Serialize(xmlWriter, assembly);
					return stringWriter.ToString()
						.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xmlns=\"urn:schemas-microsoft-com:asm.v3\"")
						.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "xmlns=\"urn:schemas-microsoft-com:asm.v3\"")
						.Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"")
						.Replace("encoding=\"utf-8\"", "encoding=\"utf-8\" standalone=\"yes\"")
						.Replace(" xmlns=\"urn:schemas-microsoft-com:asm.v3\">", ">");
				}
			}
		}

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
}
