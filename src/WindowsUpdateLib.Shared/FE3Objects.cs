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
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace WindowsUpdateLib
{
    #region XML Objects
    public static class CSOAPCommon
    {
        [XmlRoot(ElementName = "Action", Namespace = "http://www.w3.org/2005/08/addressing")]
        public class Action
        {
            [XmlAttribute(AttributeName = "mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public string MustUnderstand { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Update", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Update
        {
            [XmlElement(ElementName = "ID", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "Xml", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Xml { get; set; }
        }

        [XmlRoot(ElementName = "Updates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Updates
        {
            [XmlElement(ElementName = "Update", Namespace = Constants.ClientWebServiceServerNamespace)]
            public Update[] Update { get; set; }
        }

        [XmlRoot(ElementName = "infoTypes", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class InfoTypes
        {
            [XmlElement(ElementName = "XmlUpdateFragmentType", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] XmlUpdateFragmentType { get; set; }
        }

        [XmlRoot(ElementName = "OutOfScopeRevisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class OutOfScopeRevisionIDs
        {
            [XmlElement(ElementName = "int", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "locales", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Locales
        {
            [XmlElement(ElementName = "string", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] String { get; set; }
        }

        public class Cookie
        {
            [XmlElement(ElementName = "Expiration", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Expiration { get; set; }

            [XmlElement(ElementName = "EncryptedData", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string EncryptedData { get; set; }
        }

        [XmlRoot(ElementName = "To", Namespace = "http://www.w3.org/2005/08/addressing")]
        public class To
        {
            [XmlAttribute(AttributeName = "mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public string MustUnderstand { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Timestamp", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public class Timestamp
        {
            [XmlElement(ElementName = "Created", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public string Created { get; set; }

            [XmlElement(ElementName = "Expires", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public string Expires { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }

            [XmlAttribute(AttributeName = "Id", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "TicketType")]
        public class TicketType
        {
            [XmlElement(ElementName = "User")]
            public string User { get; set; }

            [XmlAttribute(AttributeName = "Name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "Version")]
            public string Version { get; set; }

            [XmlAttribute(AttributeName = "Policy")]
            public string Policy { get; set; }
        }

        [XmlRoot(ElementName = "WindowsUpdateTicketsToken", Namespace = Constants.WindowsUpdateAuthorizationSchema)]
        public class WindowsUpdateTicketsToken
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces ns = new(
                new XmlQualifiedName[]
                {
                    new XmlQualifiedName("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"),
                    new XmlQualifiedName("wuws", Constants.WindowsUpdateAuthorizationSchema)
                }
            );

            [XmlElement(ElementName = "TicketType", Namespace = "")]
            public TicketType[] TicketType { get; set; }

            [XmlAttribute(AttributeName = "id", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public string Id { get; set; }

            [XmlAttribute(AttributeName = "wsu", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Wsu { get; set; }

            [XmlAttribute(AttributeName = "wuws", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Wuws { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Security", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
        public class Security
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces ns = new(
                new XmlQualifiedName[]
                {
                    new XmlQualifiedName("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")
                }
            );

            [XmlElement(ElementName = "Timestamp", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public Timestamp Timestamp { get; set; }

            [XmlElement(ElementName = "WindowsUpdateTicketsToken", Namespace = Constants.WindowsUpdateAuthorizationSchema)]
            public WindowsUpdateTicketsToken WindowsUpdateTicketsToken { get; set; }

            [XmlAttribute(AttributeName = "mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public string MustUnderstand { get; set; }

            [XmlAttribute(AttributeName = "o", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string O { get; set; }
        }

        [XmlRoot(ElementName = "Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public class Header
        {
            [XmlElement(ElementName = "Action", Namespace = "http://www.w3.org/2005/08/addressing")]
            public Action Action { get; set; }

            [XmlElement(ElementName = "MessageID", Namespace = "http://www.w3.org/2005/08/addressing")]
            public string MessageID { get; set; }

            [XmlElement(ElementName = "To", Namespace = "http://www.w3.org/2005/08/addressing")]
            public To To { get; set; }

            [XmlElement(ElementName = "RelatesTo", Namespace = "http://www.w3.org/2005/08/addressing")]
            public string RelatesTo { get; set; }

            [XmlElement(ElementName = "Security", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
            public Security Security { get; set; }
        }

        [XmlRoot(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public class Body
        {
            [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Xsi { get; set; }

            [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Xsd { get; set; }

            //
            // Requests
            //

            [XmlElement(ElementName = "GetCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetCookieRequest.GetCookie GetCookie { get; set; }

            [XmlElement(ElementName = "SyncUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSyncUpdatesRequest.SyncUpdates SyncUpdates { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetExtendedUpdateInfoRequest.GetExtendedUpdateInfo GetExtendedUpdateInfo { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo2", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetExtendedUpdateInfo2Request.GetExtendedUpdateInfo2 GetExtendedUpdateInfo2 { get; set; }

            //
            // Responses
            //

            [XmlElement(ElementName = "GetCookieResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetCookieResponse.GetCookieResponse GetCookieResponse { get; set; }

            [XmlElement(ElementName = "SyncUpdatesResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSyncUpdatesResponse.SyncUpdatesResponse SyncUpdatesResponse { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfoResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetExtendedUpdateInfoResponse.GetExtendedUpdateInfoResponse GetExtendedUpdateInfoResponse { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo2Response", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CGetExtendedUpdateInfo2Response.GetExtendedUpdateInfo2Response GetExtendedUpdateInfo2Response { get; set; }
        }

        [XmlRoot(ElementName = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
        public class Envelope
        {
            [XmlElement(ElementName = "Header", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public Header Header { get; set; }

            [XmlElement(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public Body Body { get; set; }

            [XmlAttribute(AttributeName = "a", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string A { get; set; }

            [XmlAttribute(AttributeName = "s", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string S { get; set; }

            [XmlAttribute(AttributeName = "u", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string U { get; set; }
        }
    }

    public static class CGetCookieRequest
    {
        [XmlRoot(ElementName = "oldCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class OldCookie
        {
            [XmlElement(ElementName = "Expiration", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Expiration { get; set; }
        }

        [XmlRoot(ElementName = "GetCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetCookie
        {
            [XmlElement(ElementName = "oldCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
            public OldCookie OldCookie { get; set; }

            [XmlElement(ElementName = "lastChange", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string LastChange { get; set; }

            [XmlElement(ElementName = "currentTime", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string CurrentTime { get; set; }

            [XmlElement(ElementName = "protocolVersion", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ProtocolVersion { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CGetCookieResponse
    {
        [XmlRoot(ElementName = "GetCookieResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetCookieResponse
        {
            [XmlElement(ElementName = "GetCookieResult", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Cookie GetCookieResult { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CSyncUpdatesRequest
    {
        [XmlRoot(ElementName = "InstalledNonLeafUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class InstalledNonLeafUpdateIDs
        {
            [XmlElement(ElementName = "int", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "OtherCachedUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class OtherCachedUpdateIDs
        {
            [XmlElement(ElementName = "int", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "CategoryIdentifier", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class CategoryIdentifier
        {
            [XmlElement(ElementName = "Id", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] Id { get; set; }
        }

        [XmlRoot(ElementName = "FilterAppCategoryIds", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class FilterAppCategoryIds
        {
            [XmlElement(ElementName = "CategoryIdentifier", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CategoryIdentifier CategoryIdentifier { get; set; }
        }

        [XmlRoot(ElementName = "XmlUpdateFragmentTypes", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class XmlUpdateFragmentTypes
        {
            [XmlElement(ElementName = "XmlUpdateFragmentType", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] XmlUpdateFragmentType { get; set; }
        }

        [XmlRoot(ElementName = "ExtendedUpdateInfoParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class ExtendedUpdateInfoParameters
        {
            [XmlElement(ElementName = "XmlUpdateFragmentTypes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public XmlUpdateFragmentTypes XmlUpdateFragmentTypes { get; set; }

            [XmlElement(ElementName = "Locales", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Locales Locales { get; set; }
        }

        [XmlRoot(ElementName = "ProductsParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class ProductsParameters
        {
            [XmlElement(ElementName = "SyncCurrentVersionOnly", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string SyncCurrentVersionOnly { get; set; }

            [XmlElement(ElementName = "DeviceAttributes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string DeviceAttributes { get; set; }

            [XmlElement(ElementName = "CallerAttributes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string CallerAttributes { get; set; }

            [XmlElement(ElementName = "Products", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Products { get; set; }
        }

        [XmlRoot(ElementName = "parameters", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Parameters
        {
            [XmlElement(ElementName = "ExpressQuery", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ExpressQuery { get; set; }

            [XmlElement(ElementName = "InstalledNonLeafUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public InstalledNonLeafUpdateIDs InstalledNonLeafUpdateIDs { get; set; }

            [XmlElement(ElementName = "OtherCachedUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public OtherCachedUpdateIDs OtherCachedUpdateIDs { get; set; }

            [XmlElement(ElementName = "SkipSoftwareSync", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string SkipSoftwareSync { get; set; }

            [XmlElement(ElementName = "NeedTwoGroupOutOfScopeUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string NeedTwoGroupOutOfScopeUpdates { get; set; }

            [XmlElement(ElementName = "FilterAppCategoryIds", Namespace = Constants.ClientWebServiceServerNamespace)]
            public FilterAppCategoryIds FilterAppCategoryIds { get; set; }

            [XmlElement(ElementName = "AlsoPerformRegularSync", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string AlsoPerformRegularSync { get; set; }

            [XmlElement(ElementName = "ComputerSpec", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ComputerSpec { get; set; }

            [XmlElement(ElementName = "ExtendedUpdateInfoParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
            public ExtendedUpdateInfoParameters ExtendedUpdateInfoParameters { get; set; }

            [XmlElement(ElementName = "ClientPreferredLanguages", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ClientPreferredLanguages { get; set; }

            [XmlElement(ElementName = "ProductsParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
            public ProductsParameters ProductsParameters { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class SyncUpdates
        {
            [XmlElement(ElementName = "cookie", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Cookie Cookie { get; set; }

            [XmlElement(ElementName = "parameters", Namespace = Constants.ClientWebServiceServerNamespace)]
            public Parameters Parameters { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CSyncUpdatesResponse
    {
        [XmlRoot(ElementName = "Deployment", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Deployment
        {
            [XmlElement(ElementName = "ID", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "Action", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Action2 { get; set; }

            [XmlElement(ElementName = "IsAssigned", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string IsAssigned { get; set; }

            [XmlElement(ElementName = "LastChangeTime", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string LastChangeTime { get; set; }

            [XmlElement(ElementName = "AutoSelect", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string AutoSelect { get; set; }

            [XmlElement(ElementName = "AutoDownload", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string AutoDownload { get; set; }

            [XmlElement(ElementName = "SupersedenceBehavior", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string SupersedenceBehavior { get; set; }

            [XmlElement(ElementName = "Priority", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Priority { get; set; }

            [XmlElement(ElementName = "HandlerSpecificAction", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string HandlerSpecificAction { get; set; }

            [XmlElement(ElementName = "FlightId", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string FlightId { get; set; }

            [XmlElement(ElementName = "FlightMetadata", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string FlightMetadata { get; set; }
        }

        [XmlRoot(ElementName = "UpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class UpdateInfo
        {
            [XmlElement(ElementName = "ID", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "Deployment", Namespace = Constants.ClientWebServiceServerNamespace)]
            public Deployment Deployment { get; set; }

            [XmlElement(ElementName = "IsLeaf", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string IsLeaf { get; set; }

            [XmlElement(ElementName = "IsShared", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string IsShared { get; set; }

            [XmlElement(ElementName = "Xml", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Xml { get; set; }
        }

        [XmlRoot(ElementName = "NewUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class NewUpdates
        {
            [XmlElement(ElementName = "UpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
            public UpdateInfo[] UpdateInfo { get; set; }
        }

        [XmlRoot(ElementName = "ExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class ExtendedUpdateInfo
        {
            [XmlElement(ElementName = "Updates", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Updates Updates { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdatesResult", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class SyncUpdatesResult
        {
            [XmlElement(ElementName = "NewUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
            public NewUpdates NewUpdates { get; set; }

            [XmlElement(ElementName = "OutOfScopeRevisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.OutOfScopeRevisionIDs OutOfScopeRevisionIDs { get; set; }

            [XmlElement(ElementName = "Truncated", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Truncated { get; set; }

            [XmlElement(ElementName = "NewCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Cookie NewCookie { get; set; }

            [XmlElement(ElementName = "DriverSyncNotNeeded", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string DriverSyncNotNeeded { get; set; }

            [XmlElement(ElementName = "ExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
            public ExtendedUpdateInfo ExtendedUpdateInfo { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdatesResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class SyncUpdatesResponse
        {
            [XmlElement(ElementName = "SyncUpdatesResult", Namespace = Constants.ClientWebServiceServerNamespace)]
            public SyncUpdatesResult SyncUpdatesResult { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CGetExtendedUpdateInfo2Request
    {
        [XmlRoot(ElementName = "UpdateIdentity", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class UpdateIdentity
        {
            [XmlElement(ElementName = "UpdateID", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string UpdateID { get; set; }

            [XmlElement(ElementName = "RevisionNumber", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string RevisionNumber { get; set; }
        }

        [XmlRoot(ElementName = "updateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class UpdateIDs
        {
            [XmlElement(ElementName = "UpdateIdentity", Namespace = Constants.ClientWebServiceServerNamespace)]
            public UpdateIdentity UpdateIdentity { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfo2
        {
            [XmlElement(ElementName = "updateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public UpdateIDs UpdateIDs { get; set; }

            [XmlElement(ElementName = "infoTypes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.InfoTypes InfoTypes { get; set; }

            [XmlElement(ElementName = "deviceAttributes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string DeviceAttributes { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CGetExtendedUpdateInfo2Response
    {
        [XmlRoot(ElementName = "FileLocation", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class FileLocation
        {
            [XmlElement(ElementName = "FileDigest", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string FileDigest { get; set; }

            [XmlElement(ElementName = "Url", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string Url { get; set; }

            [XmlElement(ElementName = "PiecesHashUrl", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string PiecesHashUrl { get; set; }

            [XmlElement(ElementName = "BlockMapUrl", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string BlockMapUrl { get; set; }

            [XmlElement(ElementName = "EsrpDecryptionInformation", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string EsrpDecryptionInformation { get; set; }
        }

        [XmlRoot(ElementName = "FileLocations", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class FileLocations
        {
            [XmlElement(ElementName = "FileLocation", Namespace = Constants.ClientWebServiceServerNamespace)]
            public FileLocation[] FileLocation { get; set; }
        }

        [XmlRoot(ElementName = "FileDecryptionData", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class FileDecryptionData
        {
            [XmlElement(ElementName = "FileDecryption", Namespace = Constants.ClientWebServiceServerNamespace)]
            public FileDecryption FileDecryption { get; set; }
        }

        [XmlRoot(ElementName = "FileDecryption", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class FileDecryption
        {
            [XmlElement(ElementName = "FileDigest", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string FileDigest { get; set; }

            [XmlElement(ElementName = "DecryptionKey", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string DecryptionKey { get; set; }

            [XmlElement(ElementName = "SecurityData", Namespace = Constants.ClientWebServiceServerNamespace)]
            public SecurityData SecurityData { get; set; }
        }

        [XmlRoot(ElementName = "SecurityData", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class SecurityData
        {
            [XmlElement(ElementName = "base64Binary", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string base64Binary { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2Result", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfo2Result
        {
            [XmlElement(ElementName = "FileLocations", Namespace = Constants.ClientWebServiceServerNamespace)]
            public FileLocations FileLocations { get; set; }

            [XmlElement(ElementName = "FileDecryptionData", Namespace = Constants.ClientWebServiceServerNamespace)]
            public FileDecryptionData FileDecryptionData { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2Response", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfo2Response
        {
            [XmlElement(ElementName = "GetExtendedUpdateInfo2Result", Namespace = Constants.ClientWebServiceServerNamespace)]
            public GetExtendedUpdateInfo2Result GetExtendedUpdateInfo2Result { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CGetExtendedUpdateInfoRequest
    {
        [XmlRoot(ElementName = "revisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class RevisionIDs
        {
            [XmlElement(ElementName = "int", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfo
        {
            [XmlElement(ElementName = "cookie", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Cookie Cookie { get; set; }

            [XmlElement(ElementName = "revisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public RevisionIDs RevisionIDs { get; set; }

            [XmlElement(ElementName = "infoTypes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.InfoTypes InfoTypes { get; set; }

            [XmlElement(ElementName = "locales", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Locales Locales { get; set; }

            [XmlElement(ElementName = "deviceAttributes", Namespace = Constants.ClientWebServiceServerNamespace)]
            public string DeviceAttributes { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public static class CGetExtendedUpdateInfoResponse
    {
        [XmlRoot(ElementName = "GetExtendedUpdateInfoResult", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfoResult
        {
            [XmlElement(ElementName = "OutOfScopeRevisionIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.OutOfScopeRevisionIDs OutOfScopeRevisionIDs { get; set; }

            [XmlElement(ElementName = "Updates", Namespace = Constants.ClientWebServiceServerNamespace)]
            public CSOAPCommon.Updates Updates { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfoResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class GetExtendedUpdateInfoResponse
        {
            [XmlElement(ElementName = "GetExtendedUpdateInfoResult", Namespace = Constants.ClientWebServiceServerNamespace)]
            public GetExtendedUpdateInfoResult GetExtendedUpdateInfoResult { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }

        [XmlRoot(ElementName = "Verification", Namespace = Constants.ClientWebServiceServerNamespace)]
        public class Verification
        {
            [XmlAttribute(AttributeName = "Timestamp")]
            public string Timestamp { get; set; }

            [XmlAttribute(AttributeName = "LeafCertificateId")]
            public string LeafCertificateId { get; set; }

            [XmlAttribute(AttributeName = "Signature")]
            public string Signature { get; set; }

            [XmlAttribute(AttributeName = "Algorithm")]
            public string Algorithm { get; set; }
        }
    }

    public static class CExtendedUpdateInfoXml
    {
        [XmlRoot(ElementName = "LocalizedProperties")]
        public class LocalizedProperties
        {
            [XmlElement(ElementName = "Language")]
            public string Language { get; set; }

            [XmlElement(ElementName = "Title")]
            public string Title { get; set; }

            [XmlElement(ElementName = "Description")]
            public string Description { get; set; }
        }

        [XmlRoot(ElementName = "UpdateIdentity")]
        public class UpdateIdentity
        {
            [XmlAttribute(AttributeName = "UpdateID")]
            public string UpdateID { get; set; }

            [XmlAttribute(AttributeName = "RevisionNumber")]
            public string RevisionNumber { get; set; }
        }

        [XmlRoot(ElementName = "Properties")]
        public class Properties
        {
            [XmlAttribute(AttributeName = "UpdateType")]
            public string UpdateType { get; set; }

            [XmlAttribute(AttributeName = "PerUser")]
            public string PerUser { get; set; }

            [XmlElement(ElementName = "SecuredFragment")]
            public string SecuredFragment { get; set; }

            [XmlAttribute(AttributeName = "PackageRank")]
            public string PackageRank { get; set; }

            [XmlAttribute(AttributeName = "ExplicitlyDeployable")]
            public string ExplicitlyDeployable { get; set; }

            [XmlAttribute(AttributeName = "ApplyPackageRank")]
            public string ApplyPackageRank { get; set; }
        }

        [XmlRoot(ElementName = "b.WindowsVersion")]
        public class BWindowsVersion
        {
            [XmlAttribute(AttributeName = "Comparison")]
            public string Comparison { get; set; }

            [XmlAttribute(AttributeName = "MajorVersion")]
            public string MajorVersion { get; set; }

            [XmlAttribute(AttributeName = "MinorVersion")]
            public string MinorVersion { get; set; }

            [XmlAttribute(AttributeName = "BuildNumber")]
            public string BuildNumber { get; set; }
        }

        [XmlRoot(ElementName = "IsInstalled")]
        public class IsInstalled
        {
            [XmlElement(ElementName = "b.WindowsVersion")]
            public BWindowsVersion BWindowsVersion { get; set; }

            [XmlElement(ElementName = "True")]
            public string True { get; set; }

            [XmlElement(ElementName = "Or")]
            public Or Or { get; set; }

            [XmlElement(ElementName = "And")]
            public And And { get; set; }

            [XmlElement(ElementName = "b.WmiQuery")]
            public BWmiQuery BWmiQuery { get; set; }

            [XmlElement(ElementName = "b.RegSz")]
            public BRegSz BRegSz { get; set; }

            [XmlElement(ElementName = "b.SystemMetric")]
            public BSystemMetric BSystemMetric { get; set; }

            [XmlElement(ElementName = "b.Memory")]
            public BMemory BMemory { get; set; }

            [XmlElement(ElementName = "b.Direct3D")]
            public BDirect3D BDirect3D { get; set; }

            [XmlElement(ElementName = "b.SensorById")]
            public BSensorById BSensorById { get; set; }

            [XmlElement(ElementName = "b.Camera")]
            public BCamera BCamera { get; set; }

            [XmlElement(ElementName = "b.NFC")]
            public BNFC BNFC { get; set; }

            [XmlElement(ElementName = "b.VideoMemory")]
            public BVideoMemory BVideoMemory { get; set; }

            [XmlElement(ElementName = "AppxPackageInstalled")]
            public string AppxPackageInstalled { get; set; }
        }

        [XmlRoot(ElementName = "ApplicabilityRules")]
        public class ApplicabilityRules
        {
            [XmlElement(ElementName = "IsInstalled")]
            public IsInstalled IsInstalled { get; set; }

            [XmlElement(ElementName = "b.WindowsVersion")]
            public BWindowsVersion BWindowsVersion { get; set; }

            [XmlElement(ElementName = "Metadata")]
            public Metadata Metadata { get; set; }

            [XmlElement(ElementName = "IsInstallable")]
            public IsInstallable IsInstallable { get; set; }
        }

        [XmlRoot(ElementName = "Xml")]
        public class Xml
        {
            [XmlElement(ElementName = "LocalizedProperties")]
            public LocalizedProperties LocalizedProperties { get; set; }

            [XmlElement(ElementName = "UpdateIdentity")]
            public UpdateIdentity UpdateIdentity { get; set; }

            [XmlElement(ElementName = "Properties")]
            public Properties Properties { get; set; }

            [XmlElement(ElementName = "ApplicabilityRules")]
            public ApplicabilityRules ApplicabilityRules { get; set; }

            [XmlElement(ElementName = "ExtendedProperties")]
            public ExtendedProperties ExtendedProperties { get; set; }

            [XmlElement(ElementName = "HandlerSpecificData")]
            public HandlerSpecificData HandlerSpecificData { get; set; }

            [XmlElement(ElementName = "Relationships")]
            public Relationships Relationships { get; set; }

            [XmlElement(ElementName = "Files")]
            public Files Files { get; set; }
        }

        [XmlRoot(ElementName = "ExtendedProperties")]
        public class ExtendedProperties
        {
            [XmlAttribute(AttributeName = "DefaultPropertiesLanguage")]
            public string DefaultPropertiesLanguage { get; set; }

            [XmlAttribute(AttributeName = "Handler")]
            public string Handler { get; set; }

            [XmlAttribute(AttributeName = "CreationDate")]
            public string CreationDate { get; set; }

            [XmlAttribute(AttributeName = "IsAppxFramework")]
            public string IsAppxFramework { get; set; }

            [XmlAttribute(AttributeName = "CompatibleProtocolVersion")]
            public string CompatibleProtocolVersion { get; set; }

            [XmlAttribute(AttributeName = "FromStoreService")]
            public string FromStoreService { get; set; }

            [XmlAttribute(AttributeName = "ContentType")]
            public string ContentType { get; set; }

            [XmlAttribute(AttributeName = "PackageIdentityName")]
            public string PackageIdentityName { get; set; }

            [XmlAttribute(AttributeName = "LegacyMobileProductId")]
            public string LegacyMobileProductId { get; set; }

            [XmlElement(ElementName = "InstallationBehavior")]
            public string InstallationBehavior { get; set; }

            [XmlAttribute(AttributeName = "MaxDownloadSize")]
            public string MaxDownloadSize { get; set; }

            [XmlAttribute(AttributeName = "MinDownloadSize")]
            public string MinDownloadSize { get; set; }

            [XmlAttribute(AttributeName = "PackageContentId")]
            public string PackageContentId { get; set; }

            [XmlAttribute(AttributeName = "ProductName")]
            public string ProductName { get; set; }

            [XmlAttribute(AttributeName = "ReleaseVersion")]
            public string ReleaseVersion { get; set; }

            [XmlAttribute(AttributeName = "AutoSelectOnWebsites")]
            public string AutoSelectOnWebsites { get; set; }

            [XmlAttribute(AttributeName = "BrowseOnly")]
            public string BrowseOnly { get; set; }
        }

        [XmlRoot(ElementName = "CategoryInformation")]
        public class CategoryInformation
        {
            [XmlAttribute(AttributeName = "CategoryType")]
            public string CategoryType { get; set; }

            [XmlAttribute(AttributeName = "ProhibitsSubcategories")]
            public string ProhibitsSubcategories { get; set; }

            [XmlAttribute(AttributeName = "ProhibitsUpdates")]
            public string ProhibitsUpdates { get; set; }

            [XmlAttribute(AttributeName = "DisplayOrder")]
            public string DisplayOrder { get; set; }

            [XmlAttribute(AttributeName = "ExcludedByDefault")]
            public string ExcludedByDefault { get; set; }
        }

        [XmlRoot(ElementName = "HandlerSpecificData")]
        public class HandlerSpecificData
        {
            [XmlElement(ElementName = "CategoryInformation")]
            public CategoryInformation CategoryInformation { get; set; }

            [XmlAttribute(AttributeName = "type")]
            public string Type { get; set; }

            [XmlElement(ElementName = "AppxPackageInstallData")]
            public AppxPackageInstallData AppxPackageInstallData { get; set; }
        }

        [XmlRoot(ElementName = "b.RegValueExists")]
        public class BRegValueExists
        {
            [XmlAttribute(AttributeName = "Key")]
            public string Key { get; set; }

            [XmlAttribute(AttributeName = "Subkey")]
            public string Subkey { get; set; }

            [XmlAttribute(AttributeName = "Value")]
            public string Value { get; set; }

            [XmlAttribute(AttributeName = "Type")]
            public string Type { get; set; }

            [XmlAttribute(AttributeName = "RegType32")]
            public string RegType32 { get; set; }
        }

        [XmlRoot(ElementName = "Not")]
        public class Not
        {
            [XmlElement(ElementName = "b.RegValueExists")]
            public BRegValueExists BRegValueExists { get; set; }

            [XmlElement(ElementName = "b.RegDword")]
            public BRegDword BRegDword { get; set; }
        }

        [XmlRoot(ElementName = "b.RegDword")]
        public class BRegDword
        {
            [XmlAttribute(AttributeName = "Key")]
            public string Key { get; set; }

            [XmlAttribute(AttributeName = "Subkey")]
            public string Subkey { get; set; }

            [XmlAttribute(AttributeName = "Value")]
            public string Value { get; set; }

            [XmlAttribute(AttributeName = "Comparison")]
            public string Comparison { get; set; }

            [XmlAttribute(AttributeName = "Data")]
            public string Data { get; set; }
        }

        [XmlRoot(ElementName = "Or")]
        public class Or
        {
            [XmlElement(ElementName = "Not")]
            public Not Not { get; set; }

            [XmlElement(ElementName = "b.RegDword")]
            public BRegDword BRegDword { get; set; }

            [XmlElement(ElementName = "And")]
            public And[] And { get; set; }

            [XmlElement(ElementName = "b.RegSz")]
            public BRegSz BRegSz { get; set; }

            [XmlElement(ElementName = "b.WindowsVersion")]
            public BWindowsVersion BWindowsVersion { get; set; }

            /*[XmlElement(ElementName = "Or")]
            public Or Or { get; set; }*/ // TODO: fix
        }

        [XmlRoot(ElementName = "And")]
        public class And
        {
            [XmlElement(ElementName = "b.WindowsVersion")]
            public BWindowsVersion[] BWindowsVersion { get; set; }

            [XmlElement(ElementName = "Not")]
            public Not[] Not { get; set; }

            [XmlElement(ElementName = "b.RegDword")]
            public BRegDword[] BRegDword { get; set; }

            [XmlElement(ElementName = "Or")]
            public Or[] Or { get; set; }

            [XmlElement(ElementName = "b.SystemMetric")]
            public BSystemMetric[] BSystemMetric { get; set; }

            [XmlElement(ElementName = "b.RegSz")]
            public BRegSz BRegSz { get; set; }
        }

        [XmlRoot(ElementName = "b.WmiQuery")]
        public class BWmiQuery
        {
            [XmlAttribute(AttributeName = "Namespace")]
            public string Namespace { get; set; }

            [XmlAttribute(AttributeName = "WqlQuery")]
            public string WqlQuery { get; set; }
        }

        [XmlRoot(ElementName = "b.SystemMetric")]
        public class BSystemMetric
        {
            [XmlAttribute(AttributeName = "Comparison")]
            public string Comparison { get; set; }

            [XmlAttribute(AttributeName = "Index")]
            public string Index { get; set; }

            [XmlAttribute(AttributeName = "Value")]
            public string Value { get; set; }
        }

        [XmlRoot(ElementName = "b.RegSz")]
        public class BRegSz
        {
            [XmlAttribute(AttributeName = "Key")]
            public string Key { get; set; }

            [XmlAttribute(AttributeName = "Subkey")]
            public string Subkey { get; set; }

            [XmlAttribute(AttributeName = "Value")]
            public string Value { get; set; }

            [XmlAttribute(AttributeName = "Comparison")]
            public string Comparison { get; set; }

            [XmlAttribute(AttributeName = "Data")]
            public string Data { get; set; }

            [XmlAttribute(AttributeName = "RegType32")]
            public string RegType32 { get; set; }
        }

        [XmlRoot(ElementName = "Prerequisites")]
        public class Prerequisites
        {
            [XmlElement(ElementName = "UpdateIdentity")]
            public UpdateIdentity[] UpdateIdentity { get; set; }

            [XmlElement(ElementName = "AtLeastOne")]
            public AtLeastOne[] AtLeastOne { get; set; }
        }

        [XmlRoot(ElementName = "Relationships")]
        public class Relationships
        {
            [XmlElement(ElementName = "Prerequisites")]
            public Prerequisites Prerequisites { get; set; }

            [XmlElement(ElementName = "BundledUpdates")]
            public BundledUpdates BundledUpdates { get; set; }
        }

        [XmlRoot(ElementName = "b.Memory")]
        public class BMemory
        {
            [XmlAttribute(AttributeName = "MinSizeInMB")]
            public string MinSizeInMB { get; set; }

            [XmlAttribute(AttributeName = "Type")]
            public string Type { get; set; }
        }

        [XmlRoot(ElementName = "b.Direct3D")]
        public class BDirect3D
        {
            [XmlAttribute(AttributeName = "HardwareVersion")]
            public string HardwareVersion { get; set; }

            [XmlAttribute(AttributeName = "FeatureLevelMajor")]
            public string FeatureLevelMajor { get; set; }

            [XmlAttribute(AttributeName = "FeatureLevelMinor")]
            public string FeatureLevelMinor { get; set; }
        }

        [XmlRoot(ElementName = "b.SensorById")]
        public class BSensorById
        {
            [XmlAttribute(AttributeName = "Id")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "AtLeastOne")]
        public class AtLeastOne
        {
            [XmlElement(ElementName = "UpdateIdentity")]
            public UpdateIdentity[] UpdateIdentity { get; set; }

            [XmlAttribute(AttributeName = "IsCategory")]
            public string IsCategory { get; set; }
        }

        [XmlRoot(ElementName = "b.Camera")]
        public class BCamera
        {
            [XmlAttribute(AttributeName = "Location")]
            public string Location { get; set; }
        }

        [XmlRoot(ElementName = "b.NFC")]
        public class BNFC
        {
            [XmlAttribute(AttributeName = "Capability")]
            public string Capability { get; set; }
        }

        [XmlRoot(ElementName = "b.VideoMemory")]
        public class BVideoMemory
        {
            [XmlAttribute(AttributeName = "MinSizeInMB")]
            public string MinSizeInMB { get; set; }
        }

        [XmlRoot(ElementName = "AppxFamilyMetadata")]
        public class AppxFamilyMetadata
        {
            [XmlAttribute(AttributeName = "Name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "Publisher")]
            public string Publisher { get; set; }

            [XmlAttribute(AttributeName = "LegacyMobileProductId")]
            public string LegacyMobileProductId { get; set; }
        }

        [XmlRoot(ElementName = "AppxPackageMetadata")]
        public class AppxPackageMetadata
        {
            [XmlElement(ElementName = "AppxFamilyMetadata")]
            public AppxFamilyMetadata AppxFamilyMetadata { get; set; }

            [XmlElement(ElementName = "AppxMetadata")]
            public AppxMetadata AppxMetadata { get; set; }
        }

        [XmlRoot(ElementName = "Metadata")]
        public class Metadata
        {
            [XmlElement(ElementName = "AppxPackageMetadata")]
            public AppxPackageMetadata AppxPackageMetadata { get; set; }
        }

        [XmlRoot(ElementName = "IsInstallable")]
        public class IsInstallable
        {
            [XmlElement(ElementName = "AppxPackageInstallable")]
            public string AppxPackageInstallable { get; set; }
        }

        [XmlRoot(ElementName = "AppxMetadata")]
        public class AppxMetadata
        {
            [XmlElement(ElementName = "ApplicabilityBlob")]
            public string ApplicabilityBlob { get; set; }

            [XmlAttribute(AttributeName = "PackageType")]
            public string PackageType { get; set; }

            [XmlAttribute(AttributeName = "IsAppxBundle")]
            public string IsAppxBundle { get; set; }

            [XmlAttribute(AttributeName = "PackageMoniker")]
            public string PackageMoniker { get; set; }
        }

        [XmlRoot(ElementName = "BundledUpdates")]
        public class BundledUpdates
        {
            [XmlElement(ElementName = "AtLeastOne")]
            public AtLeastOne AtLeastOne { get; set; }
        }

        [XmlRoot(ElementName = "AdditionalDigest")]
        public class AdditionalDigest
        {
            [XmlAttribute(AttributeName = "Algorithm")]
            public string Algorithm { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "PiecesHashDigest")]
        public class PiecesHashDigest
        {
            [XmlAttribute(AttributeName = "Algorithm")]
            public string Algorithm { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "BlockMapDigest")]
        public class BlockMapDigest
        {
            [XmlAttribute(AttributeName = "Algorithm")]
            public string Algorithm { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "File")]
        public class File
        {
            [XmlElement(ElementName = "AdditionalDigest")]
            public AdditionalDigest AdditionalDigest { get; set; }

            [XmlElement(ElementName = "PiecesHashDigest")]
            public PiecesHashDigest PiecesHashDigest { get; set; }

            [XmlElement(ElementName = "BlockMapDigest")]
            public BlockMapDigest BlockMapDigest { get; set; }

            [XmlAttribute(AttributeName = "FileName")]
            public string FileName { get; set; }

            [XmlAttribute(AttributeName = "Digest")]
            public string Digest { get; set; }

            [XmlAttribute(AttributeName = "DigestAlgorithm")]
            public string DigestAlgorithm { get; set; }

            [XmlAttribute(AttributeName = "Size")]
            public string Size { get; set; }

            [XmlAttribute(AttributeName = "Modified")]
            public string Modified { get; set; }

            [XmlAttribute(AttributeName = "InstallerSpecificIdentifier")]
            public string InstallerSpecificIdentifier { get; set; }

            [XmlAttribute(AttributeName = "PatchingType")]
            public string PatchingType { get; set; }
        }

        [XmlRoot(ElementName = "Files")]
        public class Files
        {
            [XmlElement(ElementName = "File")]
            public File[] File { get; set; }
        }

        [XmlRoot(ElementName = "AppxPackageInstallData")]
        public class AppxPackageInstallData
        {
            [XmlAttribute(AttributeName = "PackageFileName")]
            public string PackageFileName { get; set; }

            [XmlAttribute(AttributeName = "MainPackage")]
            public string MainPackage { get; set; }
        }
    }
    #endregion XML Objects

    #region JSON Objects
    public class EsrpDecryptionInformation
    {
        [DataMember(Name = "KeyData")]
        public string KeyData { get; set; }

        [DataMember(Name = "EncryptionBufferSize")]
        public long EncryptionBufferSize { get; set; }

        [DataMember(Name = "AlgorithmName")]
        public string AlgorithmName { get; set; }

        [DataMember(Name = "ChainingMode")]
        public string ChainingMode { get; set; }

        public static EsrpDecryptionInformation DeserializeFromJson(string json)
        {
            using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer serializer = new(typeof(EsrpDecryptionInformation));
            return serializer.ReadObject(memoryStream) as EsrpDecryptionInformation;
        }
    }

    public static class CAppxMetadataJSON
    {
        public partial class AppxMetadataJson
        {
            [JsonPropertyName("blob.version")]
            public long BlobVersion { get; set; }

            [JsonPropertyName("content.isMain")]
            public bool ContentIsMain { get; set; }

            [JsonPropertyName("content.packageId")]
            public string ContentPackageId { get; set; }

            [JsonPropertyName("content.productId")]
            public Guid ContentProductId { get; set; }

            [JsonPropertyName("content.targetPlatforms")]
            public ContentTargetPlatform[] ContentTargetPlatforms { get; set; }

            [JsonPropertyName("content.type")]
            public long ContentType { get; set; }

            [JsonPropertyName("policy")]
            public Policy Policy { get; set; }

            [JsonPropertyName("policy2")]
            public Policy2 Policy2 { get; set; }
        }

        public partial class ContentTargetPlatform
        {
            [JsonPropertyName("platform.maxVersionTested")]
            public long PlatformMaxVersionTested { get; set; }

            [JsonPropertyName("platform.minVersion")]
            public long PlatformMinVersion { get; set; }

            [JsonPropertyName("platform.target")]
            public long PlatformTarget { get; set; }
        }

        public partial class Policy
        {
            [JsonPropertyName("category.first")]
            public string CategoryFirst { get; set; }

            [JsonPropertyName("category.second")]
            public string CategorySecond { get; set; }

            [JsonPropertyName("category.third")]
            public string CategoryThird { get; set; }

            [JsonPropertyName("optOut.backupRestore")]
            public bool OptOutBackupRestore { get; set; }

            [JsonPropertyName("optOut.removeableMedia")]
            public bool OptOutRemoveableMedia { get; set; }
        }

        public partial class Policy2
        {
            [JsonPropertyName("ageRating")]
            public long AgeRating { get; set; }

            [JsonPropertyName("optOut.DVR")]
            public bool OptOutDvr { get; set; }

            [JsonPropertyName("thirdPartyAppRatings")]
            public ThirdPartyAppRating[] ThirdPartyAppRatings { get; set; }
        }

        public partial class ThirdPartyAppRating
        {
            [JsonPropertyName("level")]
            public long Level { get; set; }

            [JsonPropertyName("systemId")]
            public long SystemId { get; set; }
        }
    }
    #endregion JSON Objects
}
