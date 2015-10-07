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
using Windows.Storage;
using MarkerMetro.Unity.WinIntegration.Storage;
#endif

namespace MarkerMetro.Unity.WinIntegration.Facebook
{
    public delegate void FacebookDelegate(FBResult result);

    public delegate void InitDelegate();

    public delegate void HideUnityDelegate(bool hideUnity);

    public enum HttpMethod
    {
        GET,
        POST,
        DELETE
    }

    public class FBResult
    {
        public string Error { get; set; }
        public string Text { get; set; }
        public JsonObject Json { get; set; }
    }

    public class MissingPlatformException : Exception
    {
        public MissingPlatformException()
            : base("Platform components have not been set")
        {
        }
    }

    public static class FB
    {

#if NETFX_CORE
        private static FacebookClient _client;
        private static IWebInterface _web;
        private static HideUnityDelegate _onHideUnity;
        private static string _redirectUrl = "http://www.facebook.com/connect/login_success.html";

        private const string TOKEN_KEY = "ATK";
        private const string EXPIRY_DATE = "EXP";
        private const string EXPIRY_DATE_BIN = "EXPB";
        private const string FBID_KEY = "FBID";
        private const string FBNAME_KEY = "FBNAME";

#endif

        public static string UserId { get; private set; }
        public static string UserName { get; set; }
        public static bool IsLoggedIn { get { return !string.IsNullOrEmpty(AccessToken); } }
        public static string AppId { get; private set; }
        public static string AccessToken { get; private set; }

        public static DateTime Expires { get; private set; }

        // check whether facebook is initialized
        public static bool IsInitialized
        {
            get
            {
#if NETFX_CORE
                return _client != null;
#else
                throw new PlatformNotSupportedException("");
#endif
            }
        }

        public static void Logout()
        {
#if NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || !IsLoggedIn) return;

            var uri = _client.GetLogoutUrl(new
            {
                access_token = AccessToken,
                next = _redirectUrl,
                display = "popup"
            });

            _web.ClearCookies();
            InvalidateData();
            _web.Navigate(uri, false, (url, state) =>
            {
                _web.Finish(); 
            },
            (url, error, state) => _web.Finish());
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                {
                    callback(new FBResult() { Error = "Already in use" });
                }
                return;
            }

            var uri = _client.GetLoginUrl(new
            {
                redirect_uri = _redirectUrl,
                scope = permissions,
                display = "popup",
                response_type = "token"
            });
            _web.ClearCookies();
            _web.Navigate(uri, true, onError: LoginNavigationError, state: callback, startedCallback: LoginNavigationStarted);
            if (_onHideUnity != null)
            {
                _onHideUnity(true);
            }
#else
            throw new PlatformNotSupportedException("");
#endif
        }


#if NETFX_CORE

        private static void LoginNavigationError(Uri url, int error, object state)
        {
            //Debug.LogError("Nav error: " + error);
            if (state is FacebookDelegate)
                ((FacebookDelegate)state)(new FBResult() { Error = error.ToString() });
            _web.Finish();
            if (_onHideUnity != null)
                _onHideUnity(false);
        }

        private static void LoginNavigationStarted(Uri url, object state)
        {
            FacebookOAuthResult result;
            // Check if we're waiting for user input or if login is complete
            if (_client.TryParseOAuthCallbackUrl(url, out result))
            {
                // Login complete
                if (result.IsSuccess)
                {
                    AccessToken = result.AccessToken;
                    Expires = result.Expires;
                    _client.AccessToken = AccessToken;
                    Settings.Set(TOKEN_KEY, EncryptionProvider.Encrypt(AccessToken, AppId));
                    Settings.Set(EXPIRY_DATE_BIN, Expires.ToBinary());
                }
                _web.Finish();
                if (_onHideUnity != null)
                {
                    _onHideUnity(false);
                }

                API("/me?fields=id,name", HttpMethod.GET, fbResult =>
                {
                    if (IsLoggedIn)
                    {
                        UserId = fbResult.Json["id"] as string;
                        UserName = fbResult.Json["name"] as string;
                        Settings.Set(FBID_KEY, UserId);
                        Settings.Set(FBNAME_KEY, UserName);
                    }

                    if (state is FacebookDelegate)
                    {
                        JsonObject jResult = new JsonObject();
                        jResult.Add(new KeyValuePair<string, object>("authToken", AccessToken));
                        jResult.Add(new KeyValuePair<string, object>("authTokenExpiry", Expires.ToString()));

                        ((FacebookDelegate)state)(new FBResult()
                        {
                            Json = jResult,
                            Text = jResult.ToString()
                        });
                    }
                });
            }
        }
#endif

