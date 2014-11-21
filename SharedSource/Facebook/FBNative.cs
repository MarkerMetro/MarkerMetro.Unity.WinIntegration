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

            Task.Run(async () =>
            {
                // check and extend token if required
                await FacebookSessionClient.CheckAndExtendTokenIfNeeded();
                if (onInitComplete != null)
                    Dispatcher.InvokeOnAppThread(() => { onInitComplete(); });
            });

            if (onHideUnity != null)
                throw new NotSupportedException("onHideUnity is not currently supported at this time.");
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

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if WINDOWS_PHONE //|| NETFX_CORE
            FacebookSessionClient.OnFacebookAuthenticationFinished = (success, session, error) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = success ? "Success" : "Fail", Error = error }); });
            };

            _fbSessionClient.LoginWithBehavior(permissions, FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
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

            // throw not supported exception when user passed in parameters not supported currently
            if (!string.IsNullOrWhiteSpace(filters) || excludeIds != null || maxRecipients != null)
                throw new NotSupportedException("filters, excludeIds and maxRecipients are not currently supported at this time.");
#else
            throw new PlatformNotSupportedException("");
#endif

        }

        /// <summary>
        /// Show the Feed Dialog in browser
        /// </summary>
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

            // throw not supported exception when user passed in parameters not supported currently
            if (!string.IsNullOrWhiteSpace(mediaSource) || !string.IsNullOrWhiteSpace(actionName) || !string.IsNullOrWhiteSpace(actionLink) ||
                !string.IsNullOrWhiteSpace(reference) || properties != null)
                throw new NotSupportedException("mediaSource, actionName, actionLink, reference and properties are not currently supported at this time.");
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