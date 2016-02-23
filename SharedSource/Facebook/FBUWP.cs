#if NETFX_CORE
using MarkerMetro.Unity.WinLegacy.Security.Cryptography;
using System.Threading.Tasks;
using System.Globalization;
#endif
using System;
using System.Collections.Generic;

using Facebook;
using MarkerMetro.Unity.WinIntegration;

#if NETFX_CORE
using Windows.Storage;
using MarkerMetro.Unity.WinIntegration.Storage;
using Windows.Security.Authentication.Web;
#endif

#if WINDOWS_UWP
using Facebook.Graph;
using Windows.Foundation.Collections;
#endif

namespace MarkerMetro.Unity.WinIntegration.Facebook
{
    public delegate object FBReturnObjectFromJsonDelegate(string jsonText);

    /// <summary>
    /// Unity Facebook implementation
    /// </summary>
    public static class FBUWP
    {
        private const string EXPIRY_DATE_BIN = "FBEXPB";

        // check whether facebook is initialized
        public static bool IsInitialized
        {
            get
            {
#if WINDOWS_UWP
                FBSession session = FBSession.ActiveSession;
                return session != null ? !string.IsNullOrEmpty(session.FBAppId) : false;
#else
                throw new PlatformNotSupportedException("");
#endif
            }
        }

