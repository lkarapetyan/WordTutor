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
            if (AdmAccessToken._admAccessToken.access_token != "")
            {
                // We already have the access token. Proceed with the translation request
                SendTranslateViaBingRequest();
            }
            else
            {
                // Fetch the access token
                AdmAccessToken._admAccessToken.AccessTokenAvailable += new AdmAccessToken.AccessTokenHandler(OnAccessTokenAvailable);
                AdmAccessToken._admAccessToken.GetAccessToken();
            }
        }
        
        private void OnAccessTokenAvailable()
        {
            if (strTextToTranslate != "")
            {
                SendTranslateViaBingRequest();
            }
        }

        private void SendTranslateViaBingRequest()
        {
            try 
            {
                string strLngFrom = ApplicationData.Current.LocalSettings.Values["tramslateFromLanguage"].ToString();
                string strLngTo = ApplicationData.Current.LocalSettings.Values["tramslateToLanguage"].ToString();
                string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + WebUtility.UrlEncode(strTextToTranslate) + "&from=" + strLngFrom + "&to=" + strLngTo;
                System.Net.WebRequest translationWebRequest = System.Net.HttpWebRequest.Create(uri);
                // The authorization header needs to be "Bearer" + " " + the access token
                string headerValue = "Bearer " + AdmAccessToken._admAccessToken.access_token;
                translationWebRequest.Headers["Authorization"] = headerValue;
                // And now we call the service. When the translation is complete, we'll get the callback
                IAsyncResult writeRequestStreamCallback = (IAsyncResult)translationWebRequest.BeginGetResponse(new AsyncCallback(BingTranslationReady), translationWebRequest);
                strTextToTranslate = "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened " + ex.Message);
            }
        }

        private async void BingTranslationReady(IAsyncResult ar)
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
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                translationResult.Text = strTest;
                WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(request.RequestUri.Query);
                Word w = new Word();
                w.Text = decoder.GetFirstValueByName("text");
                w.Learning = true;
                Description d = new Description() { Text = strTest };
                w.addDescription(d);
                App.ViewModel.addWord(w);
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
