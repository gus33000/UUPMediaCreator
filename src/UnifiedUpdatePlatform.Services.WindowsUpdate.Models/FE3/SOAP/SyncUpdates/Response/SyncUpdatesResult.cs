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
using System.Xml;
using System.Xml.Serialization;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.Common;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.SyncUpdates.Response
{
    [XmlRoot(ElementName = "SyncUpdatesResult", Namespace = Constants.ClientWebServiceServerNamespace)]
    public class SyncUpdatesResult
    {
        [XmlElement(ElementName = "NewUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public NewUpdates NewUpdates
        {
            get; set;
        }

        [XmlElement(ElementName = "OutOfScopeRevisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public OutOfScopeRevisionIDs OutOfScopeRevisionIDs
        {
            get; set;
        }

        [XmlElement(ElementName = "Truncated", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string Truncated
        {
            get; set;
        }

        [XmlElement(ElementName = "NewCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
        public Cookie NewCookie
        {
            get; set;
        }

        [XmlElement(ElementName = "DriverSyncNotNeeded", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string DriverSyncNotNeeded
        {
            get; set;
        }

        [XmlElement(ElementName = "ExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
        public ExtendedUpdateInfo ExtendedUpdateInfo
        {
            get; set;
        }
    }
}