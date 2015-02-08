#if WINDOWS_PHONE || NETFX_CORE
using System;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#elif NETFX_CORE
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

#if WINDOWS_PHONE
                return IsolatedStorageSettings.ApplicationSettings.Contains(key);
#elif NETFX_CORE
            Object value = null;
            return roamingSettings.Values.TryGetValue(key, out value);
#endif
        }

        public static void DeleteKey(string key)
        {
            if (HasKey(key))
            {
#if WINDOWS_PHONE
                    IsolatedStorageSettings.ApplicationSettings.Remove(key);
                    IsolatedStorageSettings.ApplicationSettings.Save();
#elif NETFX_CORE
                roamingSettings.Values.Remove(key);
#endif
            }
        }

        public static void Set(string key, object value)
        {
#if WINDOWS_PHONE
                if (HasKey(key))
                {
                    IsolatedStorageSettings.ApplicationSettings[key] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(key, value);
                }
                IsolatedStorageSettings.ApplicationSettings.Save();
#elif NETFX_CORE
            roamingSettings.Values[key] = value;
#endif
        }

        public static string GetString(string key)
        {
            if (HasKey(key))
            {
#if WINDOWS_PHONE
                    return (string)IsolatedStorageSettings.ApplicationSettings[key];
#elif NETFX_CORE
                return (string)roamingSettings.Values[key];
#endif
            }
            return null;
        }

        public static long GetLong(string key)
        {
            if (HasKey(key))
            {
#if WINDOWS_PHONE
                    return (long)IsolatedStorageSettings.ApplicationSettings[key];
#elif NETFX_CORE
                return (long)roamingSettings.Values[key];
#endif
            }
            return 0;
        }
    }
}

#endif