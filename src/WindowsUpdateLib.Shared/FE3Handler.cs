using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WindowsUpdateLib
{
    #region XML Objects
    public class CSOAPCommon
    {
        [XmlRoot(ElementName = "Action", Namespace = "http://www.w3.org/2005/08/addressing")]
        public class Action
        {
            [XmlAttribute(AttributeName = "mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
            public string MustUnderstand { get; set; }
            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Update", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class Update
        {
            [XmlElement(ElementName = "ID", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ID { get; set; }
            [XmlElement(ElementName = "Xml", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Xml { get; set; }
        }

        [XmlRoot(ElementName = "Updates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class Updates
        {
            [XmlElement(ElementName = "Update", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public Update[] Update { get; set; }
        }

        [XmlRoot(ElementName = "infoTypes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class InfoTypes
        {
            [XmlElement(ElementName = "XmlUpdateFragmentType", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] XmlUpdateFragmentType { get; set; }
        }

        [XmlRoot(ElementName = "OutOfScopeRevisionIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class OutOfScopeRevisionIDs
        {
            [XmlElement(ElementName = "int", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "locales", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class Locales
        {
            [XmlElement(ElementName = "string", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] String { get; set; }
        }

        public class Cookie
        {
            [XmlElement(ElementName = "Expiration", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Expiration { get; set; }
            [XmlElement(ElementName = "EncryptedData", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
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

        [XmlRoot(ElementName = "WindowsUpdateTicketsToken", Namespace = "http://schemas.microsoft.com/msus/2014/10/WindowsUpdateAuthorization")]
        public class WindowsUpdateTicketsToken
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces ns = new XmlSerializerNamespaces(
                new XmlQualifiedName[]
                {
                    new XmlQualifiedName("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"),
                    new XmlQualifiedName("wuws", "http://schemas.microsoft.com/msus/2014/10/WindowsUpdateAuthorization")
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
            public XmlSerializerNamespaces ns = new XmlSerializerNamespaces(
                new XmlQualifiedName[]
                {
                    new XmlQualifiedName("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")
                }
            );

            [XmlElement(ElementName = "Timestamp", Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
            public Timestamp Timestamp { get; set; }
            [XmlElement(ElementName = "WindowsUpdateTicketsToken", Namespace = "http://schemas.microsoft.com/msus/2014/10/WindowsUpdateAuthorization")]
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

            [XmlElement(ElementName = "GetCookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CGetCookieRequest.GetCookie GetCookie { get; set; }

            [XmlElement(ElementName = "SyncUpdates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSyncUpdatesRequest.SyncUpdates SyncUpdates { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CGetExtendedUpdateInfoRequest.GetExtendedUpdateInfo GetExtendedUpdateInfo { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo2", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CGetExtendedUpdateInfo2Request.GetExtendedUpdateInfo2 GetExtendedUpdateInfo2 { get; set; }

            //
            // Responses
            //

            [XmlElement(ElementName = "GetCookieResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CGetCookieResponse.GetCookieResponse GetCookieResponse { get; set; }

            [XmlElement(ElementName = "SyncUpdatesResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSyncUpdatesResponse.SyncUpdatesResponse SyncUpdatesResponse { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfoResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CGetExtendedUpdateInfoResponse.GetExtendedUpdateInfoResponse GetExtendedUpdateInfoResponse { get; set; }

            [XmlElement(ElementName = "GetExtendedUpdateInfo2Response", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
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

    public class CGetCookieRequest
    {
        [XmlRoot(ElementName = "oldCookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class OldCookie
        {
            [XmlElement(ElementName = "Expiration", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Expiration { get; set; }
        }

        [XmlRoot(ElementName = "GetCookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetCookie
        {
            [XmlElement(ElementName = "oldCookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public OldCookie OldCookie { get; set; }
            [XmlElement(ElementName = "lastChange", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string LastChange { get; set; }
            [XmlElement(ElementName = "currentTime", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string CurrentTime { get; set; }
            [XmlElement(ElementName = "protocolVersion", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ProtocolVersion { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CGetCookieResponse
    {
        [XmlRoot(ElementName = "GetCookieResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetCookieResponse
        {
            [XmlElement(ElementName = "GetCookieResult", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Cookie GetCookieResult { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CSyncUpdatesRequest
    {
        [XmlRoot(ElementName = "InstalledNonLeafUpdateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class InstalledNonLeafUpdateIDs
        {
            [XmlElement(ElementName = "int", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "OtherCachedUpdateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class OtherCachedUpdateIDs
        {
            [XmlElement(ElementName = "int", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "CategoryIdentifier", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class CategoryIdentifier
        {
            [XmlElement(ElementName = "Id", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] Id { get; set; }
        }

        [XmlRoot(ElementName = "FilterAppCategoryIds", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class FilterAppCategoryIds
        {
            [XmlElement(ElementName = "CategoryIdentifier", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CategoryIdentifier CategoryIdentifier { get; set; }
        }

        [XmlRoot(ElementName = "XmlUpdateFragmentTypes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class XmlUpdateFragmentTypes
        {
            [XmlElement(ElementName = "XmlUpdateFragmentType", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] XmlUpdateFragmentType { get; set; }
        }

        [XmlRoot(ElementName = "ExtendedUpdateInfoParameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class ExtendedUpdateInfoParameters
        {
            [XmlElement(ElementName = "XmlUpdateFragmentTypes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public XmlUpdateFragmentTypes XmlUpdateFragmentTypes { get; set; }
            [XmlElement(ElementName = "Locales", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Locales Locales { get; set; }
        }

        [XmlRoot(ElementName = "ProductsParameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class ProductsParameters
        {
            [XmlElement(ElementName = "SyncCurrentVersionOnly", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string SyncCurrentVersionOnly { get; set; }
            [XmlElement(ElementName = "DeviceAttributes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string DeviceAttributes { get; set; }
            [XmlElement(ElementName = "CallerAttributes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string CallerAttributes { get; set; }
            [XmlElement(ElementName = "Products", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Products { get; set; }
        }

        [XmlRoot(ElementName = "parameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class Parameters
        {
            [XmlElement(ElementName = "ExpressQuery", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ExpressQuery { get; set; }
            [XmlElement(ElementName = "InstalledNonLeafUpdateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public InstalledNonLeafUpdateIDs InstalledNonLeafUpdateIDs { get; set; }
            [XmlElement(ElementName = "OtherCachedUpdateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public OtherCachedUpdateIDs OtherCachedUpdateIDs { get; set; }
            [XmlElement(ElementName = "SkipSoftwareSync", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string SkipSoftwareSync { get; set; }
            [XmlElement(ElementName = "NeedTwoGroupOutOfScopeUpdates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string NeedTwoGroupOutOfScopeUpdates { get; set; }
            [XmlElement(ElementName = "FilterAppCategoryIds", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public FilterAppCategoryIds FilterAppCategoryIds { get; set; }
            [XmlElement(ElementName = "AlsoPerformRegularSync", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string AlsoPerformRegularSync { get; set; }
            [XmlElement(ElementName = "ComputerSpec", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ComputerSpec { get; set; }
            [XmlElement(ElementName = "ExtendedUpdateInfoParameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public ExtendedUpdateInfoParameters ExtendedUpdateInfoParameters { get; set; }
            [XmlElement(ElementName = "ClientPreferredLanguages", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ClientPreferredLanguages { get; set; }
            [XmlElement(ElementName = "ProductsParameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public ProductsParameters ProductsParameters { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class SyncUpdates
        {
            [XmlElement(ElementName = "cookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Cookie Cookie { get; set; }
            [XmlElement(ElementName = "parameters", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public Parameters Parameters { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CSyncUpdatesResponse
    {
        [XmlRoot(ElementName = "Deployment", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class Deployment
        {
            [XmlElement(ElementName = "ID", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ID { get; set; }
            [XmlElement(ElementName = "Action", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Action2 { get; set; }
            [XmlElement(ElementName = "IsAssigned", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string IsAssigned { get; set; }
            [XmlElement(ElementName = "LastChangeTime", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string LastChangeTime { get; set; }
            [XmlElement(ElementName = "AutoSelect", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string AutoSelect { get; set; }
            [XmlElement(ElementName = "AutoDownload", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string AutoDownload { get; set; }
            [XmlElement(ElementName = "SupersedenceBehavior", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string SupersedenceBehavior { get; set; }
            [XmlElement(ElementName = "Priority", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Priority { get; set; }
            [XmlElement(ElementName = "HandlerSpecificAction", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string HandlerSpecificAction { get; set; }
        }

        [XmlRoot(ElementName = "UpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class UpdateInfo
        {
            [XmlElement(ElementName = "ID", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string ID { get; set; }
            [XmlElement(ElementName = "Deployment", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public Deployment Deployment { get; set; }
            [XmlElement(ElementName = "IsLeaf", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string IsLeaf { get; set; }
            [XmlElement(ElementName = "IsShared", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string IsShared { get; set; }
            [XmlElement(ElementName = "Xml", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Xml { get; set; }
        }

        [XmlRoot(ElementName = "NewUpdates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class NewUpdates
        {
            [XmlElement(ElementName = "UpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public UpdateInfo[] UpdateInfo { get; set; }
        }

        [XmlRoot(ElementName = "ExtendedUpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class ExtendedUpdateInfo
        {
            [XmlElement(ElementName = "Updates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Updates Updates { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdatesResult", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class SyncUpdatesResult
        {
            [XmlElement(ElementName = "NewUpdates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public NewUpdates NewUpdates { get; set; }
            [XmlElement(ElementName = "OutOfScopeRevisionIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.OutOfScopeRevisionIDs OutOfScopeRevisionIDs { get; set; }
            [XmlElement(ElementName = "Truncated", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Truncated { get; set; }
            [XmlElement(ElementName = "NewCookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Cookie NewCookie { get; set; }
            [XmlElement(ElementName = "DriverSyncNotNeeded", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string DriverSyncNotNeeded { get; set; }
            [XmlElement(ElementName = "ExtendedUpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public ExtendedUpdateInfo ExtendedUpdateInfo { get; set; }
        }

        [XmlRoot(ElementName = "SyncUpdatesResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class SyncUpdatesResponse
        {
            [XmlElement(ElementName = "SyncUpdatesResult", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public SyncUpdatesResult SyncUpdatesResult { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CGetExtendedUpdateInfo2Request
    {
        [XmlRoot(ElementName = "UpdateIdentity", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class UpdateIdentity
        {
            [XmlElement(ElementName = "UpdateID", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string UpdateID { get; set; }
            [XmlElement(ElementName = "RevisionNumber", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string RevisionNumber { get; set; }
        }

        [XmlRoot(ElementName = "updateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class UpdateIDs
        {
            [XmlElement(ElementName = "UpdateIdentity", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public UpdateIdentity UpdateIdentity { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfo2
        {
            [XmlElement(ElementName = "updateIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public UpdateIDs UpdateIDs { get; set; }
            [XmlElement(ElementName = "infoTypes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.InfoTypes InfoTypes { get; set; }
            [XmlElement(ElementName = "deviceAttributes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string DeviceAttributes { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CGetExtendedUpdateInfo2Response
    {
        [XmlRoot(ElementName = "FileLocation", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class FileLocation
        {
            [XmlElement(ElementName = "FileDigest", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string FileDigest { get; set; }
            [XmlElement(ElementName = "Url", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string Url { get; set; }
            [XmlElement(ElementName = "PiecesHashUrl", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string PiecesHashUrl { get; set; }
            [XmlElement(ElementName = "BlockMapUrl", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string BlockMapUrl { get; set; }
            [XmlElement(ElementName = "EsrpDecryptionInformation", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string EsrpDecryptionInformation { get; set; }
        }

        [XmlRoot(ElementName = "FileLocations", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class FileLocations
        {
            [XmlElement(ElementName = "FileLocation", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public FileLocation[] FileLocation { get; set; }
        }

        [XmlRoot(ElementName = "FileDecryptionData", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class FileDecryptionData
        {
            [XmlElement(ElementName = "FileDecryption", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public FileDecryption FileDecryption { get; set; }
        }

        [XmlRoot(ElementName = "FileDecryption", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class FileDecryption
        {
            [XmlElement(ElementName = "FileDigest", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string FileDigest { get; set; }
            [XmlElement(ElementName = "DecryptionKey", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string DecryptionKey { get; set; }
            [XmlElement(ElementName = "SecurityData", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public SecurityData SecurityData { get; set; }
        }

        [XmlRoot(ElementName = "SecurityData", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class SecurityData
        {
            [XmlElement(ElementName = "base64Binary", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string base64Binary { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2Result", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfo2Result
        {
            [XmlElement(ElementName = "FileLocations", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public FileLocations FileLocations { get; set; }
            [XmlElement(ElementName = "FileDecryptionData", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public FileDecryptionData FileDecryptionData { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo2Response", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfo2Response
        {
            [XmlElement(ElementName = "GetExtendedUpdateInfo2Result", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public GetExtendedUpdateInfo2Result GetExtendedUpdateInfo2Result { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CGetExtendedUpdateInfoRequest
    {
        [XmlRoot(ElementName = "revisionIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class RevisionIDs
        {
            [XmlElement(ElementName = "int", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string[] Int { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfo", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfo
        {
            [XmlElement(ElementName = "cookie", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Cookie Cookie { get; set; }
            [XmlElement(ElementName = "revisionIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public RevisionIDs RevisionIDs { get; set; }
            [XmlElement(ElementName = "infoTypes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.InfoTypes InfoTypes { get; set; }
            [XmlElement(ElementName = "locales", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Locales Locales { get; set; }
            [XmlElement(ElementName = "deviceAttributes", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public string DeviceAttributes { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }

    public class CGetExtendedUpdateInfoResponse
    {
        [XmlRoot(ElementName = "GetExtendedUpdateInfoResult", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfoResult
        {
            [XmlElement(ElementName = "OutOfScopeRevisionIDs", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.OutOfScopeRevisionIDs OutOfScopeRevisionIDs { get; set; }
            [XmlElement(ElementName = "Updates", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public CSOAPCommon.Updates Updates { get; set; }
        }

        [XmlRoot(ElementName = "GetExtendedUpdateInfoResponse", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
        public class GetExtendedUpdateInfoResponse
        {
            [XmlElement(ElementName = "GetExtendedUpdateInfoResult", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
            public GetExtendedUpdateInfoResult GetExtendedUpdateInfoResult { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
        [XmlRoot(ElementName = "Verification", Namespace = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService")]
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

    public class CExtendedUpdateInfoXml
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
    #endregion

    #region JSON Objects
    public class CAppxMetadataJSON
    {
        public partial class AppxMetadata
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
    #endregion

    public enum MachineType : ushort
    {
        unknown = 0x0,
        axp = 0x184,
        am33 = 0x1d3,
        amd64 = 0x8664,
        arm = 0x1c0,
        arm64 = 0xaa64,
        woa = 0x1c4,
        ebc = 0xebc,
        x86 = 0x14c,
        ia64 = 0x200,
        m32r = 0x9041,
        mips16 = 0x266,
        mipsfpu = 0x366,
        mipsfpu16 = 0x466,
        powerpc = 0x1f0,
        powerpcfp = 0x1f1,
        r4000 = 0x166,
        sh3 = 0x1a2,
        sh3dsp = 0x1a3,
        sh4 = 0x1a6,
        sh5 = 0x1a8,
        thumb = 0x1c2,
        wcemipsv2 = 0x169,
    }

    public enum OSSkuId
    {
        Undefined = 0x00000000,
        Ultimate = 0x00000001,
        HomeBasic = 0x00000002,
        HomePremium = 0x00000003,
        Enterprise = 0x00000004,
        HomeBasicN = 0x00000005,
        Business = 0x00000006,
        StandardServer = 0x00000007,
        DatacenterServer = 0x00000008,
        SmallBusinessServer = 0x00000009,
        EnterpriseServer = 0x0000000A,
        Starter = 0x0000000B,
        DatacenterServerCore = 0x0000000C,
        StandardServerCore = 0x0000000D,
        EnterpriseServerCore = 0x0000000E,
        EnterpriseServerIA64 = 0x0000000F,
        BusinessN = 0x00000010,
        WebServer = 0x00000011,
        ClusterServer = 0x00000012,
        HomeServer = 0x00000013,
        StorageExpressServer = 0x00000014,
        StorageStandardServer = 0x00000015,
        StorageWorkgroupServer = 0x00000016,
        StorageEnterpriseServer = 0x00000017,
        ServerForSmallBusiness = 0x00000018,
        SmallBusinessServerPremium = 0x00000019,
        HomePremiumN = 0x0000001A,
        EnterpriseN = 0x0000001B,
        UltimateN = 0x0000001C,
        WebServerCore = 0x0000001D,
        MediumBusinessServerManagement = 0x0000001E,
        MediumBusinessServerSecurity = 0x0000001F,
        MediumBusinessServerMessaging = 0x00000020,
        ServerFoundation = 0x00000021,
        HomePremiumServer = 0x00000022,
        ServerForSmallBusinessV = 0x00000023,
        StandardServerV = 0x00000024,
        DatacenterServerV = 0x00000025,
        EnterpriseServerV = 0x00000026,
        DatacenterServerCoreV = 0x00000027,
        StandardServerCoreV = 0x00000028,
        EnterpriseServerCoreV = 0x00000029,
        HyperV = 0x0000002A,
        StorageExpressServerCore = 0x0000002B,
        StorageServerStandardCore = 0x0000002C,
        StorageWorkgroupServerCore = 0x0000002D,
        StorageEnterpriseServerCore = 0x0000002E,
        StarterN = 0x0000002F,
        Professional = 0x00000030,
        ProfessionalN = 0x00000031,
        SBSolutionServer = 0x00000032,
        ServerForSBSolutions = 0x00000033,
        StandardServerSolutions = 0x00000034,
        StandardServerSolutionsCore = 0x00000035,
        SBSolutionServerEM = 0x00000036,
        ServerForSBSolutionsEM = 0x00000037,
        SolutionEmbeddedServer = 0x00000038,
        SolutionEmbeddedServerCore = 0x00000039,
        ProfessionalEmbedded = 0x0000003A,
        EssentialBusinessServerMGMT = 0x0000003B,
        EssentialBusinessServerADDL = 0x0000003C,
        EssentialBusinessServerMGMTSVC = 0x0000003D,
        EssentialBusinessServerADDLSVC = 0x0000003E,
        SmallBusinessServerPremiumCore = 0x0000003F,
        ClusterServerV = 0x00000040,
        Embedded = 0x00000041,
        StarterE = 0x00000042,
        HomeBasicE = 0x00000043,
        HomePremiumE = 0x00000044,
        ProfessionalE = 0x00000045,
        EnterpriseE = 0x00000046,
        UltimateE = 0x00000047,
        EnterpriseEvaluation = 0x00000048,
        Unknown49,
        Prerelease = 0x0000004A,
        Unknown4B,
        MultipointStandardServer = 0x0000004C,
        MultipointPremiumServer = 0x0000004D,
        Unknown4E = 0x0000004E,
        StandardEvaluationServer = 0x0000004F,
        DatacenterEvaluationServer = 0x00000050,
        PrereleaseARM = 0x00000051,
        PrereleaseN = 0x00000052,
        Unknown53,
        EnterpriseNEvaluation = 0x00000054,
        EmbeddedAutomotive = 0x00000055,
        EmbeddedIndustryA = 0x00000056,
        ThinPC = 0x00000057,
        EmbeddedA = 0x00000058,
        EmbeddedIndustry = 0x00000059,
        EmbeddedE = 0x0000005A,
        EmbeddedIndustryE = 0x0000005B,
        EmbeddedIndustryAE = 0x0000005C,
        Unknown5D,
        Unknown5E,
        StorageWorkgroupEvaluationServer = 0x0000005F,
        StorageStandardEvaluationServer = 0x00000060,
        CoreARM = 0x00000061,
        CoreN = 0x00000062,
        CoreCountrySpecific = 0x00000063,
        CoreSingleLanguage = 0x00000064,
        Core = 0x00000065,
        Unknown66,
        ProfessionalWMC = 0x00000067,
        Unknown68,
        EmbeddedIndustryEval = 0x00000069,
        EmbeddedIndustryEEval = 0x0000006A,
        EmbeddedEval = 0x0000006B,
        EmbeddedEEval = 0x0000006C,
        NanoServer = 0x0000006D,
        CloudStorageServer = 0x0000006E,
        CoreConnected = 0x0000006F,
        ProfessionalStudent = 0x00000070,
        CoreConnectedN = 0x00000071,
        ProfessionalStudentN = 0x00000072,
        CoreConnectedSingleLanguage = 0x00000073,
        CoreConnectedCountrySpecific = 0x00000074,
        ConnectedCAR = 0x00000075,
        IndustryHandheld = 0x00000076,
        PPIPro = 0x00000077,
        ARM64Server = 0x00000078,
        Education = 0x00000079,
        EducationN = 0x0000007A,
        IoTUAP = 0x0000007B,
        CloudHostInfrastructureServer = 0x0000007C,
        EnterpriseS = 0x0000007D,
        EnterpriseSN = 0x0000007E,
        ProfessionalS = 0x0000007F,
        ProfessionalSN = 0x00000080,
        EnterpriseSEvaluation = 0x00000081,
        EnterpriseSNEvaluation = 0x00000082,
        Unknown83,
        Unknown84,
        Unknown85,
        Unknown86,
        Holographic = 0x00000087,
        HolographicBusiness = 0x00000088,
        Unknown89 = 0x00000089,
        ProSingleLanguage = 0x0000008A,
        ProChina = 0x0000008B,
        EnterpriseSubscription = 0x0000008C,
        EnterpriseSubscriptionN = 0x0000008D,
        Unknown8E,
        DatacenterNanoServer = 0x0000008F,
        StandardNanoServer = 0x00000090,
        DatacenterAServerCore = 0x00000091,
        StandardAServerCore = 0x00000092,
        DatacenterWSServerCore = 0x00000093,
        StandardWSServerCore = 0x00000094,
        UtilityVM = 0x00000095,
        Unknown96,
        Unknown97,
        Unknown98,
        Unknown99,
        Unknown9A,
        Unknown9B,
        Unknown9C,
        Unknown9D,
        Unknown9E,
        DatacenterEvaluationServerCore = 0x0000009F,
        StandardEvaluationServerCore = 0x000000A0,
        ProWorkstation = 0x000000A1,
        ProWorkstationN = 0x000000A2,
        UnknownA3,
        ProForEducation = 0x000000A4,
        ProForEducationN = 0x000000A5,
        UnknownA6,
        UnknownA7,
        AzureServerCore = 0x000000A8,
        AzureNanoServer = 0x000000A9,
        UnknownAA = 0x000000AA,
        EnterpriseG = 0x000000AB,
        EnterpriseGN = 0x000000AC,
        UnknownAD,
        UnknownAE,
        ServerRDSH = 0x000000AF,
        UnknownB0,
        UnknownB1,
        Cloud = 0x000000B2,
        CloudN = 0x000000B3,
        HubOS = 0x000000B4,
        UnknownB5,
        OneCoreUpdateOS = 0x000000B6,
        CloudE = 0x000000B7,
        Andromeda = 0x000000B8,
        IoTOS = 0x000000B9,
        CloudEN = 0x000000BA,
        IoTEdgeOS = 0x000000BB,
        IoTEnterprise = 0x000000BC,
        Lite = 0x000000BD,
        UnknownBE,
        IoTEnterpriseS = 0x000000BF
    }

    public class CTAC
    {
        public string DeviceAttributes { get; set; }
        public string CallerAttributes { get; set; }
        public string Products { get; set; }
        public bool SyncCurrentVersionOnly { get; set; }
    }

    public class FE3Handler
    {
        private static string UserAgent = "Windows-Update-Agent/10.0.10011.16384 Client-Protocol/2.41";
        private static string Endpoint = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx";
        private static string Action = "http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService/";
        private static string MSCV = "0";

        public static CTAC BuildCTAC(string DeviceFamily, OSSkuId ReportingSku, string ReportingVersion, MachineType MachineType, string Ring, string BranchReadinessLevel, string branch, bool SyncCurrentVersionOnly, bool IsStore)
        {
            CTAC ctac = new CTAC();

            string content = Ring == "RP" ? "Current" : "Active";
            int flightEnabled = Ring == "Retail" ? 0 : 1;
            string App = IsStore ? "WU_STORE" : "WU_OS";

            ctac.DeviceAttributes = $"E:BranchReadinessLevel={BranchReadinessLevel}&CurrentBranch={branch}&OEMModel=VM&FlightRing={Ring}&AttrDataVer=92&InstallLanguage=en-US&OSUILocale=en-US&InstallationType=Client&FlightingBranchName=external&OSSkuId={(int)ReportingSku}&FlightContent={content}&App={App}&ProcessorManufacturer=GenuineIntel&OEMName_Uncleaned=VM&AppVer={ReportingVersion}&OSArchitecture={MachineType.ToString().ToUpper()}&IsFlightingEnabled={flightEnabled}&TelemetryLevel=3&DefaultUserRegion=244&WuClientVer={ReportingVersion}&OSVersion={ReportingVersion}&DeviceFamily={DeviceFamily}&IsRetailOS={flightEnabled + 1 % 1}";

            if (ReportingSku == OSSkuId.EnterpriseS || ReportingSku == OSSkuId.EnterpriseSN)
            {
                ctac.DeviceAttributes += "&BlockFeatureUpdates=1";
            }

            ctac.CallerAttributes = "E:Interactive=1&IsSeeker=1&SheddingAware=1&";
            if (IsStore)
            {
                ctac.CallerAttributes = "Acquisition=1&Id=Acquisition%3BMicrosoft.WindowsStore_8wekyb3d8bbwe&";
            }
            else
            {
                ctac.CallerAttributes = "Id=UpdateOrchestrator&";
            }
            ctac.Products = "";
            if (!IsStore)
            {
                ctac.Products = $"PN=Client.OS.rs2.{MachineType}&Branch={branch}&PrimaryOSProduct=1&Repairable=1&V={ReportingVersion};";
            }

            ctac.SyncCurrentVersionOnly = SyncCurrentVersionOnly;

            return ctac;
        }

        #region Data manipulation
        public static async Task<string> PostToWindowsUpdateAsync(string method, string message, bool secured)
        {
            string _endpoint = Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            IFlurlRequest request = _endpoint.WithHeader("MS-CV", MSCV)
                .WithHeader("SOAPAction", Action + method)
                .WithHeader("User-agent", UserAgent)
                .WithHeader("Method", "POST");

            StringContent content = new StringContent(message, System.Text.Encoding.UTF8, "application/soap+xml");
            IFlurlResponse response = await request.SendAsync(HttpMethod.Post, content);
            return await response.ResponseMessage.Content.ReadAsStringAsync();
        }

        private static CSOAPCommon.Envelope GetEnveloppe(string method, string authorizationToken, bool secured)
        {
            string _endpoint = Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            CSOAPCommon.Envelope envelope = new CSOAPCommon.Envelope()
            {
                Header = new CSOAPCommon.Header()
                {
                    Action = new CSOAPCommon.Action()
                    {
                        MustUnderstand = "1",
                        Text = Action + method
                    },
                    MessageID = $"urn:uuid:{Guid.NewGuid().ToString("D")}",
                    To = new CSOAPCommon.To()
                    {
                        MustUnderstand = "1",
                        Text = _endpoint
                    },
                    Security = new CSOAPCommon.Security()
                    {
                        MustUnderstand = "1",
                        Timestamp = new CSOAPCommon.Timestamp()
                        {
                            Created = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            Expires = "2044-08-02T20:09:03Z"
                        },
                        WindowsUpdateTicketsToken = new CSOAPCommon.WindowsUpdateTicketsToken()
                        {
                            Id = "ClientMSA"
                        }
                    }
                },
                Body = new CSOAPCommon.Body()
                {

                }
            };

            if (!string.IsNullOrEmpty(authorizationToken))
            {
                envelope.Header.Security.WindowsUpdateTicketsToken.TicketType = new CSOAPCommon.TicketType[]
                {
                    new CSOAPCommon.TicketType()
                    {
                        Name = "MSA",
                        Version = "1.0",
                        Policy = "MBI_SSL",
                        User = authorizationToken
                    },
                    new CSOAPCommon.TicketType()
                    {
                        Name = "AAD",
                        Version = "1.0",
                        Policy = "MBI_SSL"
                    }
                };
            }
            else
            {
                envelope.Header.Security.WindowsUpdateTicketsToken.Text = "";
            }

            return envelope;
        }

        private static string SerializeSOAPEnvelope(CSOAPCommon.Envelope envelope)
        {
            if (envelope == null) return string.Empty;

            var xmlSerializer = new XmlSerializer(typeof(CSOAPCommon.Envelope));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("s", "http://www.w3.org/2003/05/soap-envelope");
            ns.Add("a", "http://www.w3.org/2005/08/addressing");

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, envelope, ns);
                    return stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
                }
            }
        }

        private static CSOAPCommon.Envelope DeserializeSOAPEnvelope(string message)
        {
            if (message == null) return null;

            var xmlSerializer = new XmlSerializer(typeof(CSOAPCommon.Envelope));

            using (var stringReader = new StringReader(message))
            {
                return (CSOAPCommon.Envelope)xmlSerializer.Deserialize(stringReader);
            }
        }

        public static CExtendedUpdateInfoXml.Xml DeserializeInfoXML(string xml)
        {
            if (xml == null) return null;

            string message = "<Xml>" + xml + "</Xml>";

            var xmlSerializer = new XmlSerializer(typeof(CExtendedUpdateInfoXml.Xml));

            using (var stringReader = new StringReader(message))
            {
                return (CExtendedUpdateInfoXml.Xml)xmlSerializer.Deserialize(stringReader);
            }
        }

        public static CAppxMetadataJSON.AppxMetadata DeserializeAppxJSON(string json)
        {
            return JsonSerializer.Deserialize<CAppxMetadataJSON.AppxMetadata>(json);
        }
        #endregion

        #region WU functions
        public static async Task<CGetCookieResponse.GetCookieResponse> GetCookie()
        {
            CSOAPCommon.Envelope envelope = GetEnveloppe("GetCookie", null, false);

            envelope.Body.GetCookie = new CGetCookieRequest.GetCookie()
            {
                OldCookie = new CGetCookieRequest.OldCookie()
                {
                    Expiration = "2016-07-27T07:18:09Z"
                },
                LastChange = "2015-10-21T17:01:07.1472913Z",
                CurrentTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                ProtocolVersion = "2.41"
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return renvelope.Body.GetCookieResponse;
        }

        public static async Task<CGetExtendedUpdateInfoResponse.GetExtendedUpdateInfoResponse> GetExtendedUpdateInfo(
            CSOAPCommon.Cookie cookie,
            string token,
            string[] revisionId,
            CTAC ctac)
        {
            CSOAPCommon.Envelope envelope = GetEnveloppe("GetExtendedUpdateInfo", token, false);

            envelope.Body.GetExtendedUpdateInfo = new CGetExtendedUpdateInfoRequest.GetExtendedUpdateInfo()
            {
                Cookie = cookie,
                RevisionIDs = new CGetExtendedUpdateInfoRequest.RevisionIDs()
                {
                    Int = revisionId
                },
                InfoTypes = new CSOAPCommon.InfoTypes()
                {
                    XmlUpdateFragmentType = new string[]
                    {
                        "FileUrl",
                        "FileDecryption",
                        "Extended",
                        "LocalizedProperties",
                        "Eula",
                        "Published",
                        "Core",
                        "VerificationRule"
                    }
                },
                Locales = new CSOAPCommon.Locales()
                {
                    String = new string[] { "en-US", "en" }
                },
                DeviceAttributes = ctac.DeviceAttributes
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return renvelope.Body.GetExtendedUpdateInfoResponse;
        }


        public static async Task<CGetExtendedUpdateInfo2Response.GetExtendedUpdateInfo2Response> GetExtendedUpdateInfo2(
            string token,
            string UpdateID,
            string RevisionNumber,
            CTAC ctac
            )
        {
            CSOAPCommon.Envelope envelope = GetEnveloppe("GetExtendedUpdateInfo2", token, true);

            envelope.Body.GetExtendedUpdateInfo2 = new CGetExtendedUpdateInfo2Request.GetExtendedUpdateInfo2()
            {
                UpdateIDs = new CGetExtendedUpdateInfo2Request.UpdateIDs()
                {
                    UpdateIdentity = new CGetExtendedUpdateInfo2Request.UpdateIdentity()
                    {
                        UpdateID = UpdateID,
                        RevisionNumber = RevisionNumber
                    }
                },
                InfoTypes = new CSOAPCommon.InfoTypes()
                {
                    XmlUpdateFragmentType = new string[]
                    {
                        "FileUrl",
                        "FileDecryption",
                        "EsrpDecryptionInformation",
                        "PiecesHashUrl",
                        "BlockMapUrl"
                    }
                },
                DeviceAttributes = ctac.DeviceAttributes
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetExtendedUpdateInfo2", message, true);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return renvelope.Body.GetExtendedUpdateInfo2Response;
        }

        public static async Task<CSyncUpdatesResponse.SyncUpdatesResponse> SyncUpdates(
            CSOAPCommon.Cookie cookie,
            string token,
            string[] InstalledNonLeafUpdateIDs,
            string[] OtherCachedUpdateIDs,
            string[] CategoryIdentifiers,
            CTAC ctac
            )
        {
            string[] _InstalledNonLeafUpdateIDs = new int[]
            {
                1,
                2,
                3,
                10,
                11,
                17,
                19,
                2359974,
                2359977,
                5143990,
                5169043,
                5169044,
                5169047,
                8788830,
                8806526,
                9125350,
                9154769,
                10809856,
                23110993,
                23110994,
                23110995,
                23110996,
                23110999,
                23111000,
                23111001,
                23111002,
                23111003,
                23111004,
                24513870,
                28880263,
                30077688,
                30486944,
                59830006,
                59830007,
                59830008,
                60484010,
                62450018,
                62450019,
                62450020,
                98959022,
                98959023,
                98959024,
                98959025,
                98959026,
                105939029,
                105995585,
                106017178,
                107825194,
                117765322,
                129905029,
                130040030,
                130040031,
                130040032,
                130040033,
                133399034,
                138372035,
                138372036,
                139536037,
                139536038,
                139536039,
                139536040,
                142045136,
                158941041,
                158941042,
                158941043,
                158941044,
                159776047,
                160733048,
                160733049,
                160733050,
                160733051,
                160733055,
                160733056,
                161870057,
                161870058,
                161870059
                /*0,
                2,
                117765322,
                106017178,
                105939029,
                142045136,
                142046142,
                105984757,
                105995585,
                107825194,
                140377312,
                140367832,
                138380194,
                138378071,
                140406050,
                140423713,
                140422973,
                140421668,
                138380128,
                10,
                11,
                133399034,
                2359977,
                31209591,
                31213588,
                31211625,
                105379574,
                31212713,
                105370746,
                31220302,
                31220279,
                105376634,
                31207585,
                31219420,
                90199702,
                105368563,
                31218518,
                90212090,
                5169044,
                31197811,
                31205557,
                105362613,
                105378752,
                31202713,
                31222086,
                30530962,
                105373615,
                105364552,
                31208451,
                105373503,
                8806526,
                31258008,
                31208440,
                31190936,
                105377546,
                105369591,
                31210536,
                31203522,
                105381626,
                31198836,
                105382587,
                59830009*/
                /*1,
                2,
                3,
                10,
                11,
                17,
                19,
                2359974,
                2359977,
                5143990,
                5169043,
                5169044,
                5169047,
                8788830,
                8806526,
                9125350,
                9154769,
                10809856,
                23110993,
                23110994,
                23110995,
                23110996,
                23110999,
                23111000,
                23111001,
                23111002,
                23111003,
                23111004,
                24513870,
                28880263,
                30077688,
                30486944,
                59830006,
                59830007,
                59830008,
                60484010,
                62450018,
                62450019,
                62450020,
                69801474,
                98959022,
                98959023,
                98959024,
                98959025,
                98959026,
                105939029,
                105995585,
                106017178,
                107825194,
                117765322,
                129905029,
                130040030,
                130040031,
                130040032,
                130040033,
                133399034,
                138372035,
                138372036,
                139536037,
                139536038,
                139536039,
                139536040,
                142045136,
                158941041,
                158941042,
                158941043,
                158941044,
                159776047,
                160733048,
                160733049,
                160733050,
                160733051,
                160733055,
                160733056,
                161870057,
                161870058,
                161870059,*/
            }.Select(x => x.ToString()).ToArray();

            if (InstalledNonLeafUpdateIDs != null)
            {
                var tmplist = _InstalledNonLeafUpdateIDs.ToList();
                tmplist.AddRange(InstalledNonLeafUpdateIDs);
                _InstalledNonLeafUpdateIDs = tmplist.ToArray();
            }

            CSOAPCommon.Envelope envelope = GetEnveloppe("SyncUpdates", token, false);

            envelope.Body.SyncUpdates = new CSyncUpdatesRequest.SyncUpdates()
            {
                Cookie = cookie,
                Parameters = new CSyncUpdatesRequest.Parameters()
                {
                    ExpressQuery = "false",
                    InstalledNonLeafUpdateIDs = new CSyncUpdatesRequest.InstalledNonLeafUpdateIDs()
                    {
                        Int = _InstalledNonLeafUpdateIDs
                    },
                    OtherCachedUpdateIDs = new CSyncUpdatesRequest.OtherCachedUpdateIDs()
                    {
                        Int = OtherCachedUpdateIDs != null ? OtherCachedUpdateIDs : new string[0]
                    },
                    SkipSoftwareSync = "false",
                    NeedTwoGroupOutOfScopeUpdates = "true",
                    FilterAppCategoryIds = CategoryIdentifiers != null && CategoryIdentifiers.Length != 0 ? new CSyncUpdatesRequest.FilterAppCategoryIds()
                    {
                        CategoryIdentifier = new CSyncUpdatesRequest.CategoryIdentifier()
                        {
                            Id = CategoryIdentifiers != null ? CategoryIdentifiers : new string[0]
                        }
                    } : null,
                    AlsoPerformRegularSync = "false",
                    ComputerSpec = "",
                    ExtendedUpdateInfoParameters = new CSyncUpdatesRequest.ExtendedUpdateInfoParameters()
                    {
                        XmlUpdateFragmentTypes = new CSyncUpdatesRequest.XmlUpdateFragmentTypes()
                        {
                            XmlUpdateFragmentType = new string[]
                            {
                                "Extended",
                                "LocalizedProperties",
                                "Eula",
                                "Published",
                                "Core"
                            }
                        },
                        Locales = new CSOAPCommon.Locales()
                        {
                            String = new string[] { "en-US", "en" }
                        }
                    },
                    ClientPreferredLanguages = "",
                    ProductsParameters = new CSyncUpdatesRequest.ProductsParameters()
                    {
                        SyncCurrentVersionOnly = ctac.SyncCurrentVersionOnly ? "true" : "false",
                        DeviceAttributes = ctac.DeviceAttributes,
                        CallerAttributes = ctac.CallerAttributes,
                        Products = ctac.Products
                    }
                }
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("SyncUpdates", message, false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return renvelope.Body.SyncUpdatesResponse;
        }
        #endregion

        #region Application specific functions
        public static async Task<UpdateData[]> GetUpdates(string categoryId, CTAC ctac, string token, string filter = "Application") // Or ProductRelease
        {
            var cookie = await GetCookie();

            List<string> InstalledNonLeafUpdateIDs = new List<string>();
            List<string> OtherCachedUpdateIDs = new List<string>();

            List<CSyncUpdatesResponse.SyncUpdatesResponse> responses = new List<CSyncUpdatesResponse.SyncUpdatesResponse>();

            //
            // Scan all updates
            // WU will not return all updates in one go
            // So we need to perform multiple scans and cache the ids
            //
            while (true)
            {
                var result = await SyncUpdates(cookie.GetCookieResult, token, InstalledNonLeafUpdateIDs.ToArray(), OtherCachedUpdateIDs.ToArray(), new string[] { categoryId }, ctac);

                // Refresh the cookie
                cookie.GetCookieResult.EncryptedData = result.SyncUpdatesResult.NewCookie.EncryptedData;
                cookie.GetCookieResult.Expiration = result.SyncUpdatesResult.NewCookie.Expiration;

                if (result.SyncUpdatesResult.ExtendedUpdateInfo == null || result.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update == null || result.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update.Count() == 0)
                {
                    break;
                }

                foreach (var update in result.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    InstalledNonLeafUpdateIDs.Add(update.ID);
                    OtherCachedUpdateIDs.Add(update.ID);
                }

                responses.Add(result);
            }

            List<UpdateData> updateDatas = new List<UpdateData>();

            foreach (var response in responses.ToArray())
            {
                foreach (var update in response.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    UpdateData data = new UpdateData() { Update = update };

                    foreach (var updateInfo in response.SyncUpdatesResult.NewUpdates.UpdateInfo)
                    {
                        if (ulong.Parse(update.ID) == ulong.Parse(updateInfo.ID))
                        {
                            data.UpdateInfo = updateInfo;
                            break;
                        }
                    }

                    CExtendedUpdateInfoXml.Xml updateXml = DeserializeInfoXML(data.Update.Xml + data.UpdateInfo.Xml);
                    data.Xml = updateXml;

                    if (data.Xml.ApplicabilityRules != null && data.Xml.ApplicabilityRules.Metadata != null &&
                        data.Xml.ApplicabilityRules.Metadata.AppxPackageMetadata != null && data.Xml.ApplicabilityRules.Metadata.AppxPackageMetadata.AppxMetadata != null)
                    {
                        data.AppxMetadata = DeserializeAppxJSON(data.Xml.ApplicabilityRules.Metadata.AppxPackageMetadata.AppxMetadata.ApplicabilityBlob);
                    }

                    if (updateDatas.Any(x => x.Update.ID == update.ID))
                    {
                        var existingDataIndex = updateDatas.IndexOf(updateDatas.First(x => x.Update.ID == update.ID));
                        if (data.Xml.LocalizedProperties == null)
                        {
                            var backup = updateDatas[existingDataIndex].Xml;
                            updateDatas[existingDataIndex].Xml = data.Xml;

                            updateDatas[existingDataIndex].Xml.LocalizedProperties = backup.LocalizedProperties;
                        }

                        if (updateDatas[existingDataIndex].Xml.LocalizedProperties == null)
                        {
                            updateDatas[existingDataIndex].Xml.LocalizedProperties = data.Xml.LocalizedProperties;
                        }

                        continue;
                    }

                    updateDatas.Add(data);
                }
            }

            List<UpdateData> relevantUpdateDatas = new List<UpdateData>();

            foreach (var updateData in updateDatas.ToArray())
            {
                if (updateData.Xml.ExtendedProperties != null)
                {
                    if (updateData.Xml.ExtendedProperties.ContentType == filter && updateData.Xml.Files != null)
                    {
                        updateData.CTAC = ctac;
                        relevantUpdateDatas.Add(updateData);
                    }
                }
            }

            return relevantUpdateDatas.ToArray();
        }
        #endregion

        #region File url specific functions
        public static async Task<string> GetFileUrl(UpdateData updateData, string fileDigest, string token, CTAC ctac)
        {
            var result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, ctac);

            if (result.GetExtendedUpdateInfo2Result.FileLocations != null)
            {
                foreach (var fileLocation in result.GetExtendedUpdateInfo2Result.FileLocations.FileLocation)
                {
                    if (fileLocation.FileDigest == fileDigest)
                    {
                        return fileLocation.Url;
                    }
                }
            }

            return "";
        }
        #endregion
    }

    public class UpdateData
    {
        public CSyncUpdatesResponse.UpdateInfo UpdateInfo;
        public CSOAPCommon.Update Update;
        public CExtendedUpdateInfoXml.Xml Xml;
        public CAppxMetadataJSON.AppxMetadata AppxMetadata;
        public CTAC CTAC;
        public string CachedMetadata;
    }
}