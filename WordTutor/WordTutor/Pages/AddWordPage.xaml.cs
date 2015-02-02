using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

using WordTutor.Helpers;
using WordTutor.Common;
using WordTutor.Models;

namespace WordTutor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddWordPage : Page
    {
        private NavigationHelper navigationHelper;
        private static string strTextToTranslate = "";
        AdmAccessToken bingToken = new AdmAccessToken();

        public AddWordPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }


        // The code for BING translator is taken from http://blogs.msdn.com/b/translation/p/windowsphone8.aspx

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            strTextToTranslate = textToTranslate.Text;
            if (bingToken != null && bingToken.access_token != "")
            {
                // We already have the access token. Proceed with the translation request
                sendTranslateViaBingRequest();
            }
            else
            {
                // TODO currently this is written for BING. Add switch case for other translators
                // STEP 1: Create the request for the OAuth service that will
                // get us our access tokens.
                String strTranslatorAccessURI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
                System.Net.WebRequest req = System.Net.WebRequest.Create(strTranslatorAccessURI);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                IAsyncResult writeRequestStreamCallback =
                  (IAsyncResult)req.BeginGetRequestStream(new AsyncCallback(BingRequestStreamReady), req);
            }
        }

        private void BingRequestStreamReady(IAsyncResult ar)
        {
            // STEP 2: The request stream is ready. Write the request into the POST stream
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
            // STEP 3: Process the response callback to get the token
            // we'll then use that token to call the translator service
            // Pull the request out of the IAsynch result
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            // Using JSON we'll pull the response details out, and load it into an AdmAccess token class
            try
            {
                System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new
                System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AdmAccessToken));
                bingToken = (AdmAccessToken)serializer.ReadObject(response.GetResponseStream());
                sendTranslateViaBingRequest();

             }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened " + ex.Message);
            }
        }

        private void sendTranslateViaBingRequest()
        {
            try 
            {
                string strLngFrom = ApplicationData.Current.LocalSettings.Values["tramslateFromLanguage"].ToString();
                string strLngTo = ApplicationData.Current.LocalSettings.Values["tramslateToLanguage"].ToString();
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + WebUtility.UrlEncode(strTextToTranslate) + "&from=" + strLngFrom + "&to=" + strLngTo;
                System.Net.WebRequest translationWebRequest = System.Net.HttpWebRequest.Create(uri);
                // The authorization header needs to be "Bearer" + " " + the access token
                string headerValue = "Bearer " + bingToken.access_token;
                translationWebRequest.Headers["Authorization"] = headerValue;
                // And now we call the service. When the translation is complete, we'll get the callback
                IAsyncResult writeRequestStreamCallback = (IAsyncResult)translationWebRequest.BeginGetResponse(new AsyncCallback(bingTranslationReady), translationWebRequest);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened " + ex.Message);
            }
        }

        private async void bingTranslationReady(IAsyncResult ar)
        {
            // STEP 4: Process the translation
            // Get the request details
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            // Get the response details
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            // Read the contents of the response into a string
            System.IO.Stream streamResponse = response.GetResponseStream();
            System.IO.StreamReader streamRead = new System.IO.StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();
            // Translator returns XML, so load it into an XDocument
            System.Xml.Linq.XDocument xTranslation =
              System.Xml.Linq.XDocument.Parse(responseString);
            string strTest = xTranslation.Root.FirstNode.ToString();
            // We're not on the UI thread, so use the dispatcher to update the UI
            //Deployment.Current.Dispatcher.BeginInvoke(() => translationResult.Text = strTest);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                translationResult.Text = strTest;
                App.ViewModel.addWord(new Word() { Text = strTest, Learning = true });
            });
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
