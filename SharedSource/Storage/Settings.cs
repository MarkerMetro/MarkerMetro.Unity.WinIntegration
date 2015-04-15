using System;
#if NETFX_CORE
using Windows.Storage;
#endif

namespace MarkerMetro.Unity.WinIntegration.Storage
{

    public static class Settings
    {

#if NETFX_CORE
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
#endif

        public static bool HasKey(string key)
        {

#if NETFX_CORE
            Object value = null;
            return roamingSettings.Values.TryGetValue(key, out value);
#else
            return false;
#endif
        }

        public static void DeleteKey(string key)
        {
            if (HasKey(key))
            {
#if NETFX_CORE
                roamingSettings.Values.Remove(key);
#endif
            }
        }

        public static void Set(string key, object value)
        {
#if NETFX_CORE
            roamingSettings.Values[key] = value;
#endif
        }

        public static string GetString(string key)
        {
            if (HasKey(key))
            {
#if NETFX_CORE
                return (string)roamingSettings.Values[key];
#endif
            }
            return null;
        }

        public static long GetLong(string key)
        {
            if (HasKey(key))
            {
#if NETFX_CORE
                return (long)roamingSettings.Values[key];
#endif
            }
            return 0;
        }
    }
}
