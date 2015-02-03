using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WordTutor.Helpers
{
    public class AdmAccessToken
    {
        public static AdmAccessToken _admAccessToken = new AdmAccessToken();

        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }

        public AdmAccessToken()
        {
            access_token = "";
            token_type = "";
            expires_in = "";
            scope = "";
        }

        public delegate void AccessTokenHandler();
        public event AccessTokenHandler AccessTokenAvailable;

        public void GetAccessToken()
        {
            //  Create the request for the OAuth service that will get us the access tokens.
            String strTranslatorAccessURI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            System.Net.WebRequest req = System.Net.WebRequest.Create(strTranslatorAccessURI);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            IAsyncResult writeRequestStreamCallback =
                (IAsyncResult)req.BeginGetRequestStream(new AsyncCallback(BingRequestStreamReady), req);
        }

        private void BingRequestStreamReady(IAsyncResult ar)
        {
            // The request stream is ready. Write the request into the POST stream
            string clientID = "MacTutor";
            string clientSecret = "8o5famKEXj1/RG0QV92gglvHjQZKHJjsdyw99g5EAIk=";
            String strRequestDetails = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", WebUtility.UrlEncode(clientID), WebUtility.UrlEncode(clientSecret));

            // note, this isn't a new request -- the original was passed to beginrequeststream, so we're pulling a reference to it back out. It's the same request
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)ar.AsyncState;
            // now that we have the working request, write the request details into it
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strRequestDetails);
            System.IO.Stream postStream = request.EndGetRequestStream(ar);
            postStream.Write(bytes, 0, bytes.Length);
            postStream.Dispose();
            // now that the request is good to go, let's post it to the server
            // and get the response. When done, the async callback will call the
            // GetResponseCallback function
            request.BeginGetResponse(new AsyncCallback(GetBingResponseCallback), request);
        }

        private void GetBingResponseCallback(IAsyncResult ar)
        {
            // Process the response callback to get the token
            // we'll then use that token to call the translator service
            // Pull the request out of the IAsynch result
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            // Using JSON we'll pull the response details out, and load it into an AdmAccess token class
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new
                System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AdmAccessToken));
                AdmAccessToken._admAccessToken = (AdmAccessToken)serializer.ReadObject(response.GetResponseStream());
                AccessTokenAvailable();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened when fetching the access token" + ex.Message);
            }
        }
    }
}