        public static void Init(
            InitDelegate onInitComplete,
            string appId,
            HideUnityDelegate onHideUnity, string redirectUrl = null)
        {
#if NETFX_CORE
            if (_client != null)
            {
                if (onInitComplete != null)
                    onInitComplete();
                return;
            }
            if (_web == null) throw new MissingPlatformException();
            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("Invalid Facebook App ID");

            if (!string.IsNullOrEmpty(redirectUrl))
                _redirectUrl = redirectUrl;

            _client = new FacebookClient();
            _client.GetCompleted += HandleGetCompleted;
            AppId = _client.AppId = appId;
            _onHideUnity = onHideUnity;

            if (Settings.HasKey(TOKEN_KEY))
            {
                AccessToken = EncryptionProvider.Decrypt(Settings.GetString(TOKEN_KEY), AppId);
                if (Settings.HasKey(EXPIRY_DATE))
                {
                    string expDate = EncryptionProvider.Decrypt(Settings.GetString(EXPIRY_DATE), AppId);
                    Expires = DateTime.Parse(expDate, CultureInfo.InvariantCulture);
                }
                else
                {
                    long expDate = Settings.GetLong(EXPIRY_DATE_BIN);
                    Expires = DateTime.FromBinary(expDate);
                }
                _client.AccessToken = AccessToken;
                UserId = Settings.GetString(FBID_KEY);
                UserName = Settings.GetString(FBNAME_KEY);

                // verifies if the token has expired:
                if (DateTime.Compare(DateTime.UtcNow, Expires) > 0)
                    InvalidateData();
                //var task = TestAccessToken();     
                //task.Wait();
            }

            if (onInitComplete != null)
                onInitComplete();
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void ChangeRedirect(string redirectUrl)
        {
#if NETFX_CORE
            if (!string.IsNullOrEmpty(redirectUrl) && !_web.IsActive)
                _redirectUrl = redirectUrl;
#endif
        }

#if NETFX_CORE

        /// <summary>
        /// Test if the access token is still valid by making a simple API call
        /// </summary>
        /// <returns>The async task</returns>
        private static async Task TestAccessToken()
        {
            try
            {
                await _client.GetTaskAsync("/me?fields=id,name");
            }
            catch (FacebookApiException)
            {
                // If any exception then auto login has been an issue.  Set everything to null so the game 
                // thinks the user is logged out and they can restart the login procedure
                InvalidateData();
            }
        }

        private static void InvalidateData()
        {
            AccessToken = null;
            Expires = default(DateTime);
            UserId = null;
            UserName = null;
            _client.AccessToken = null;
            Settings.DeleteKey(TOKEN_KEY);
            Settings.DeleteKey(FBID_KEY);
            Settings.DeleteKey(FBNAME_KEY);
            Settings.DeleteKey(EXPIRY_DATE);
            Settings.DeleteKey(EXPIRY_DATE_BIN);
        }

        private static void HandleGetCompleted(object sender, FacebookApiEventArgs e)
        {
            var callback = e.UserState as FacebookDelegate;
            if (callback != null)
            {
                var result = new FBResult();
                if (e.Cancelled)
                    result.Error = "Cancelled";
                else if (e.Error != null)
                    result.Error = e.Error.Message;
                else
                {
                    var obj = e.GetResultData();
                    result.Text = obj.ToString();
                    result.Json = obj as JsonObject;
                }
                Dispatcher.InvokeOnAppThread(() => { callback(result); });
            }
        }

#endif

        public static void API(
            string endpoint,
            HttpMethod method,
            FacebookDelegate callback,
            object parameters = null)
        {
#if NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (!IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                    callback(new FBResult() { Error = "Not logged in" });
                return;
            }

            Task.Run(async () =>
            {
                FBResult fbResult = null;
                try
                {
                    object apiCall;
                    if (method == HttpMethod.GET)
                    {
                        apiCall = await _client.GetTaskAsync(endpoint, parameters);
                    }
                    else if (method == HttpMethod.POST)
                    {
                        apiCall = await _client.PostTaskAsync(endpoint, parameters);
                    }
                    else
                    {
                        apiCall = await _client.DeleteTaskAsync(endpoint);
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

        public static void SetPlatformInterface(IWebInterface web)
        {
#if NETFX_CORE
            _web = web;
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Show Request Dialog.
        /// to, title, data, filters, excludeIds and maxRecipients are not currently supported at this time.
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
#if NETFX_CORE
            ///
            /// @note: [vaughan.sanders 15.8.14] We are overriding the Unity FB.AppRequest here to send a more
            /// general web style request as WP8 does not support the actual request functionality.
            /// Currently we ignore all but the message and callback params
            ///

            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || !IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                    callback(new FBResult() { Error = "Already in use / Not Logged In" });
                return;
            }

            if (_onHideUnity != null)
                _onHideUnity(true);

            Uri uri = new Uri("https://www.facebook.com/dialog/apprequests?app_id=" + AppId +
                "&message=" + message + "&display=popup&redirect_uri=" + _redirectUrl, UriKind.RelativeOrAbsolute);
            _web.Navigate(uri, true, finishedCallback: (url, state) =>
            {
                if (url.ToString().StartsWith(_redirectUrl))
                {
                    // parsing query string to get request id and facebook ids of the people the request has been sent to
                    // or error code and error messages
                    FBResult fbResult = new FBResult();

                    fbResult.Json = new JsonObject();

                    string[] queries = url.Query.Split('&');
                    if (queries.Length > 0)
                    {
                        string request = string.Empty;
                        List<string> toList = new List<string>();

                        foreach (string query in queries)
                        {
                            string[] keyValue = query.Split('=');
                            if (keyValue.Length == 2)
                            {
                                if (keyValue[0].Contains("request"))
                                    request = keyValue[1];
                                else if (keyValue[0].Contains("to"))
                                    toList.Add(keyValue[1]);
                                else if (keyValue[0].Contains("error_code"))
                                    fbResult.Error = keyValue[1];
                                else if (keyValue[0].Contains("error_message"))
                                    fbResult.Text = keyValue[1].Replace('+', ' ');
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(request))
                        {
                            fbResult.Json.Add(new KeyValuePair<string, object>("request", request));
                            fbResult.Json.Add(new KeyValuePair<string, object>("to", toList));
                        }
                    }

                    // If there's no error, assign the success text
                    if (string.IsNullOrWhiteSpace(fbResult.Text))
                        fbResult.Text = "Success";

                    _web.Finish();
                    if (_onHideUnity != null)
                        _onHideUnity(false);
                    if (callback != null)
                        callback(fbResult);
                }
            }, onError: LoginNavigationError, state: callback);

            // throw not supported exception when user passed in parameters not supported currently
            if (!string.IsNullOrWhiteSpace(filters) || excludeIds != null || maxRecipients != null || to != null || !string.IsNullOrWhiteSpace(data) || !string.IsNullOrWhiteSpace(title))
                throw new NotSupportedException("to, title, data, filters, excludeIds and maxRecipients are not currently supported at this time.");
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
#if NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || !IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                    callback(new FBResult() { Error = "Already in use / Not Logged In" });
                return;
            }

            if (_onHideUnity != null)
                _onHideUnity(true);

            Uri uri = new Uri("https://www.facebook.com/dialog/feed?app_id=" + AppId + "&to=" + toId +
                "&link=" + link + "&name=" + linkName + "&caption=" + linkCaption + "&description=" + linkDescription + "&picture=" + picture + "&display=popup&redirect_uri=" + _redirectUrl, UriKind.RelativeOrAbsolute);
            _web.Navigate(uri, true, finishedCallback: (url, state) =>
            {
                if (url.ToString().StartsWith(_redirectUrl))
                {
                    // parsing query string to get request id and facebook ids of the people the request has been sent to
                    // or error code and error messages
                    FBResult fbResult = new FBResult();

                    fbResult.Json = new JsonObject();

                    string[] queries = url.Query.Split('&');
                    if (queries.Length > 0)
                    {
                        string postId = string.Empty;
                        List<string> toList = new List<string>();

                        foreach (string query in queries)
                        {
                            string[] keyValue = query.Split('=');
                            if (keyValue.Length == 2)
                            {
                                if (keyValue[0].Contains("post_id"))
                                    postId = keyValue[1];
                                else if (keyValue[0].Contains("error_code"))
                                    fbResult.Error = keyValue[1];
                                else if (keyValue[0].Contains("error_msg"))
                                    fbResult.Text = keyValue[1].Replace('+', ' ');
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(postId))
                        {
                            fbResult.Json.Add(new KeyValuePair<string, object>("post_id", postId));
                        }
                    }

                    // If there's no error, assign the success text
                    if (string.IsNullOrWhiteSpace(fbResult.Text))
                        fbResult.Text = "Success";

                    _web.Finish();
                    if (_onHideUnity != null)
                        _onHideUnity(false);
                    if (callback != null)
                        callback(fbResult);
                }
            }, onError: LoginNavigationError, state: callback);

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