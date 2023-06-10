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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CompDB
{
    public static class CompDBXmlClass
    {
        [XmlRoot(ElementName = "Tag", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Tag
        {
            [XmlAttribute(AttributeName = "Name", Namespace = "")]
            public string Name
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Package
        {
            [XmlAttribute(AttributeName = "ID", Namespace = "")]
            public string ID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "PackageType", Namespace = "")]
            public string PackageType
            {
                get; set;
            }
            [XmlElement(ElementName = "SatelliteInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public SatelliteInfo SatelliteInfo
            {
                get; set;
            }
            [XmlElement(ElementName = "Payload", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Payload Payload
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "InstalledSize", Namespace = "")]
            public string InstalledSize
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Version", Namespace = "")]
            public string Version
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "UpdateType", Namespace = "")]
            public string UpdateType
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Packages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Packages
        {
            [XmlElement(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<Package> Package
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Feature", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Feature
        {
            [XmlElement(ElementName = "Packages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Packages Packages
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FeatureID", Namespace = "")]
            public string FeatureID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FMID", Namespace = "")]
            public string FMID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Group", Namespace = "")]
            public string Group
            {
                get; set;
            }
            [XmlElement(ElementName = "Dependencies", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Dependencies Dependencies
            {
                get; set;
            }
            [XmlElement(ElementName = "InitialIntents", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public InitialIntents InitialIntents
            {
                get; set;
            }
            [XmlElement(ElementName = "CustomInformation", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public CustomInformation CustomInformation
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Family", Namespace = "")]
            public string Family
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Features", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Features
        {
            [XmlElement(ElementName = "Feature", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Feature[] Feature
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Require", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Require
        {
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "RequireInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class RequireInfo
        {
            [XmlElement(ElementName = "Require", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<Require> Require
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "SatelliteInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class SatelliteInfo
        {
            [XmlElement(ElementName = "RequireInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public RequireInfo RequireInfo
            {
                get; set;
            }
            [XmlElement(ElementName = "ApplyToInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public ApplyToInfo ApplyToInfo
            {
                get; set;
            }
            [XmlElement(ElementName = "DeclareInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public DeclareInfo DeclareInfo
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "PayloadItem", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class PayloadItem
        {
            [XmlAttribute(AttributeName = "PayloadHash", Namespace = "")]
            public string SourceHash
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "PayloadSize", Namespace = "")]
            public string PayloadSize
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Path", Namespace = "")]
            public string SourceName
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "PayloadType", Namespace = "")]
            public string PayloadType
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "AltSourceName", Namespace = "")]
            public string AltSourceName
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "AltSourceHash", Namespace = "")]
            public string AltSourceHash
            {
                get; set;
            }

            public string Path => AltSourceName ?? SourceName;
            public string PayloadHash => AltSourceHash ?? SourceHash;
        }

        [XmlRoot(ElementName = "Payload", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Payload
        {
            [XmlElement(ElementName = "PayloadItem", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<PayloadItem> PayloadItem
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "ApplyTo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class ApplyTo
        {
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "CompDB", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class CompDB
        {
            [XmlElement(ElementName = "Tags", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Tags Tags
            {
                get; set;
            }
            [XmlElement(ElementName = "Features", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Features Features
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string xsi
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string xsd
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "CreatedDate", Namespace = "")]
            public string CreatedDate
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Revision", Namespace = "")]
            public string Revision
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "SchemaVersion", Namespace = "")]
            public string SchemaVersion
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Product", Namespace = "")]
            public string Product
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "BuildID", Namespace = "")]
            public string BuildID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "BuildInfo", Namespace = "")]
            public string BuildInfo
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "OSVersion", Namespace = "")]
            public string OSVersion
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "BuildArch", Namespace = "")]
            public string BuildArch
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "ReleaseType", Namespace = "")]
            public string ReleaseType
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Name", Namespace = "")]
            public string Name
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "xmlns", Namespace = "")]
            public string xmlns
            {
                get; set;
            }
            [XmlElement(ElementName = "Packages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Packages Packages
            {
                get; set;
            }
            [XmlElement(ElementName = "AppX", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Appx AppX
            {
                get; set;
            }
            [XmlElement(ElementName = "MSConditionalFeatures", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public MSConditionalFeatures MSConditionalFeatures
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "TargetBuildID", Namespace = "")]
            public string TargetBuildID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "TargetBuildInfo", Namespace = "")]
            public string TargetBuildInfo
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "TargetOSVersion", Namespace = "")]
            public string TargetOSVersion
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Tags", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Tags
        {
            [XmlElement(ElementName = "Tag", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<Tag> Tag
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "ApplyToInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class ApplyToInfo
        {
            [XmlElement(ElementName = "ApplyTo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<ApplyTo> ApplyTo
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Declare", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Declare
        {
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "DeclareInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class DeclareInfo
        {
            [XmlElement(ElementName = "Declare", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Declare Declare
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Dependencies", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Dependencies
        {
            [XmlElement(ElementName = "Feature", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<Feature> Feature
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "InitialIntent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class InitialIntent
        {
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "CustomInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class CustomInfo
        {
            [XmlAttribute(AttributeName = "Key", Namespace = "")]
            public string Key
            {
                get; set;
            }

            [XmlText]
            public string Value
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "CustomInformation", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class CustomInformation
        {
            [XmlElement(ElementName = "CustomInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public CustomInfo[] CustomInfo
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "InitialIntents", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class InitialIntents
        {
            [XmlElement(ElementName = "InitialIntent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<InitialIntent> InitialIntent
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Condition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Condition
        {
            [XmlAttribute(AttributeName = "Type", Namespace = "")]
            public string Type
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Name", Namespace = "")]
            public string Name
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FMID", Namespace = "")]
            public string FMID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Operator", Namespace = "")]
            public string Operator
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "RegistryKey", Namespace = "")]
            public string RegistryKey
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Value", Namespace = "")]
            public string Value
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "RegistryKeyType", Namespace = "")]
            public string RegistryKeyType
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Status", Namespace = "")]
            public string Status
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FeatureStatus", Namespace = "")]
            public string FeatureStatus
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "ConditionSet", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class ConditionSet
        {
            [XmlElement(ElementName = "Conditions", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Conditions Conditions
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "Operator", Namespace = "")]
            public string Operator
            {
                get; set;
            }
            [XmlElement(ElementName = "ConditionSets", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public ConditionSets ConditionSets
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Conditions", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Conditions
        {
            [XmlElement(ElementName = "Condition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<Condition> Condition
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "ConditionSets", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class ConditionSets
        {
            [XmlElement(ElementName = "ConditionSet", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public ConditionSet ConditionSet
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "ConditionalFeature", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class ConditionalFeature
        {
            [XmlElement(ElementName = "Condition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Condition Condition
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "InstallAction", Namespace = "")]
            public string InstallAction
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FeatureID", Namespace = "")]
            public string FeatureID
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FMID", Namespace = "")]
            public string FMID
            {
                get; set;
            }
            [XmlElement(ElementName = "ConditionSet", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public ConditionSet ConditionSet
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "UpdateAction", Namespace = "")]
            public string UpdateAction
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "MSConditionalFeatures", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class MSConditionalFeatures
        {
            [XmlElement(ElementName = "ConditionalFeature", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<ConditionalFeature> ConditionalFeature
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "AppX", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Appx
        {
            [XmlElement(ElementName = "AppXPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public AppxPackages AppXPackages
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "AppXPackages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class AppxPackages
        {
            [XmlElement(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<AppxPackage> Package
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class AppxPackage
        {
            [XmlAttribute(AttributeName = "AppXPackageType")]
            public string AppXPackageType
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FullName")]
            public string FullName
            {
                get; set;
            }
            [XmlAttribute(AttributeName = "FamilyName")]
            public string FamilyName
            {
                get; set;
            }
            [XmlElement(ElementName = "Payload", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Payload Payload
            {
                get; set;
            }
            [XmlElement(ElementName = "LicenseData", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string LicenseData
            {
                get; set;
            }
            // ...
        }

        public static CompDB DeserializeCompDB(Stream stringReader)
        {
            XmlSerializer xmlSerializer = new(typeof(CompDB));
            return (CompDB)xmlSerializer.Deserialize(stringReader);
        }
    }
}