        public static bool IsLoggedIn
        {
            get
            {
#if WINDOWS_UWP
                FBSession session = FBSession.ActiveSession;
                return session != null ? session.LoggedIn : false;
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }

        public static string UserId
        {
            get
            {
#if WINDOWS_UWP
                FBSession session = FBSession.ActiveSession;
                if (session != null && session.LoggedIn && session.User != null)
                {
                    return session.User.Id;
                }
                else
                {
                    return null;
                }
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }
        public static string UserName
        {
            get
            {
#if WINDOWS_UWP
                FBSession session = FBSession.ActiveSession;
                if (session != null && session.LoggedIn && session.User != null)
                {
                    return session.User.Name;
                }
                else
                {
                    return null;
                }
#else
                throw new PlatformNotSupportedException("CheckAndExtendTokenIfNeeded");
#endif
            }
        }

        /// <summary>
        /// FB.Init as per Unity SDK
        /// </summary>
        /// <remarks>
        /// https://developers.facebook.com/docs/unity/reference/current/FB.Init
        /// </remarks>
        public static void Init(InitDelegate onInitComplete, string fbAppId, HideUnityDelegate onHideUnity)
        {
#if WINDOWS_UWP
            FBSession session = FBSession.ActiveSession;
            session.FBAppId = fbAppId;
            session.WinAppId = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();

            if (onInitComplete != null)
            {
                if (Settings.HasKey(EXPIRY_DATE_BIN))
                {
                    long expDate = Settings.GetLong(EXPIRY_DATE_BIN);
                    DateTime expireDate = DateTime.FromBinary(expDate);

                    // verifies if the token has expired:
                    if (DateTime.Compare(DateTime.UtcNow, expireDate) > 0)
                    {
                        InvalidateData();
                        onInitComplete();
                    }
                    else
                    {
                        Login(null, (result) => { onInitComplete(); });
                    }
                }
                else
                {
                    onInitComplete();
                }
            }

            if (onHideUnity != null)
                throw new NotSupportedException("onHideUnity is not currently supported at this time.");
#else
            throw new PlatformNotSupportedException("");
#endif
        }

#if WINDOWS_UWP
        private static void InvalidateData()
        {
            Settings.DeleteKey(EXPIRY_DATE_BIN);
        }
#endif

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if WINDOWS_UWP
            // Get active session
            FBSession session = FBSession.ActiveSession;

            if (!IsInitialized)
            {
                if (callback != null)
                    callback(new FBResult() { Error = "Not initialized." });
                return;
            }

            Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    FBPermissions fbPermissions;
                    if (string.IsNullOrEmpty(permissions))
                    {
                        List<String> permissionList = new List<String>();
                        permissionList.Add("public_profile");
                        fbPermissions = new FBPermissions(permissionList);
                    }
                    else
                    {
                        var permissionsArray = permissions.Split(',');
                        fbPermissions = new FBPermissions(permissionsArray);
                    }

                    // Login to Facebook
                    global::Facebook.FBResult fbResult = await session.LoginAsync(fbPermissions);

                    var result = new FBResult(fbResult);

                    if (fbResult.Succeeded)
                    {
                        Settings.Set(EXPIRY_DATE_BIN, session.AccessTokenData.ExpirationDate.DateTime.ToUniversalTime().ToBinary());
                    }

                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => { callback(result); });
                    }
                });
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Logout()
        {
#if WINDOWS_UWP
            if (!IsLoggedIn)
                return;

            Task.Run(async () =>
            {
                FBSession session = FBSession.ActiveSession;
                string appID = session.FBAppId;

                await session.LogoutAsync();
                InvalidateData();

                session.FBAppId = appID;
                session.WinAppId = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void API(
            string endpoint,
            HttpMethod method,
            FacebookDelegate callback,
            object parameters = null,
            FBReturnObjectFromJsonDelegate fbReturnObjectDelegate = null)
        {
#if WINDOWS_UWP
            if (!IsLoggedIn)
            {
                if (callback != null)
                    callback(new FBResult() { Error = "Not logged in" });
                return;
            }

            // Get active session
            FBSession session = FBSession.ActiveSession;
            
            // If the user is logged in
            if (session.LoggedIn)
            {
                Task.Run(async () =>
                {
                    PropertySet param = new PropertySet();
                    if (parameters != null && parameters is ICollection<KeyValuePair<string, object>>)
                    {
                        foreach (KeyValuePair<string, object> pair in parameters as ICollection<KeyValuePair<string, object>>)
                        {
                            param.Add(pair);
                        }
                    }

                    FBSingleValue sval = new FBSingleValue(endpoint, param,
                                    new FBJsonClassFactory(fbReturnObjectDelegate));

                    global::Facebook.FBResult fbResult;
                    if (method == HttpMethod.GET)
                    {
                        fbResult = await sval.GetAsync();
                    }
                    else if (method == HttpMethod.POST)
                    {
                        fbResult = await sval.PostAsync();
                    }
                    else
                    {
                        fbResult = await sval.DeleteAsync();
                    }

                    var result = new FBResult(fbResult);

                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => { callback(result); });
                    }
                });
            }
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
#if WINDOWS_UWP
            if (!IsLoggedIn)
            {
                if (callback != null)
                    callback(new FBResult() { Error = "Not logged in" });
                return;
            }

            // Get active session
            FBSession session = FBSession.ActiveSession;

            if (session.LoggedIn)
            {
                Task.Run(async () =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        // Set parameters
                        PropertySet parameters = new PropertySet();
                        parameters.Add("message", message);
                        parameters.Add("to", to);
                        parameters.Add("data", data);
                        parameters.Add("title", title);

                        // Display feed dialog
                        global::Facebook.FBResult fbResult = await session.ShowRequestsDialogAsync(parameters);

                        var result = new FBResult(fbResult);

                        if (callback != null)
                        {
                            Dispatcher.InvokeOnAppThread(() => { callback(result); });
                        }
                    });
                });
            }

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
#if WINDOWS_UWP
            if (!IsLoggedIn)
            {
                if (callback != null)
                    callback(new FBResult() { Error = "Not logged in" });
                return;
            }

            // Get active session
            FBSession session = FBSession.ActiveSession;

            if (session.LoggedIn)
            {
                Task.Run(async () =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        // Set caption, link and description parameters
                        PropertySet parameters = new PropertySet();
                        parameters.Add("title", linkName);
                        parameters.Add("link", link);
                        parameters.Add("description", linkDescription);
                        parameters.Add("toId", toId);
                        parameters.Add("linkCaption", linkCaption);
                        parameters.Add("picture", picture);

                        // Display feed dialog
                        global::Facebook.FBResult fbResult = await session.ShowFeedDialogAsync(parameters);

                        var result = new FBResult(fbResult);

                        if (callback != null)
                        {
                            Dispatcher.InvokeOnAppThread(() => { callback(result); });
                        }
                    });
                });
            }

            // throw not supported exception when user passed in parameters not supported currently
            if (!string.IsNullOrWhiteSpace(mediaSource) || !string.IsNullOrWhiteSpace(actionName) || !string.IsNullOrWhiteSpace(actionLink) ||
                !string.IsNullOrWhiteSpace(reference) || properties != null)
                throw new NotSupportedException("mediaSource, actionName, actionLink, reference and properties are not currently supported at this time.");
#else
            throw new PlatformNotSupportedException("");
#endif
        }
    }
}