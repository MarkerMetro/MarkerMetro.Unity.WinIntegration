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
using BugSense;
#elif WINDOWS_PHONE
using Mindscape.Raygun4Net;
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Phone.Info;
using Windows.ApplicationModel.Store;
using Windows.Networking.Connectivity;
using BugSense;
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
        Lazy<RaygunClient> _raygun;
        bool _bugsenseCreated = false;
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

        public static void Initialize(string apiKey)
        {
            lock (_sync)
            {
                if (_instance == null)
                    _instance = new ExceptionLogger();

#if NETFX_CORE || WINDOWS_PHONE
                _instance._raygun = new Lazy<RaygunClient>(() => BuildRaygunClient(apiKey));
#else
                Debug.WriteLine("ExceptionLogger not supported");
#endif
            }
        }

#if NETFX_CORE || WINDOWS_PHONE
#if NETFX_CORE
        public static void InitializeBugsense(string apikey, Windows.UI.Xaml.Application app)
#elif WINDOWS_PHONE
        public static void InitializeBugsense(string apikey, System.Windows.Application app, Microsoft.Phone.Controls.PhoneApplicationFrame frame)
#endif
        {
            lock (_sync)
            {
                if (_instance == null)
                    _instance = new ExceptionLogger();

#if NETFX_CORE
                BugSenseHandler.Instance.InitAndStartSession(new BugSense.Model.ExceptionManager(app), apikey);
#elif WINDOWS_PHONE
                BugSenseHandler.Instance.InitAndStartSession(new BugSense.Core.Model.ExceptionManager(app), frame, apikey);
#endif
                _instance._bugsenseCreated = true;
            }
        }
#endif

        public void Close()
        {
#if NETFX_CORE || WINDOWS_PHONE
            BugSenseHandler.Instance.CloseSession();
            _bugsenseCreated = false;
#endif
        }

#if NETFX_CORE || WINDOWS_PHONE
        static RaygunClient BuildRaygunClient(string apiKey)
        {
            try
            {
                string version = null, user = null;

                version = Helper.Instance.GetAppVersion();
                try
                {
                    user = Helper.Instance.GetUserDeviceId();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to get UserDeviceId: {0}", ex);
                }

                return new RaygunClient(apiKey)
                {
                    ApplicationVersion = version,
                    User = user,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to BuildRaygunClient", ex);

                throw;
            }
        }
#endif

        public void Send(Exception ex)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_raygun != null)
                _raygun.Value.Send(ex);
            else if (_bugsenseCreated)
                BugSenseHandler.Instance.LogException(ex);
#else
            // Debug.WriteLine("ExceptionLogger not supported: {0}", ex);
#endif
        }

        public void Send(string message, string stackTrace)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_raygun != null)
                _raygun.Value.Send(new WrappedException(message, stackTrace));
            else if (_bugsenseCreated)
                BugSenseHandler.Instance.LogException(new WrappedException(message, stackTrace));
#else
            // Debug.WriteLine("ExceptionLogger not supported: {0}", ex);
#endif
        }

        public void AddMetadata(string key, string data)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_bugsenseCreated)
                BugSenseHandler.Instance.AddCrashExtraData(new BugSense.Core.Model.CrashExtraData(key, data));
#endif
        }

        public void SetUsername(string username)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_bugsenseCreated)
                BugSenseHandler.Instance.UserIdentifier = username;
#endif
        }
    }
}
