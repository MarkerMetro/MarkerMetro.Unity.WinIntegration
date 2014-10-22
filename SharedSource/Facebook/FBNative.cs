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
        /// <summary>
        /// FB.Init as per Unity SDK
        /// </summary>
        /// <remarks>
        /// https://developers.facebook.com/docs/unity/reference/current/FB.Init
        /// </remarks>
        public static void Init(InitDelegate onInitComplete, string appId, HideUnityDelegate onHideUnity)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("Invalid Facebook App ID");
            _fbSessionClient = new FacebookSessionClient(appId);
            _onHideUnity = onHideUnity;
            if (onInitComplete != null)
                Dispatcher.InvokeOnAppThread(() => { onInitComplete(); });
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
            FacebookClient fb = new FacebookClient(FacebookSessionClient.CurrentSession.AccessToken);
            if (method != HttpMethod.GET) throw new NotImplementedException();
            fb.GetAsync(endpoint, null, callback);
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
            FacebookSessionClient.ShowAppRequestsDialog();
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });

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

        public static void CheckAndExtendTokenIfNeeded(Action callback)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            var task = FacebookSessionClient.CheckAndExtendTokenIfNeeded();
            task.Wait();
            if (callback != null) callback();
#else
            throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
        }

    }
}