using System;
using System.Diagnostics;
#if NETFX_CORE
using Mindscape.Raygun4Net;
using BugSense;
using Windows.UI.Xaml;
#elif WINDOWS_PHONE
using Mindscape.Raygun4Net;
using System.Windows;
using BugSense;
using Microsoft.Phone.Controls;
#endif

namespace MarkerMetro.Unity.WinIntegration
{

    /// <summary>
    /// Exception Logger 
    /// </summary>
    public class ExceptionLogger
    {

        public enum LoggerType
        {
            RaygunIO,
            BugSense
        }

        static ExceptionLogger _instance;
        static readonly object _sync = new object();
        static LoggerType _loggerType = LoggerType.RaygunIO;

#if NETFX_CORE || WINDOWS_PHONE
        Lazy<RaygunClient> _raygun;
        bool _bugsenseCreated = false;
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

        public static void Initialize(string apiKey, LoggerType loggerType = LoggerType.RaygunIO)
        {
            lock (_sync)
            {
                if (_instance == null)
                    _instance = new ExceptionLogger();

#if NETFX_CORE || WINDOWS_PHONE
                if (loggerType == LoggerType.RaygunIO)
                { 
                    _instance._raygun = new Lazy<RaygunClient>(() => BuildRaygunClient(apiKey));
                }
                else if (loggerType == LoggerType.BugSense)
                {
#if NETFX_CORE
                    BugSenseHandler.Instance.InitAndStartSession(new BugSense.Model.ExceptionManager(Application.Current), apiKey);
#elif WINDOWS_PHONE
                    BugSenseHandler.Instance.InitAndStartSession(new BugSense.Core.Model.ExceptionManager(Application.Current), ((PhoneApplicationFrame)Application.Current.RootVisual), apiKey);
#endif
                    _instance._bugsenseCreated = true;
                }
#else
                Debug.WriteLine("ExceptionLogger not supported");
#endif
            }
        }

        public void Close()
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_loggerType == LoggerType.BugSense)
            {
                BugSenseHandler.Instance.CloseSession();
                _bugsenseCreated = false;
            }
            else
            {
                throw new NotSupportedException("ExceptionLogger.Close()");
            }
#else
             throw new PlatformNotSupportedException("ExceptionLogger.Close()");
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
            if (_loggerType == LoggerType.BugSense)
            {
                if (_bugsenseCreated)
                {
                    BugSenseHandler.Instance.LogException(ex);
                }
            }
            else if (_loggerType == LoggerType.RaygunIO)
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
            if (_loggerType == LoggerType.BugSense)
            {
                if (_bugsenseCreated)
                { 
                    BugSenseHandler.Instance.LogException(new WrappedException(message, stackTrace));
                }
            }
            else if (_loggerType == LoggerType.RaygunIO)
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

        public void AddMetadata(string key, string data)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_loggerType == LoggerType.BugSense)
            {
                if (_bugsenseCreated)
                {
                    BugSenseHandler.Instance.AddCrashExtraData(new BugSense.Core.Model.CrashExtraData(key, data));
                }
            }
            else
            {
                throw new NotSupportedException("ExceptionLogger.AddMetadata()");
            }
#else
            throw new PlatformNotSupportedException("ExceptionLogger.AddMetadata()");
#endif
        }

        public void SetUsername(string username)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (_loggerType == LoggerType.BugSense)
            {
                if (_bugsenseCreated)
                {
                    BugSenseHandler.Instance.UserIdentifier = username;
                }
            }
            else
            {
                throw new NotSupportedException("ExceptionLogger.SetUsername()");
            }
#else
            throw new PlatformNotSupportedException("ExceptionLogger.AddMetadata()");
#endif
        }
    }
}
