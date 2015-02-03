using WordTutor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WordTutor.ViewModels;
using WordTutor.Helpers;
using System.Net;
using Windows.Storage;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace WordTutor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;
        private static WordListViewModel viewModel = null;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

         /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static WordListViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new WordListViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: Change this value to a cache size that is appropriate for your application.
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate.
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter.
                if (!rootFrame.Navigate(typeof(PivotPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active.
            Window.Current.Activate();

            // Fetch the languages supported by BING
            FetchSupportedLanguageCodes();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            await ViewModel.SaveData();
            deferral.Complete();
        }

        /// <summary>
        /// Is used to fetch the languages codes supported by BING
        /// </summary>
        private void FetchSupportedLanguageCodes()
        {
            // TODO currently this is written for BING. Add switch case for other translators
            if (AdmAccessToken._admAccessToken.access_token != "")
            {
                // We already have the access token. Proceed with the fetch request
                FetchBingSupportedLanguageCodes();
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
            FetchBingSupportedLanguageCodes();
        }

        private void FetchBingSupportedLanguageCodes()
        {
            try
            {
                string uriForCodes = "http://api.microsofttranslator.com/V2/Ajax.svc/GetLanguagesForTranslate";
                System.Net.WebRequest translationWebRequest = System.Net.HttpWebRequest.Create(uriForCodes);
                // The authorization header needs to be "Bearer" + " " + the access token
                string headerValue = "Bearer " + AdmAccessToken._admAccessToken.access_token;
                translationWebRequest.Headers["Authorization"] = headerValue;
                // And now we call the service. When the translation is complete, we'll get the callback
                IAsyncResult writeRequestStreamCallback = (IAsyncResult)translationWebRequest.BeginGetResponse(new AsyncCallback(BingSupportedLanguageCodesFetched), translationWebRequest);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened while fetching language codes" + ex.Message);
            }
        }

        private void BingSupportedLanguageCodesFetched(IAsyncResult ar)
        {
            // Fetched language codes
            // Get the request details
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            // Get the response details
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            // Read the contents of the response into a string
            System.IO.Stream streamResponse = response.GetResponseStream();
            System.IO.StreamReader streamRead = new System.IO.StreamReader(streamResponse);
            string langCodes = streamRead.ReadToEnd();
            ApplicationData.Current.LocalSettings.Values["supportedLanguageCodes"] = langCodes;

            // We have language codes. Fetch the language names as well
            try
            {
                string uriForNames = "http://api.microsofttranslator.com/V2/Ajax.svc/GetLanguageNames?locale=en&languageCodes=" + langCodes;
                System.Net.WebRequest translationWebRequest = System.Net.HttpWebRequest.Create(uriForNames);
                // The authorization header needs to be "Bearer" + " " + the access token
                string headerValue = "Bearer " + AdmAccessToken._admAccessToken.access_token;
                translationWebRequest.Headers["Authorization"] = headerValue;
                // And now we call the service. When the translation is complete, we'll get the callback
                IAsyncResult writeRequestStreamCallback = (IAsyncResult)translationWebRequest.BeginGetResponse(new AsyncCallback(BingSupportedLanguageNamesFetched), translationWebRequest);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Something bad happened while fetching language names" + ex.Message);
            }
        }

        private void BingSupportedLanguageNamesFetched(IAsyncResult ar)
        {
            // Fetched language names
            // Get the request details
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            // Get the response details
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            // Read the contents of the response into a string
            System.IO.Stream streamResponse = response.GetResponseStream();
            System.IO.StreamReader streamRead = new System.IO.StreamReader(streamResponse);
            ApplicationData.Current.LocalSettings.Values["supportedLanguageNames"] = streamRead.ReadToEnd();
        }
    }
}
