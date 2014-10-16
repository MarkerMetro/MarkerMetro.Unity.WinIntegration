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
using System.IO.IsolatedStorage;
#elif NETFX_CORE
using Windows.Storage;
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

#if WINDOWS_PHONE || NETFX_CORE
        private static FacebookClient _client;
        private static IWebInterface _web;
        private static HideUnityDelegate _onHideUnity;
        private static string _redirectUrl = "http://www.facebook.com/connect/login_success.html";

        private const string TOKEN_KEY = "ATK";
        private const string EXPIRY_DATE = "EXP";
        private const string FBID_KEY = "FBID";
        private const string FBNAME_KEY = "FBNAME";

#endif

        public static string UserId { get; private set; }
        public static string UserName { get; set; }
        public static bool IsLoggedIn { get { return !string.IsNullOrEmpty(AccessToken); } }
        public static string AppId { get; private set; }
        public static string AccessToken { get; private set; }

        public static DateTime Expires { get; private set; }

        public static void Logout()
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || !IsLoggedIn) return;

            var uri = _client.GetLogoutUrl(new
            {
                access_token = AccessToken,
                next = _redirectUrl
            });

            _web.Navigate(uri, (url, state) =>
            {
                InvalidateData();
                _web.Finish();
            },
            (url, error, state) => _web.Finish());
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void Login(string permissions, FacebookDelegate callback)
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (_web.IsActive || IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                    Dispatcher.InvokeOnAppThread( () => { callback(new FBResult() { Error = "Already in use" }); });
                return;
            }

            var uri = _client.GetLoginUrl(new
            {
                redirect_uri = _redirectUrl,
                scope = permissions,
                display = "popup",
                response_type = "token"
            });
            _web.Navigate(uri, onError: LoginNavigationError, state: callback, startedCallback: LoginNavigationStarted);
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });
#else
            throw new PlatformNotSupportedException("");
#endif
        }


#if WINDOWS_PHONE || NETFX_CORE

        private static void LoginNavigationError(Uri url, int error, object state)
        {
            //Debug.LogError("Nav error: " + error);
            if (state is FacebookDelegate)
                Dispatcher.InvokeOnAppThread(() => { ((FacebookDelegate)state)(new FBResult() { Error = error.ToString() }); });
            _web.Finish();
            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
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
                    FBStorage.SetString(TOKEN_KEY, EncryptionProvider.Encrypt(AccessToken, AppId));
                    FBStorage.SetString(EXPIRY_DATE, EncryptionProvider.Encrypt(
                        Expires.ToString("d", CultureInfo.InvariantCulture), AppId));
                }
                _web.Finish();
                if (_onHideUnity != null)
                    Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });

                API("/me?fields=id,name", HttpMethod.GET, fbResult =>
                {
                    if (IsLoggedIn)
                    {
                        UserId = fbResult.Json["id"] as string;
                        UserName = fbResult.Json["name"] as string;
                        FBStorage.SetString(FBID_KEY, UserId);
                        FBStorage.SetString(FBNAME_KEY, UserName);
                    }

                    if (state is FacebookDelegate)
                    {
                        JsonObject jResult = new JsonObject();
                        jResult.Add(new KeyValuePair<string, object>("authToken", AccessToken));
                        jResult.Add(new KeyValuePair<string, object>("authTokenExpiry", Expires.ToString()));
                        
                        Dispatcher.InvokeOnAppThread(() => { ((FacebookDelegate)state)(new FBResult() {
                            Json = jResult,
                            Text = jResult.ToString()
                        }); });
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
#if WINDOWS_PHONE || NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("Invalid Facebook App ID");

            if (!string.IsNullOrEmpty(redirectUrl))
                _redirectUrl = redirectUrl;

            _client = new FacebookClient();
            _client.GetCompleted += HandleGetCompleted;
            AppId = _client.AppId = appId;
            _onHideUnity = onHideUnity;

            if (FBStorage.HasKey(TOKEN_KEY))
            {
                AccessToken = EncryptionProvider.Decrypt(FBStorage.GetString(TOKEN_KEY), AppId);
                string expDate = EncryptionProvider.Decrypt(FBStorage.GetString(EXPIRY_DATE), AppId);
                Expires = DateTime.Parse(expDate, CultureInfo.InvariantCulture);
                _client.AccessToken = AccessToken;

                // verifies if the token has expired:
                if (DateTime.Compare(DateTime.Now, Expires) > 0)  // < timezone?
                    InvalidateData();
                //var task = TestAccessToken();     
                //task.Wait();
            }

            _client.AccessToken = AccessToken;
            UserId = FBStorage.GetString(FBID_KEY);
            UserName = FBStorage.GetString(FBNAME_KEY);

            if (onInitComplete != null)
                Dispatcher.InvokeOnAppThread(() => { onInitComplete(); });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

#if WINDOWS_PHONE || NETFX_CORE

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
            catch ( FacebookApiException )
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
            FBStorage.DeleteKey(TOKEN_KEY);
            FBStorage.DeleteKey(FBID_KEY);
            FBStorage.DeleteKey(FBNAME_KEY);
            FBStorage.DeleteKey(EXPIRY_DATE);
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
            FacebookDelegate callback)
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (_web == null) throw new MissingPlatformException();
            if (!IsLoggedIn)
            {
                // Already in use
                if (callback != null)
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Not logged in" }); });
                return;
            }

            if (method != HttpMethod.GET) throw new NotImplementedException();

            _client.GetAsync(endpoint, null, callback);
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        public static void SetPlatformInterface(IWebInterface web)
        {
#if WINDOWS_PHONE || NETFX_CORE
            _web = web;
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
#if WINDOWS_PHONE || NETFX_CORE
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
                    Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Error = "Already in use / Not Logged In" }); });
                return;
            }

            if (_onHideUnity != null)
                Dispatcher.InvokeOnAppThread(() => { _onHideUnity(true); });

            Uri uri = new Uri("https://www.facebook.com/dialog/apprequests?app_id=" + AppId + 
                "&message=" + message + "&redirect_uri=" + _redirectUrl, UriKind.RelativeOrAbsolute);
            _web.Navigate(uri, finishedCallback: (url, state) => 
            {
                if ( url.ToString().StartsWith( _redirectUrl ) )
                {
                    _web.Finish();
                    if (_onHideUnity != null)
                        Dispatcher.InvokeOnAppThread(() => { _onHideUnity(false); });
                    if (callback != null)
                        Dispatcher.InvokeOnAppThread(() => { callback(new FBResult() { Text = "Finished AppRequest" }); });
                }
            }, onError: LoginNavigationError, state: callback);
#else
            throw new PlatformNotSupportedException("");
#endif
        }

    }
}