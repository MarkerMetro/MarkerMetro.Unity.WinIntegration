#if WINDOWS_PHONE || NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Cryptography;
using System.Threading.Tasks;
using System.Globalization;

#endif
using Facebook;
using System;
using System.Collections.Generic;

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
            _fbSessionClient.LoginWithBehavior(permissions, FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
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

        /// <summary>
        /// Get current logged in user info
        /// </summary>
        public static void GetCurrentUser(Action<FBUser> callback)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            API("me", HttpMethod.GET, (result) =>
            {
                var data = (IDictionary<string, object>)result.Json;
                var me = new GraphUser(data);

                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBUser(me)); });
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Show Request Dialog in browser
        /// </summary>
        public static void AppRequestViaBrowser(
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
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            FacebookSessionClient.OnFacebookAppRequestFinished = (result) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            };

            // pass in params to facebook client's app request
            FacebookSessionClient.AppRequestViaBrowser(message, to, data, title);
#else
            throw new PlatformNotSupportedException("");
#endif

        }

        /// <summary>
        /// Show the Feed Dialog in browser
        /// </summary>
        public static void FeedViaBrowser(
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null,
            FacebookDelegate callback = null)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            FacebookSessionClient.OnFacebookFeedFinished = (result) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            };

            // pass in params to facebook client's app request
            FacebookSessionClient.FeedViaBrowser(toId, link, linkName, linkCaption, linkDescription, picture);
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Show the Request Dialog
        /// </summary>
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
                if (_fbSessionClient.IsDialogOpen || !IsLoggedIn)
                {
                    //Already in use
                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Already in use / Not Logged In" }); });
                    return;
                }

                // tell unity to pause when the dialog is active
                if (_onHideUnity != null)
                    Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });

                // pass in params to facebook client's app request
                _fbSessionClient.AppRequest(message, to, data, title, (result) =>
                {
                    if (_onHideUnity != null)
                        Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });

                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
                });
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        // Show the Feed Dialog
        public static void Feed(
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null,
            FacebookDelegate callback = null)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
            {
                if (_fbSessionClient.IsDialogOpen || !IsLoggedIn)
                {
                    //Already in use
                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Already in use / Not Logged In" }); });
                    return;
                }

                // tell unity to pause when the dialog is active
                if (_onHideUnity != null)
                    Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });

                // pass in params to facebook client's app request
                _fbSessionClient.Feed(toId, link, linkName, linkCaption, linkDescription, picture, (result) =>
                {
                    if (_onHideUnity != null)
                        Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });

                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
                });
            });
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

#if WINDOWS_PHONE
        // return whether back button pressed event is consumed by facebook for closing the dialog
        public static bool BackButtonPressed()
        {
            if (_fbSessionClient != null && _fbSessionClient.IsDialogOpen)
            {
                _fbSessionClient.CloseWebDialog();
                return true;
            }
            else
                return false;
        }
#endif

        // check whether facebook is initialized
        public static bool IsInitialized
        {
            get
            {
#if WINDOWS_PHONE //|| NETFX_CORE
                return _fbSessionClient != null;
#else
                throw new PlatformNotSupportedException("");
#endif
            }
        }
    }
}