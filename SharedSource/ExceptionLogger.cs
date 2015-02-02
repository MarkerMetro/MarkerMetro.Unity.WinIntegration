using System;
using System.Diagnostics;
#if NETFX_CORE
using Mindscape.Raygun4Net;
using Windows.UI.Xaml;
#elif WINDOWS_PHONE
using Mindscape.Raygun4Net;
using System.Windows;
using Microsoft.Phone.Controls;
#endif

namespace MarkerMetro.Unity.WinIntegration
{

    /// <summary>
    /// Exception Logger.
    /// </summary>
    public class ExceptionLogger
    {

        public enum LoggerType
        {
            RaygunIO
        }

        static ExceptionLogger _instance;
        static readonly object _sync = new object();


#if NETFX_CORE || WINDOWS_PHONE
        Lazy<RaygunClient> _raygun;
        LoggerType _loggerType = LoggerType.RaygunIO;
#endif

        private ExceptionLogger()
        { }

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

        public bool Initialized
        {
            get
            {
#if NETFX_CORE || WINDOWS_PHONE
                if (_loggerType == LoggerType.RaygunIO)
                {
                    return _raygun != null;
                }
                else
                {
                    return false;
                }
#else
                return false;
#endif
            }
        }

        public static void Initialize(string apiKey, LoggerType loggerType = LoggerType.RaygunIO)
        {
            lock (_sync)
            {
                if (_instance == null)
                    _instance = new ExceptionLogger();

#if NETFX_CORE || WINDOWS_PHONE

                _instance._loggerType = loggerType;

                if (loggerType == LoggerType.RaygunIO)
                { 
                    _instance._raygun = new Lazy<RaygunClient>(() => BuildRaygunClient(apiKey));
                }
#else
                Debug.WriteLine("ExceptionLogger not supported");
#endif
            }
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
            if (_loggerType == LoggerType.RaygunIO)
            {
                if (_raygun != null)
                {
                    _raygun.Value.Send(ex);
                }
            }
            else
            {
                throw new NotSupportedException("ExceptionLogger.Send()");
            }
#else
            throw new PlatformNotSupportedException("ExceptionLogger.Send()");
#endif
        }

        public void Send(string message, string stackTrace)
        {
#if NETFX_CORE || WINDOWS_PHONE
           if (_loggerType == LoggerType.RaygunIO)
            {
                if (_raygun != null)
                {
                 _raygun.Value.Send(new WrappedException(message, stackTrace));
                }
            }
            else
            {
                throw new NotSupportedException("ExceptionLogger.Send()");
            }

#else
            throw new PlatformNotSupportedException("ExceptionLogger.Send()");
#endif
        }

    }
}
