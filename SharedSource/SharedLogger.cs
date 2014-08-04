using MarkerMetro.Unity.WinIntegration.Resources;
using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Mindscape.Raygun4Net;
using System.Collections;
#elif WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Phone.Info;
using Windows.ApplicationModel.Store;
using Windows.Networking.Connectivity;
using Mindscape.Raygun4Net;
#endif

namespace MarkerMetro.Unity.WinIntegration
{
    /// <summary>
    /// Shared Logger for Web Error Logging, currently using Raygun.io
    /// </summary>
    public class SharedLogger
    {
        static readonly object _sync = new object();

#if NETFX_CORE || WINDOWS_PHONE
        static RaygunClient _client;

        static RaygunClient Client
        {
            get
            {
                lock(_sync)
                {
                    if(_client==null)
                    {
                        _client = new RaygunClient(ApiKey) 
                        {
                            ApplicationVersion = Helper.Instance.GetAppVersion(),
                            User = Helper.Instance.GetUserDeviceId(),
                            // UserInfo = 
                        };
                    }
                    return _client;
                }
            }
        }
#endif
        static string _apiKey;
        public static string ApiKey
        {
            get 
            {
                lock (_sync)
                {
                    Debug.Assert(!string.IsNullOrEmpty(_apiKey), "You must initialize SharedLogger before calling SharedLogger");

                    if (string.IsNullOrEmpty(_apiKey))
                        _apiKey = "J5M66WHC/fIcZWudEXXGOw==";   // MarkerMetro API key

                    return _apiKey;
                }
            }
            set
            {
                lock (_sync)
                {
                    if (_apiKey != value)
                    {
#if NETFX_CORE || WINDOWS_PHONE
                        if (_client != null)
                            _client = null;
#endif
                        _apiKey = value;
                    }
                }
            }
        }

        public static void Send(Exception ex)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Client.Send(ex);
#endif
        }

        public static void Send(Exception ex, System.Collections.IDictionary userData)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Client.Send(ex, userData);
#endif
        }

        public static void Send(Exception ex, System.Collections.Generic.IList<string> tags)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Client.Send(ex, tags);
#endif
        }

        public static void Send(Exception ex, string tag)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Client.Send(ex, new System.Collections.Generic.List<string>() { tag });
#endif
        }

        public static void Send(Exception ex, System.Collections.Generic.IList<string> tags, System.Collections.IDictionary userData)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Client.Send(ex, tags, userData);
#endif
        }
    }
}
