using System.Xml.Serialization;

namespace UUPSort.ImageUpdate
{
    [XmlRoot(ElementName = "FeatureIDs", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class FeatureIDs
    {
        [XmlElement(ElementName = "FeatureID", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<string> FeatureID
        {
            get; set;
        }
    }
}
