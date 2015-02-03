using System;
using System.Diagnostics;

namespace MarkerMetro.Unity.WinIntegration
{

    /// <summary>
    /// Exception Logger.
    /// </summary>
    public static class ExceptionLogger
    {

        private static IExceptionLogger _logger;
        static readonly object _sync = new object();

        public static void Initialize(IExceptionLogger logger)
        {
            _logger = logger;
        }

        public static bool IsInitialized
        {
            get
            {
                return _logger != null;
            }
        }

        public static bool IsEnabled
        {
            get
            {
                return IsInitialized && _logger.IsEnabled;
            }
        }

        public static void Send(Exception ex)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (IsInitialized)
            {
                _logger.Send(ex);
            }
#else
            throw new PlatformNotSupportedException("ExceptionLogger.Send()");
#endif
        }

        public static void Send(string message, string stackTrace)
        {
#if NETFX_CORE || WINDOWS_PHONE
            if (IsInitialized)
            {
                _logger.Send(new WrappedException(message, stackTrace));
            }
#else
            throw new PlatformNotSupportedException("ExceptionLogger.Send()");
#endif
        }

    }
}
