#if WINDOWS_PHONE || NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Security.Cryptography;
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
using Facebook.Client;
using Windows.Storage;
#endif
using System.Linq;
namespace MarkerMetro.Unity.WinIntegration.Facebook
{

    /// <summary>
    /// Unity Facebook implementation
    /// </summary>
    public static class FBNative
    {

#if WINDOWS_PHONE || NETFX_CORE
        private static Session _fbSessionClient;
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
#if WINDOWS_PHONE
            _onHideUnity = onHideUnity;
            _fbSessionClient = Session.ActiveSession;

            Task.Run(async () =>
            {
                // check and extend token if required
                await Session.CheckAndExtendTokenIfNeeded();
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
#if WINDOWS_PHONE || NETFX_CORE
            _fbSessionClient.Logout();
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if WINDOWS_PHONE || NETFX_CORE
            Session.OnFacebookAuthenticationFinished = (AccessTokenData data) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = String.IsNullOrEmpty(data.AccessToken) ? "Success" : "Fail", Error = "error" }); });
            };

            Dispatcher.InvokeOnUIThread(() =>
            {
                _fbSessionClient.LoginWithBehavior(permissions, FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void API(
            string endpoint,
            HttpMethod method,
            FacebookDelegate callback)
        {
#if WINDOWS_PHONE || NETFX_CORE
            
            if (method != HttpMethod.GET) throw new NotImplementedException();
            Task.Run(async () =>
            {
                FacebookClient fb = new FacebookClient(Session.ActiveSession.CurrentAccessTokenData.AccessToken);
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
#if WINDOWS_PHONE || NETFX_CORE
            API("me", HttpMethod.GET, (result) =>
            {
                var data = (IDictionary<string, object>)result.Json;
                var me = new GraphUser(data);

                if (callback != null)
                    callback(new FBUser(me));
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Show Request Dialog.
        /// filters, excludeIds and maxRecipients are not currently supported at this time.
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
#if WINDOWS_PHONE || NETFX_CORE
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            
            // TODO: (sanjeevd) Fix the case where the results come back from the browser
            //Session.OnFacebookAppRequestFinished = (result) =>
            //{
            //    if (callback != null)
            //        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            //};

            // pass in params to facebook client's app request
            Dispatcher.InvokeOnUIThread(() =>
            {
                // TODO: (sanjeevd) Fix the following with the appropriate callback
                Session.ShowAppRequestsDialog(null, message, title, to.ToList());
            });

            // throw not supported exception when user passed in parameters not supported currently
            if (!string.IsNullOrWhiteSpace(filters) || excludeIds != null || maxRecipients != null)
                throw new NotSupportedException("filters, excludeIds and maxRecipients are not currently supported at this time.");
#else
            throw new PlatformNotSupportedException("");
#endif

        }

        /// <summary>
        /// Show the Feed Dialog.
        /// mediaSource, actionName, actionLink, reference and properties are not currently supported at this time.
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
#if WINDOWS_PHONE || WINDOWS_PHONE_APP
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            // TODO: (sanjeevd) - Fix the case where results come back from browser
            //Session.OnFacebookFeedFinished = (result) =>
            //{
            //    if (callback != null)
            //        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            //};

            // pass in params to facebook client's app request
            Dispatcher.InvokeOnUIThread(() =>
            {
                Session.ShowFeedDialogViaBrowser(toId, link, linkName, linkCaption, linkDescription, picture);
            });

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
#if WINDOWS_PHONE || NETFX_CORE
                return Session.ActiveSession != null && !String.IsNullOrEmpty(Session.ActiveSession.CurrentAccessTokenData.AccessToken);
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }

#if WINDOWS_PHONE
        // return whether back button pressed event is consumed by facebook for closing the dialog
        public static bool BackButtonPressed()
        {
            if (Session.ActiveSession != null && Session.IsDialogOpen)
            {
                Session.CloseWebDialog();
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
#if WINDOWS_PHONE || NETFX_CORE
                return _fbSessionClient != null;
#else
                throw new PlatformNotSupportedException("");
#endif
            }
        }

        public static void MapUri (Uri uri)
        {
#if WINDOWS_PHONE
            Dispatcher.InvokeOnAppThread(() =>
            {
                (new FacebookUriMapper()).MapUri(uri);
            });
#endif
        }
    }
}