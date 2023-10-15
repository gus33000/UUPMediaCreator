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

namespace UnifiedUpdatePlatform.Media.Creator.Planning
{
    public static class EditionMappingXML
    {
        [XmlRoot(ElementName = "Edition")]
        public class Edition
        {
            [XmlElement(ElementName = "Name")]
            public string Name
            {
                get; set;
            }

            [XmlElement(ElementName = "ParentEdition")]
            public string ParentEdition
            {
                get; set;
            }

            [XmlAttribute(AttributeName = "virtual")]
            public string Virtual
            {
                get; set;
            }
        }

        [XmlRoot(ElementName = "WindowsEditions")]
        public class WindowsEditions
        {
            [XmlElement(ElementName = "Edition")]
            public List<Edition> Edition
            {
                get; set;
            }

            [XmlAttribute(AttributeName = "e", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string E
            {
                get; set;
            }
        }

        public static WindowsEditions Deserialize(string editionMappingXml)
        {
            if (editionMappingXml == null)
            {
                return null;
            }

            XmlSerializer xmlSerializer = new(typeof(WindowsEditions));

            using StringReader stringReader = new(editionMappingXml);
            return (WindowsEditions)xmlSerializer.Deserialize(stringReader);
        }
    }
}