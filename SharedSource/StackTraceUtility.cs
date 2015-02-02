using System;

namespace MarkerMetro.Unity.UnityEngine
{
    /// <summary>
    /// Implements UnityEngine.StackTraceUtility which provides the current stack trace.
    /// </summary>
    public class StackTraceUtility
    {
        public StackTraceUtility()
        {
        }

        public static string ExtractStackTrace()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                return ExtractStringFromException(e);
            }
        }

        public static string ExtractStringFromException(object exception)
        {
            return ((Exception)exception).StackTrace;
        }
    }
}