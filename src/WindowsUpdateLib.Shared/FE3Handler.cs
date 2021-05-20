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
using CompDB;
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
    public static class FE3Handler
    {
        private static readonly CorrelationVector correlationVector = new();
        private static string MSCV = correlationVector.GetValue();
        private static readonly HttpClient httpClient = new(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // Linux
        });

        #region Data manipulation

        private static async Task<string> PostToWindowsUpdateAsync(string method, string message, bool secured)
        {
            string _endpoint = Constants.Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            MSCV = correlationVector.Increment();

            StringContent content = new(message, System.Text.Encoding.UTF8, "application/soap+xml");

            HttpRequestMessage req = new(HttpMethod.Post, _endpoint)
            {
                Content = content
            };

            req.Headers.Add("MS-CV", MSCV);
            req.Headers.Add("SOAPAction", Constants.Action + method);
            req.Headers.Add("User-agent", Constants.UserAgent);
            req.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            req.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            req.Headers.Connection.Add("keep-alive");

            HttpResponseMessage response = (await httpClient.SendAsync(req).ConfigureAwait(false)).EnsureSuccessStatusCode();
            string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return resultString;
        }

        private static CSOAPCommon.Envelope GetEnveloppe(string method, string authorizationToken, bool secured)
        {
            string _endpoint = Constants.Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            CSOAPCommon.Envelope envelope = new()
            {
                Header = new CSOAPCommon.Header()
                {
                    Action = new CSOAPCommon.Action()
                    {
                        MustUnderstand = "1",
                        Text = Constants.Action + method
                    },
                    MessageID = $"urn:uuid:{Guid.NewGuid():D}",
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
            if (envelope == null)
            {
                return string.Empty;
            }

            XmlSerializer xmlSerializer = new(typeof(CSOAPCommon.Envelope));

            XmlSerializerNamespaces ns = new();
            ns.Add("s", "http://www.w3.org/2003/05/soap-envelope");
            ns.Add("a", "http://www.w3.org/2005/08/addressing");

            using StringWriter stringWriter = new();
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
            xmlSerializer.Serialize(xmlWriter, envelope, ns);
            return stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "").Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n", "");
        }

        private static CSOAPCommon.Envelope DeserializeSOAPEnvelope(string message)
        {
            if (message == null)
            {
                return null;
            }

            XmlSerializer xmlSerializer = new(typeof(CSOAPCommon.Envelope));

            using StringReader stringReader = new(message);
            return (CSOAPCommon.Envelope)xmlSerializer.Deserialize(stringReader);
        }

        private static CExtendedUpdateInfoXml.Xml DeserializeInfoXML(string xml)
        {
            if (xml == null)
            {
                return null;
            }

            string message = "<Xml>" + xml + "</Xml>";

            XmlSerializer xmlSerializer = new(typeof(CExtendedUpdateInfoXml.Xml));

            using StringReader stringReader = new(message);
            return (CExtendedUpdateInfoXml.Xml)xmlSerializer.Deserialize(stringReader);
        }

        private static CAppxMetadataJSON.AppxMetadataJson DeserializeAppxJSON(string json)
        {
            return JsonSerializer.Deserialize<CAppxMetadataJSON.AppxMetadataJson>(json);
        }

        #endregion Data manipulation

        #region WU functions

        private static async Task<(CGetCookieResponse.GetCookieResponse, string)> GetCookie()
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

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false).ConfigureAwait(false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetCookieResponse, response);
        }

        private static async Task<(CGetExtendedUpdateInfoResponse.GetExtendedUpdateInfoResponse, string)> GetExtendedUpdateInfo(
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

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false).ConfigureAwait(false);

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

            string response = await PostToWindowsUpdateAsync("GetExtendedUpdateInfo2", message, true).ConfigureAwait(false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetExtendedUpdateInfo2Response, response);
        }

        private static async Task<(CSyncUpdatesResponse.SyncUpdatesResponse, string)> SyncUpdates(
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
                List<string> tmplist = _InstalledNonLeafUpdateIDs.ToList();
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
                        Int = OtherCachedUpdateIDs != null ? OtherCachedUpdateIDs.ToArray() : Array.Empty<string>()
                    },
                    SkipSoftwareSync = "false",
                    NeedTwoGroupOutOfScopeUpdates = "true",
                    FilterAppCategoryIds = CategoryIdentifiers != null && CategoryIdentifiers.Length != 0 ? new CSyncUpdatesRequest.FilterAppCategoryIds()
                    {
                        CategoryIdentifier = new CSyncUpdatesRequest.CategoryIdentifier()
                        {
                            Id = CategoryIdentifiers ?? Array.Empty<string>()
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

            string response = await PostToWindowsUpdateAsync("SyncUpdates", message, false).ConfigureAwait(false);

            CSOAPCommon.Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.SyncUpdatesResponse, response);
        }

        #endregion WU functions

        #region Application specific functions

        public static async Task<IEnumerable<UpdateData>> GetUpdates(string categoryId, CTAC ctac, string token, FileExchangeV3UpdateFilter filter = FileExchangeV3UpdateFilter.Application) // Or ProductRelease
        {
            (CGetCookieResponse.GetCookieResponse cookie, string cookieresp) = await GetCookie().ConfigureAwait(false);

            HashSet<string> InstalledNonLeafUpdateIDs = new();
            HashSet<string> OtherCachedUpdateIDs = new();

            HashSet<(CSyncUpdatesResponse.SyncUpdatesResponse, string)> responses = new();

            //
            // Scan all updates
            // WU will not return all updates in one go
            // So we need to perform multiple scans and cache the ids
            //
            while (true)
            {
                (CSyncUpdatesResponse.SyncUpdatesResponse, string) result = await SyncUpdates(cookie.GetCookieResult, token, InstalledNonLeafUpdateIDs, OtherCachedUpdateIDs, string.IsNullOrEmpty(categoryId) ? Array.Empty<string>() : new string[] { categoryId }, ctac).ConfigureAwait(false);

                // Refresh the cookie
                cookie.GetCookieResult.EncryptedData = result.Item1.SyncUpdatesResult.NewCookie.EncryptedData;
                cookie.GetCookieResult.Expiration = result.Item1.SyncUpdatesResult.NewCookie.Expiration;

                if (result.Item1.SyncUpdatesResult.ExtendedUpdateInfo == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update.Length == 0)
                {
                    break;
                }

                foreach (CSOAPCommon.Update update in result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    InstalledNonLeafUpdateIDs.Add(update.ID);
                    OtherCachedUpdateIDs.Add(update.ID);
                }

                responses.Add(result);
            }

            HashSet<UpdateData> updateDatas = new();

            foreach ((CSyncUpdatesResponse.SyncUpdatesResponse, string) response in responses)
            {
                foreach (CSOAPCommon.Update update in response.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    UpdateData data = new() { Update = update };

                    foreach (CSyncUpdatesResponse.UpdateInfo updateInfo in response.Item1.SyncUpdatesResult.NewUpdates.UpdateInfo)
                    {
                        if (ulong.Parse(update.ID) == ulong.Parse(updateInfo.ID))
                        {
                            data.UpdateInfo = updateInfo;
                            break;
                        }
                    }

                    CExtendedUpdateInfoXml.Xml updateXml = DeserializeInfoXML(data.Update.Xml + data.UpdateInfo.Xml);
                    data.Xml = updateXml;

                    if (data.Xml.ApplicabilityRules?.Metadata?.AppxPackageMetadata?.AppxMetadata != null)
                    {
                        data.AppxMetadata = DeserializeAppxJSON(data.Xml.ApplicabilityRules.Metadata.AppxPackageMetadata.AppxMetadata.ApplicabilityBlob);
                    }

                    if (updateDatas.Any(x => x.Update.ID == update.ID))
                    {
                        UpdateData updateData = updateDatas.First(x => x.Update.ID == update.ID);
                        if (data.Xml.LocalizedProperties == null)
                        {
                            CExtendedUpdateInfoXml.Xml backup = updateData.Xml;
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

            HashSet<UpdateData> relevantUpdateDatas = new();

            foreach (UpdateData updateData in updateDatas)
            {
                if (updateData.Xml.ExtendedProperties != null)
                {
                    if (updateData.Xml.ExtendedProperties.ContentType == filter.ToString() && updateData.Xml.Files != null)
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

        public static async Task<FileExchangeV3FileDownloadInformation> GetFileUrl(UpdateData updateData, string fileDigest, string token = null)
        {
            (CGetExtendedUpdateInfo2Response.GetExtendedUpdateInfo2Response, string) result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, updateData.CTAC).ConfigureAwait(false);

            if (updateData.Xml?.Files?.File?.FirstOrDefault(x => x.AdditionalDigest?.Text == fileDigest) is CExtendedUpdateInfoXml.File file)
            {
                fileDigest = file.Digest;
            }

            if (result.Item1.GetExtendedUpdateInfo2Result.FileLocations != null)
            {
                foreach (CGetExtendedUpdateInfo2Response.FileLocation fileLocation in result.Item1.GetExtendedUpdateInfo2Result.FileLocations.FileLocation)
                {
                    if (fileLocation.FileDigest == fileDigest)
                    {
                        return new FileExchangeV3FileDownloadInformation(fileLocation);
                    }
                }
            }

            return null;
        }

        public static async Task<IEnumerable<FileExchangeV3FileDownloadInformation>> GetFileUrls(UpdateData updateData, string token = null)
        {
            (CGetExtendedUpdateInfo2Response.GetExtendedUpdateInfo2Response, string) result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, updateData.CTAC).ConfigureAwait(false);

            updateData.GEI2Response = result.Item2;

            return result.Item1.GetExtendedUpdateInfo2Result.FileLocations?.FileLocation.Select(x => new FileExchangeV3FileDownloadInformation(x));
        }

        #endregion File url specific functions
    }

    public enum FileExchangeV3UpdateFilter
    {
        ProductRelease,
        Application
    }

    public class FileExchangeV3FileDownloadInformation
    {
        public string DownloadUrl { get; }

        public bool IsEncrypted
        {
            get
            {
                return EsrpDecryptionInformation != null;
            }
        }

        public DateTime ExpirationDate
        {
            get
            {
                DateTime dateTime = DateTime.MaxValue;
                try
                {
                    long value = long.Parse(DownloadUrl.Split("P1=")[1].Split("&")[0]);
                    dateTime = DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().DateTime;
                }
                catch { }
                return dateTime;
            }
        }

        public bool IsDownloadable
        {
            get
            {
                return ExpirationDate > DateTime.Now;
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return IsDownloadable ? DateTime.Now - ExpirationDate : new TimeSpan(0);
            }
        }

        public EsrpDecryptionInformation EsrpDecryptionInformation { get; set; } = null;

        public string Digest { get; }

        internal FileExchangeV3FileDownloadInformation(CGetExtendedUpdateInfo2Response.FileLocation fileLocation)
        {
            DownloadUrl = fileLocation.Url;
            if (!string.IsNullOrEmpty(fileLocation.EsrpDecryptionInformation))
            {
                EsrpDecryptionInformation = EsrpDecryptionInformation.DeserializeFromJson(fileLocation.EsrpDecryptionInformation);
            }
            Digest = fileLocation.FileDigest;
        }

        public override bool Equals(object obj)
        {
            return obj is FileExchangeV3FileDownloadInformation info && info.Digest == Digest;
        }

        public override int GetHashCode()
        {
            return Digest.GetHashCode();
        }

        public async Task<bool> DecryptAsync(string InputFile, string OutputFile)
        {
            if (!IsEncrypted)
            {
                return false;
            }

            try
            {
                using EsrpDecryptor esrp = new(EsrpDecryptionInformation);
                await esrp.DecryptFileAsync(InputFile, OutputFile).ConfigureAwait(false);
                return true;
            }
            catch { }

            return false;
        }
    }

    public class UpdateData
    {
        public CSyncUpdatesResponse.UpdateInfo UpdateInfo;
        public CSOAPCommon.Update Update;
        public CExtendedUpdateInfoXml.Xml Xml;
        public CAppxMetadataJSON.AppxMetadataJson AppxMetadata;
        public CTAC CTAC;
        public string CachedMetadata;
        public string SyncUpdatesResponse;
        public string GEI2Response;
        public HashSet<CompDBXmlClass.CompDB> CompDBs;
    }
}