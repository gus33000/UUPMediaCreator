﻿/*
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
    [XmlRoot(ElementName = "Condition", Namespace = Constants.ImageUpdateNamespace)]
    public class Condition
    {
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
        [XmlAttribute(AttributeName = "FMID", Namespace = Constants.ImageUpdateNamespace)]
        public string FMID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Operator", Namespace = Constants.ImageUpdateNamespace)]
        public string Operator
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "RegistryKey", Namespace = Constants.ImageUpdateNamespace)]
        public string RegistryKey
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Value", Namespace = Constants.ImageUpdateNamespace)]
        public string Value
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "RegistryKeyType", Namespace = Constants.ImageUpdateNamespace)]
        public string RegistryKeyType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Status", Namespace = Constants.ImageUpdateNamespace)]
        public string Status
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "FeatureStatus", Namespace = Constants.ImageUpdateNamespace)]
        public string FeatureStatus
        {
            get; set;
        }
    }
}