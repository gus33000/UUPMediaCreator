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
    [XmlRoot(ElementName = "PayloadItem", Namespace = Constants.ImageUpdateNamespace)]
    public class PayloadItem
    {
        [XmlAttribute(AttributeName = "PayloadHash", Namespace = Constants.ImageUpdateNamespace)]
        public string SourceHash
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "PayloadSize", Namespace = Constants.ImageUpdateNamespace)]
        public string PayloadSize
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Path", Namespace = Constants.ImageUpdateNamespace)]
        public string SourceName
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "PayloadType", Namespace = Constants.ImageUpdateNamespace)]
        public string PayloadType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "AltSourceName", Namespace = Constants.ImageUpdateNamespace)]
        public string AltSourceName
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "AltSourceHash", Namespace = Constants.ImageUpdateNamespace)]
        public string AltSourceHash
        {
            get; set;
        }

        public string Path => AltSourceName ?? SourceName;
        public string PayloadHash => AltSourceHash ?? SourceHash;
    }
}