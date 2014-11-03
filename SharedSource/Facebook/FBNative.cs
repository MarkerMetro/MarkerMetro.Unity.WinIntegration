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
                    FacebookSessionClient.ShowAppRequestsDialog();        
                });
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });


            // _fbSessionClient.WebDialog.Navigate.... < FReddy use this for now, we'll put the navigation itsef back into the FB Client later so it's more abstracted

            //public void ShowAppRequestsDialog()
            //{
            //    dialogWebBrowser.Navigate(new Uri(String.Format("https://m.facebook.com/v2.1/dialog/apprequests?access_token={0}&redirect_uri=fbconnect%3A%2F%2Fsuccess&app_id={1}&message=YOUR_MESSAGE_HERE&display=touch", FacebookSessionClient.CurrentSession.AccessToken, FacebookSessionClient.CurrentSession.AppId)));
            //}

            //public void ShowFeedDialog()
            //{
            //    dialogWebBrowser.Navigate(new Uri(String.Format("https://m.facebook.com/v2.1/dialog/feed?access_token={0}&redirect_uri=fbconnect%3A%2F%2Fsuccess&app_id={1}&display=touch", FacebookSessionClient.CurrentSession.AccessToken, FacebookSessionClient.CurrentSession.AppId)));
            //}


            //if (_web == null) throw new MissingPlatformException();
            //if (_web.IsActive || !IsLoggedIn)
            //{
            //    // Already in use
            //    if (callback != null)
            //        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Already in use / Not Logged In" }); });
            //    return;
            //}

            //if (_onHideUnity != null)
            //    Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });

            //Uri uri = new Uri("https://www.facebook.com/dialog/apprequests?app_id=" + AppId + 
            //    "&message=" + message + "&redirect_uri=" + _redirectUrl, UriKind.RelativeOrAbsolute);
            //_web.Navigate(uri, finishedCallback: (url, state) => 
            //{
            //    if ( url.ToString().StartsWith( _redirectUrl ) )
            //    {
            //        _web.Finish();
            //        if (_onHideUnity != null)
            //            Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
            //        if (callback != null)
            //            Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = "Finished AppRequest" }); });
            //    }
            //}, onError: LoginNavigationError, state: callback);
#else
            throw new PlatformNotSupportedException("");
#endif
        }

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