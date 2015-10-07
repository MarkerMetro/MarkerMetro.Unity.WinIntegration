#if NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Security.Cryptography;
using System.Threading.Tasks;
using System.Globalization;

#endif
using Facebook;
using System;
using System.Collections.Generic;

using MarkerMetro.Unity.WinIntegration;

#if NETFX_CORE
using Facebook.Client;
using Windows.Storage;
using MarkerMetro.Unity.WinIntegration.Storage;
#endif
using System.Linq;
namespace MarkerMetro.Unity.WinIntegration.Facebook
{

    /// <summary>
    /// Unity Facebook implementation
    /// </summary>
    public static class FBNative
    {

#if NETFX_CORE
        private const string FBID_KEY = "FBID";
        private const string FBNAME_KEY = "FBNAME";

        private static Session _fbSessionClient;
        private static HideUnityDelegate _onHideUnity;
        public static string AccessToken
        {
            get
            {
                if (_fbSessionClient != null && _fbSessionClient.CurrentAccessTokenData != null)
                {
                    return _fbSessionClient.CurrentAccessTokenData.AccessToken;
                }
                else
                {
                    return null;
                }
            }
        }
#else
        public static string AccessToken { get; private set; }
#endif
        public static string UserId { get; private set; }
        public static string UserName { get; private set; }

        /// <summary>
        /// FB.Init as per Unity SDK
        /// </summary>
        /// <remarks>
        /// https://developers.facebook.com/docs/unity/reference/current/FB.Init
        /// </remarks>
        public static void Init(InitDelegate onInitComplete, string appId, HideUnityDelegate onHideUnity)
        {
#if WINDOWS_PHONE_APP
            Dispatcher.InvokeOnUIThread(() =>
            {
                _onHideUnity = onHideUnity;
                _fbSessionClient = Session.ActiveSession;
                Session.AppId = appId;
                Task.Run(async () =>
                {
                    // check and extend token if required
                    await Session.CheckAndExtendTokenIfNeeded();

                    if (IsLoggedIn)
                    {
                        UserId = Settings.GetString(FBID_KEY);
                        UserName = Settings.GetString(FBNAME_KEY);
                    }

                    if (onInitComplete != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => { onInitComplete(); });
                    }
                });

                if (onHideUnity != null)
                    throw new NotSupportedException("onHideUnity is not currently supported at this time.");
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Logout()
        {
#if NETFX_CORE
            _fbSessionClient.Logout();
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if NETFX_CORE
            Session.OnFacebookAuthenticationFinished = (AccessTokenData data) =>
            {
                var result = new FBResult() { Text = (data == null || String.IsNullOrEmpty(data.AccessToken)) ? "Fail" : "Success", Error = (data == null || String.IsNullOrEmpty(data.AccessToken)) ? "Error" : null };
                if (data == null || String.IsNullOrEmpty(data.AccessToken))
                {
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => { callback(result); });
                    }
                }
                else
                {
                    GetCurrentUser((user) =>
                    {
                        UserId = user.Id;
                        UserName = user.Name;

                        Settings.Set(FBID_KEY, UserId);
                        Settings.Set(FBNAME_KEY, UserName);

                        if (callback != null)
                        {
                            Dispatcher.InvokeOnAppThread(() => { callback(result); });
                        }
                    });
                }
            };

            Dispatcher.InvokeOnUIThread(() =>
            {
                _fbSessionClient.LoginWithBehavior(permissions, FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// For platforms that do not support dynamic cast it to either IDictionary<string, object> if json object or IList<object> if array.
        /// For primitive types cast it to bool, string, dobule or long depending on the type.
        /// Reference: http://facebooksdk.net/docs/making-asynchronous-requests/#1
        /// </summary>
        public static void API(
            string endpoint,
            HttpMethod method,
            FacebookDelegate callback,
            object parameters = null)
        {
#if NETFX_CORE

            Task.Run(async () =>
            {
                FacebookClient fb = new FacebookClient(_fbSessionClient.CurrentAccessTokenData.AccessToken);
                FBResult fbResult = null;
                try
                {
                    object apiCall;
                    if (method == HttpMethod.GET)
                    {
                        apiCall = await fb.GetTaskAsync(endpoint, parameters);
                    }
                    else if (method == HttpMethod.POST)
                    {
                        apiCall = await fb.PostTaskAsync(endpoint, parameters);
                    }
                    else
                    {
                        apiCall = await fb.DeleteTaskAsync(endpoint);
                    }
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
#if NETFX_CORE
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
#if WINDOWS_PHONE_APP
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            Session.OnFacebookAppRequestFinished = (result) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            };

            // pass in params to facebook client's app request
            Dispatcher.InvokeOnUIThread(() =>
            {
                Session.ShowAppRequestDialogViaBrowser(message, title, to != null ? to.ToList<string>() : null, data);
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
#if WINDOWS_PHONE_APP
            if (!IsLoggedIn)
            {
                // not logged in
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not Logged In" }); });
                return;
            }

            Session.OnFacebookFeedFinished = (result) =>
            {
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = result.Text, Error = result.Error, Json = result.Json }); });
            };

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
#if NETFX_CORE
                return _fbSessionClient != null && !String.IsNullOrEmpty(_fbSessionClient.CurrentAccessTokenData.AccessToken);
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }

#if NETFX_CORE
        // return whether back button pressed event is consumed by facebook for closing the dialog
        public static bool BackButtonPressed()
        {
            if (_fbSessionClient != null && Session.IsDialogOpen)
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
#if NETFX_CORE
                return _fbSessionClient != null;
#else
                throw new PlatformNotSupportedException("");
#endif
            }
        }

        public static void MapUri (Uri uri)
        {
#if WINDOWS_PHONE_APP
            Dispatcher.InvokeOnAppThread(() =>
            {
                (new FacebookUriMapper()).MapUri(uri);
            });
#endif
        }
    }
}