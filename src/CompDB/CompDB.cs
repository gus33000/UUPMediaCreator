/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System.Xml.Serialization;

namespace UnifiedUpdatePlatform.Services.Composition.Database
{

    [XmlRoot(ElementName = "CompDB", Namespace = Constants.ImageUpdateNamespace)]
    public class CompDB
    {
        [XmlElement(ElementName = "Tags", Namespace = Constants.ImageUpdateNamespace)]
        public Tags Tags
        {
            get; set;
        }
        [XmlElement(ElementName = "Features", Namespace = Constants.ImageUpdateNamespace)]
        public Features Features
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "CreatedDate", Namespace = Constants.ImageUpdateNamespace)]
        public string CreatedDate
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Revision", Namespace = Constants.ImageUpdateNamespace)]
        public string Revision
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "SchemaVersion", Namespace = Constants.ImageUpdateNamespace)]
        public string SchemaVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Product", Namespace = Constants.ImageUpdateNamespace)]
        public string Product
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildID", Namespace = Constants.ImageUpdateNamespace)]
        public string BuildID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildInfo", Namespace = Constants.ImageUpdateNamespace)]
        public string BuildInfo
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "OSVersion", Namespace = Constants.ImageUpdateNamespace)]
        public string OSVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildArch", Namespace = Constants.ImageUpdateNamespace)]
        public string BuildArch
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "ReleaseType", Namespace = Constants.ImageUpdateNamespace)]
        public string ReleaseType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Type", Namespace = Constants.ImageUpdateNamespace)]
        public string Type
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Name", Namespace = Constants.ImageUpdateNamespace)]
        public string Name
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xmlns", Namespace = Constants.ImageUpdateNamespace)]
        public string Xmlns
        {
            get; set;
        }
        [XmlElement(ElementName = "Packages", Namespace = Constants.ImageUpdateNamespace)]
        public Packages Packages
        {
            get; set;
        }
        [XmlElement(ElementName = "AppX", Namespace = Constants.ImageUpdateNamespace)]
        public Appx AppX
        {
            get; set;
        }
        [XmlElement(ElementName = "MSConditionalFeatures", Namespace = Constants.ImageUpdateNamespace)]
        public MSConditionalFeatures MSConditionalFeatures
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetBuildID", Namespace = Constants.ImageUpdateNamespace)]
        public string TargetBuildID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetBuildInfo", Namespace = Constants.ImageUpdateNamespace)]
        public string TargetBuildInfo
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetOSVersion", Namespace = Constants.ImageUpdateNamespace)]
        public string TargetOSVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "UUPProduct", Namespace = Constants.ImageUpdateNamespace)]
        public string UUPProduct
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "UUPProductVersion", Namespace = Constants.ImageUpdateNamespace)]
        public string UUPProductVersion
        {
            get; set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}