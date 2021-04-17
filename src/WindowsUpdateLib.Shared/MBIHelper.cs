using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateLib.Shared
{
    public class MBIHelper
    {
        public async static Task<string> GenerateMicrosoftAccountTokenAsync(string email, string password)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes("t=" + await GetBearerTokenForScope(email, password, $"service::dcat.update.microsoft.com::MBI_SSL") + "&p="));
        }

        private async static Task<string> GetBearerTokenForScope(string email, string password, string targetscope, string clientId = "ms-app://s-1-15-2-1929064262-2866240470-255121345-2806524548-501211612-2892859406-1685495620/")
        {
            string retVal = string.Empty;
            email = WebUtility.UrlEncode(email);
            password = WebUtility.UrlEncode(password);
            targetscope = WebUtility.UrlEncode(targetscope);
            clientId = WebUtility.UrlEncode(clientId);
            HttpWebRequest hwreq = (HttpWebRequest)WebRequest.Create($"https://login.live.com/oauth20_authorize.srf?client_id={clientId}&scope={targetscope}&response_type=token&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf");
            string MSPOK = string.Empty;
            string urlPost;
            string PPFT;
            try
            {
                HttpWebResponse hwresp = (HttpWebResponse)(await hwreq.GetResponseAsync());

                foreach (string oCookie in hwresp.Headers["Set-Cookie"].Split(','))
                {
                    if (oCookie.Trim().StartsWith("MSPOK"))
                    {
                        MSPOK = oCookie.Trim().Substring(6, oCookie.IndexOf(';') - 6);
                        MSPOK = WebUtility.UrlEncode(MSPOK);
                        break;
                    }
                }

                string responsePlain = string.Empty;
                using (var reader = new StreamReader(hwresp.GetResponseStream(), Encoding.UTF8))
                {
                    responsePlain = reader.ReadToEnd();
                }
                PPFT = responsePlain.Substring(responsePlain.IndexOf("name=\"PPFT\""));
                PPFT = PPFT.Substring(PPFT.IndexOf("value=") + 7);
                PPFT = PPFT.Substring(0, PPFT.IndexOf('\"'));
                urlPost = responsePlain.Substring(responsePlain.IndexOf("urlPost:") + 9);
                urlPost = urlPost.Substring(0, urlPost.IndexOf('\''));
            }
            catch { return string.Empty; }

            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            CookieContainer hwreqCC = new CookieContainer();
            hwreqCC.Add(new Uri("https://login.live.com"), new Cookie("MSPOK", MSPOK) { Domain = "login.live.com" });
            httpClientHandler.CookieContainer = hwreqCC;

            var client = new HttpClient(httpClientHandler);

            StringContent queryString = new StringContent($"login={email}&passwd={password}&PPFT={PPFT}", Encoding.UTF8);

            queryString.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            byte[] POSTByteArray = Encoding.UTF8.GetBytes($"login={email}&passwd={password}&PPFT={PPFT}");
            queryString.Headers.ContentLength = POSTByteArray.Length;

            var hwresp2 = await client.PostAsync(new Uri(urlPost), queryString);

            try
            {
                foreach (string oLocationBit in hwresp2.Headers.Location.AbsoluteUri.Split('&'))
                {
                    if (oLocationBit.Contains("access_token"))
                    {
                        retVal = oLocationBit.Substring(oLocationBit.IndexOf("access_token") + 13);
                        if (retVal.Contains("&"))
                            retVal = retVal.Substring(0, retVal.IndexOf('&'));
                        break;
                    }
                }
            }
            catch { return string.Empty; }
            return WebUtility.UrlDecode(retVal);
        }
    }
}
