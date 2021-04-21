using System.IO;
using System.Xml;
using System.Xml.Serialization;
using WindowsUpdateLib;

namespace UUPDownload.DownloadRequest
{
    public class UpdateScan
    {
        public UpdateData UpdateData { get; set; }
        public BuildTargets.EditionPlanningWithLanguage[] Targets { get; set; }
        public MachineType Architecture { get; set; }
        public string BuildString { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(UpdateScan));

            XmlSerializerNamespaces ns = new();
            ns.Add("s", "http://www.w3.org/2003/05/soap-envelope");
            ns.Add("a", "http://www.w3.org/2005/08/addressing");

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
            xmlSerializer.Serialize(xmlWriter, this, ns);
            return stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
        }
    }
}
