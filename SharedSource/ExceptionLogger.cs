using MarkerMetro.Unity.WinIntegration.Resources;
using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
#if NETFX_CORE
using Mindscape.Raygun4Net;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using System.Collections;
#elif WINDOWS_PHONE
using Mindscape.Raygun4Net;
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Phone.Info;
using Windows.ApplicationModel.Store;
using Windows.Networking.Connectivity;
#endif

namespace MarkerMetro.Unity.WinIntegration
{


    /// <summary>
    /// Exception Logger 
    /// </summary>
    public class ExceptionLogger
    {

        static ExceptionLogger _instance;
        static readonly object _sync = new object();

#if NETFX_CORE || WINDOWS_PHONE
        Lazy<RaygunClient> _logger;
#endif

        public static ExceptionLogger Instance
        {
            get
            {
                lock (_sync)
                {
                    if (_instance == null)
                        _instance = new ExceptionLogger();
                }
                return _instance;
            }
        }

        public void Initialize(string apiKey)
        {    
#if NETFX_CORE || WINDOWS_PHONE
            _logger = new Lazy<RaygunClient>(
            () => new RaygunClient(apiKey) 
            {
                ApplicationVersion = Helper.Instance.GetAppVersion(),
                User = Helper.Instance.GetUserDeviceId(),
                // UserInfo = 
            });
#else
            throw new PlatformNotSupportedException("ExceptionLogger not supported");
#endif
        }

        public void Send(Exception ex)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Dispatcher.InvokeOnUIThread(() => 
            {
                AssertInitialized();
                _logger.Value.Send(ex);
            });
#else
            throw new PlatformNotSupportedException("ExceptionLogger not supported");
#endif
        }

        public void Send(string message, string stackTrace)
        {
#if NETFX_CORE || WINDOWS_PHONE
            Dispatcher.InvokeOnUIThread(() => 
            {
                AssertInitialized();
                _logger.Value.Send(new WrappedException(message, stackTrace));
            });
#else
            throw new PlatformNotSupportedException("ExceptionLogger not supported");
#endif
        }

        /// <summary>
        /// Call to assert that SharedLogger has been initialized
        /// </summary>
        public void AssertInitialized()
        {
            Debug.Assert(_instance != null, "You must initialize ExceptionLogger before calling it");

            if (_instance == null)
                throw new InvalidOperationException("You must initialize ExceptionLogger before calling it");
        }

    }
}
