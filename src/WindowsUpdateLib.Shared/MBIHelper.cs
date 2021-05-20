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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateLib.Shared
{
    public static class MBIHelper
    {
        public async static Task<string> GenerateMicrosoftAccountTokenAsync(string email, string password)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes("t=" + await GetBearerTokenForScope(email, password, "service::dcat.update.microsoft.com::MBI_SSL").ConfigureAwait(false) + "&p="));
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
                HttpWebResponse hwresp = (HttpWebResponse)(await hwreq.GetResponseAsync().ConfigureAwait(false));

                foreach (string oCookie in hwresp.Headers["Set-Cookie"].Split(','))
                {
                    if (oCookie.Trim().StartsWith("MSPOK"))
                    {
                        MSPOK = oCookie.Trim()[6..oCookie.IndexOf(';')];
                        MSPOK = WebUtility.UrlEncode(MSPOK);
                        break;
                    }
                }

                string responsePlain = string.Empty;
                using (StreamReader reader = new(hwresp.GetResponseStream(), Encoding.UTF8))
                {
                    responsePlain = reader.ReadToEnd();
                }
                PPFT = responsePlain[responsePlain.IndexOf("name=\"PPFT\"")..];
                PPFT = PPFT[(PPFT.IndexOf("value=") + 7)..];
                PPFT = PPFT.Substring(0, PPFT.IndexOf('\"'));
                urlPost = responsePlain[(responsePlain.IndexOf("urlPost:") + 9)..];
                urlPost = urlPost.Substring(0, urlPost.IndexOf('\''));
            }
            catch { return string.Empty; }

            HttpClientHandler httpClientHandler = new()
            {
                AllowAutoRedirect = false
            };

            CookieContainer hwreqCC = new();
            hwreqCC.Add(new Uri("https://login.live.com"), new Cookie("MSPOK", MSPOK) { Domain = "login.live.com" });
            httpClientHandler.CookieContainer = hwreqCC;

            HttpClient client = new(httpClientHandler);

            StringContent queryString = new($"login={email}&passwd={password}&PPFT={PPFT}", Encoding.UTF8);

            queryString.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            byte[] POSTByteArray = Encoding.UTF8.GetBytes($"login={email}&passwd={password}&PPFT={PPFT}");
            queryString.Headers.ContentLength = POSTByteArray.Length;

            HttpResponseMessage hwresp2 = await client.PostAsync(new Uri(urlPost), queryString).ConfigureAwait(false);

            try
            {
                foreach (string oLocationBit in hwresp2.Headers.Location.AbsoluteUri.Split('&'))
                {
                    if (oLocationBit.Contains("access_token"))
                    {
                        retVal = oLocationBit[(oLocationBit.IndexOf("access_token") + 13)..];
                        if (retVal.Contains("&"))
                        {
                            retVal = retVal.Substring(0, retVal.IndexOf('&'));
                        }

                        break;
                    }
                }
            }
            catch { return string.Empty; }
            return WebUtility.UrlDecode(retVal);
        }
    }
}
