using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WindowsUpdateLib
{
    public class FE3Handler
    {
        private static CorrelationVector correlationVector = new CorrelationVector();
        private static string MSCV = correlationVector.GetValue();
        private static IFlurlClient flurlClient = new FlurlClient(new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));

        #region Data manipulation

        public static async Task<string> PostToWindowsUpdateAsync(string method, string message, bool secured)
        {
            string _endpoint = Constants.Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            IFlurlRequest request = _endpoint.WithHeader("MS-CV", MSCV)
                .WithHeader("SOAPAction", Constants.Action + method)
                .WithHeader("User-agent", Constants.UserAgent)
                .WithHeader("Method", "POST");

            MSCV = correlationVector.Increment();

            StringContent content = new StringContent(message, System.Text.Encoding.UTF8, "application/soap+xml");
            IFlurlResponse response = await request.WithClient(flurlClient).SendAsync(HttpMethod.Post, content);
            return await response.ResponseMessage.Content.ReadAsStringAsync();
        }

        private static CSOAPCommon.Envelope GetEnveloppe(string method, string authorizationToken, bool secured)
        {
            string _endpoint = Constants.Endpoint;
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
                        Text = Constants.Action + method
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
                            Expires = Constants.SecurityExpirationTimestamp
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

        #endregion Data manipulation

        #region WU functions

        public static async Task<(CGetCookieResponse.GetCookieResponse, string)> GetCookie()
        {
            CSOAPCommon.Envelope envelope = GetEnveloppe("GetCookie", null, false);

            envelope.Body.GetCookie = new CGetCookieRequest.GetCookie()
            {
                OldCookie = new CGetCookieRequest.OldCookie()
                {
                    Expiration = Constants.OldCookieExpiration
                },
                LastChange = Constants.LastChangeDate,
                CurrentTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                ProtocolVersion = Constants.ClientProtocolVersion
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetCookieResponse, response);
        }

        public static async Task<(CGetExtendedUpdateInfoResponse.GetExtendedUpdateInfoResponse, string)> GetExtendedUpdateInfo(
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

            return (renvelope.Body.GetExtendedUpdateInfoResponse, response);
        }

        public static async Task<(CGetExtendedUpdateInfo2Response.GetExtendedUpdateInfo2Response, string)> GetExtendedUpdateInfo2(
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

            return (renvelope.Body.GetExtendedUpdateInfo2Response, response);
        }

        public static async Task<(CSyncUpdatesResponse.SyncUpdatesResponse, string)> SyncUpdates(
            CSOAPCommon.Cookie cookie,
            string token,
            IEnumerable<string> InstalledNonLeafUpdateIDs,
            IEnumerable<string> OtherCachedUpdateIDs,
            string[] CategoryIdentifiers,
            CTAC ctac
            )
        {
            string[] _InstalledNonLeafUpdateIDs = Constants.InstalledNonLeafUpdateIDs.Select(x => x.ToString()).ToArray();

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
                        Int = OtherCachedUpdateIDs != null ? OtherCachedUpdateIDs.ToArray() : new string[0]
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

            return (renvelope.Body.SyncUpdatesResponse, response);
        }

        #endregion WU functions

        #region Application specific functions

        public static async Task<IEnumerable<UpdateData>> GetUpdates(string categoryId, CTAC ctac, string token, string filter = "Application") // Or ProductRelease
        {
            (CGetCookieResponse.GetCookieResponse cookie, string cookieresp) = await GetCookie();

            HashSet<string> InstalledNonLeafUpdateIDs = new HashSet<string>();
            HashSet<string> OtherCachedUpdateIDs = new HashSet<string>();

            HashSet<(CSyncUpdatesResponse.SyncUpdatesResponse, string)> responses = new HashSet<(CSyncUpdatesResponse.SyncUpdatesResponse, string)>();

            //
            // Scan all updates
            // WU will not return all updates in one go
            // So we need to perform multiple scans and cache the ids
            //
            while (true)
            {
                var result = await SyncUpdates(cookie.GetCookieResult, token, InstalledNonLeafUpdateIDs, OtherCachedUpdateIDs, new string[] { categoryId }, ctac);

                // Refresh the cookie
                cookie.GetCookieResult.EncryptedData = result.Item1.SyncUpdatesResult.NewCookie.EncryptedData;
                cookie.GetCookieResult.Expiration = result.Item1.SyncUpdatesResult.NewCookie.Expiration;

                if (result.Item1.SyncUpdatesResult.ExtendedUpdateInfo == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update.Count() == 0)
                {
                    break;
                }

                foreach (var update in result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    InstalledNonLeafUpdateIDs.Add(update.ID);
                    OtherCachedUpdateIDs.Add(update.ID);
                }

                responses.Add(result);
            }

            HashSet<UpdateData> updateDatas = new HashSet<UpdateData>();

            foreach (var response in responses)
            {
                foreach (var update in response.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    UpdateData data = new UpdateData() { Update = update };

                    foreach (var updateInfo in response.Item1.SyncUpdatesResult.NewUpdates.UpdateInfo)
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
                        var updateData = updateDatas.First(x => x.Update.ID == update.ID);
                        if (data.Xml.LocalizedProperties == null)
                        {
                            var backup = updateData.Xml;
                            updateData.Xml = data.Xml;

                            updateData.Xml.LocalizedProperties = backup.LocalizedProperties;
                        }

                        if (updateData.Xml.LocalizedProperties == null)
                        {
                            updateData.Xml.LocalizedProperties = data.Xml.LocalizedProperties;
                        }

                        continue;
                    }

                    data.SyncUpdatesResponse = response.Item2;

                    updateDatas.Add(data);
                }
            }

            HashSet<UpdateData> relevantUpdateDatas = new HashSet<UpdateData>();

            foreach (var updateData in updateDatas)
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

            return relevantUpdateDatas;
        }

        #endregion Application specific functions

        #region File url specific functions

        public static async Task<(string,string)> GetFileUrl(UpdateData updateData, string fileDigest, string token, CTAC ctac)
        {
            var result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, ctac);

            if (updateData.Xml?.Files?.File?.FirstOrDefault(x => x.AdditionalDigest?.Text == fileDigest) is CExtendedUpdateInfoXml.File file)
            {
                fileDigest = file.Digest;
            }

            if (result.Item1.GetExtendedUpdateInfo2Result.FileLocations != null)
            {
                foreach (var fileLocation in result.Item1.GetExtendedUpdateInfo2Result.FileLocations.FileLocation)
                {
                    if (fileLocation.FileDigest == fileDigest)
                    {
                        return (fileLocation.Url, fileLocation.EsrpDecryptionInformation);
                    }
                }
            }

            return ("","");
        }

        public static async Task<CGetExtendedUpdateInfo2Response.FileLocation[]> GetFileUrls(UpdateData updateData, string token, CTAC ctac)
        {
            var result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, ctac);

            updateData.GEI2Response = result.Item2;

            if (result.Item1.GetExtendedUpdateInfo2Result.FileLocations != null)
            {
                return result.Item1.GetExtendedUpdateInfo2Result.FileLocations.FileLocation;
            }

            return null;
        }

        #endregion File url specific functions
    }

    public class UpdateData
    {
        public CSyncUpdatesResponse.UpdateInfo UpdateInfo;
        public CSOAPCommon.Update Update;
        public CExtendedUpdateInfoXml.Xml Xml;
        public CAppxMetadataJSON.AppxMetadata AppxMetadata;
        public CTAC CTAC;
        public string CachedMetadata;
        public string SyncUpdatesResponse;
        public string GEI2Response;
    }
}