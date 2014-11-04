#if WINDOWS_PHONE || NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Cryptography;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;

#endif
using Facebook;
using System; 

using MarkerMetro.Unity.WinIntegration;

#if WINDOWS_PHONE
using Facebook.Client;
using System.IO.IsolatedStorage;
using Windows.Storage;
using System.Xml.Linq;
using System.IO;
using System.Linq;
#elif NETFX_CORE
using Windows.Storage;
#endif

namespace MarkerMetro.Unity.WinIntegration.Facebook
{

    /// <summary>
    /// Unity Facebook implementation
    /// </summary>
    public static class FBNative
    {

#if WINDOWS_PHONE //|| NETFX_CORE
        private static FacebookSessionClient _fbSessionClient;
        private static HideUnityDelegate _onHideUnity;
        private static string _redirectUrl = "fbconnect%3A%2F%2Fsuccess";

        public static FacebookSessionClient FBSessionClient { get { return _fbSessionClient; } }
#endif

        public static FBLoginCompleteDelegate OnFBLoginComplete;
        public delegate void FBLoginCompleteDelegate(bool success, string error);

        /// <summary>
        /// FB.Init as per Unity SDK
        /// </summary>
        /// <remarks>
        /// https://developers.facebook.com/docs/unity/reference/current/FB.Init
        /// </remarks>
        public static void Init(InitDelegate onInitComplete, string appId, HideUnityDelegate onHideUnity)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            _onHideUnity = onHideUnity;
            _fbSessionClient = new FacebookSessionClient(appId);

            FacebookSessionClient.OnFacebookAuthenticationFinished = (success, session, error) =>
            {
                Dispatcher.InvokeOnAppThread(() =>
                {
                    if (OnFBLoginComplete != null)
                    {
                        OnFBLoginComplete(success, error);
                    }
                });
            };

            Task.Run(async () =>
            {
                // check and extend token if required
                await FacebookSessionClient.CheckAndExtendTokenIfNeeded();
                if (onInitComplete != null)
                    Dispatcher.InvokeOnAppThread(() => { onInitComplete(); });
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Logout()
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            _fbSessionClient.Logout();
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Login(string permissions)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            _fbSessionClient.LoginWithBehavior("", FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
#else
            throw new PlatformNotSupportedException("");
#endif
        }
        public static void API(
            string endpoint,
            HttpMethod method,
            FacebookDelegate callback)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            
            if (method != HttpMethod.GET) throw new NotImplementedException();        
            Task.Run(async () =>
            {
                FacebookClient fb = new FacebookClient(FacebookSessionClient.CurrentSession.AccessToken);
                FBResult fbResult = null;
                try
                { 
                    var apiCall = await fb.GetTaskAsync(endpoint, null);
                    if (apiCall != null)
                    {
                        fbResult = new FBResult();
                        fbResult.Text = apiCall.ToString();
                        fbResult.Json = apiCall as JsonObject;
                    }
                }
                catch (Exception ex)
                {
                    fbResult = new FBResult();
                    fbResult.Error = ex.Message;
                }
                if (callback != null)
                {
                    Dispatcher.InvokeOnAppThread(() => { callback(fbResult); });
                }
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }


        public static void AppRequest(
                string message,
                string[] to = null,
                string filters = "",
                string[] excludeIds = null,
                int? maxRecipients = null,
                string data = "",
                string title = "",
                FacebookDelegate callback = null
            )
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
            {
                if (_fbSessionClient.WebDialog.IsActive || !IsLoggedIn)
                {
                    //Already in use
                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Already in use / Not Logged In" }); });
                    return;
                }

                if (_onHideUnity != null)
                    Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });

                FacebookSession currentSession = FacebookSessionClient.CurrentSession;

                Uri uri = new Uri(String.Format("https://m.facebook.com/v2.1/dialog/apprequests?access_token={0}&redirect_uri={1}&app_id={2}&message={3}&display=touch",
                    currentSession.AccessToken, _redirectUrl, currentSession.AppId, message));

                _fbSessionClient.WebDialog.Navigate(uri, true, finishedCallback: (url, state) =>
                {
                    if (url.ToString().StartsWith(_redirectUrl))
                    {
                        _fbSessionClient.WebDialog.Finish();
                        if (_onHideUnity != null)
                            Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
                        if (callback != null)
                            Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = "Finished AppRequest" }); });
                    }
                }, onError: NavigationError, state: callback);
            });
            

            //public void ShowAppRequestsDialog()
            //{
            //    dialogWebBrowser.Navigate(new Uri(String.Format("https://m.facebook.com/v2.1/dialog/apprequests?access_token={0}&redirect_uri=fbconnect%3A%2F%2Fsuccess&app_id={1}&message=YOUR_MESSAGE_HERE&display=touch", FacebookSessionClient.CurrentSession.AccessToken, FacebookSessionClient.CurrentSession.AppId)));
            //}

            //public void ShowFeedDialog()
            //{
            //    dialogWebBrowser.Navigate(new Uri(String.Format("https://m.facebook.com/v2.1/dialog/feed?access_token={0}&redirect_uri=fbconnect%3A%2F%2Fsuccess&app_id={1}&display=touch", FacebookSessionClient.CurrentSession.AccessToken, FacebookSessionClient.CurrentSession.AppId)));
            //}
            
#else
            throw new PlatformNotSupportedException("");
#endif
        }

#if WINDOWS_PHONE //|| NETFX_CORE
        private static void NavigationError(Uri url, int error, object state)
        {
            //Debug.LogError("Nav error: " + error);
            if (state is FacebookDelegate)
                Dispatcher.InvokeOnAppThread(() => { ((FacebookDelegate)state)(new FBResult() { Error = error.ToString(), Text = "AppRequest cancelled or ended with error." }); });
            _fbSessionClient.WebDialog.Finish();
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
        }
#endif


        // additional methods added for convenience

        public static bool IsLoggedIn
        {
            get
            {
#if WINDOWS_PHONE //|| NETFX_CORE
                return FacebookSessionClient.CurrentSession != null && !String.IsNullOrEmpty(FacebookSessionClient.CurrentSession.AccessToken);
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }

    }
}