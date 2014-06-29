#if WINDOWS_PHONE || NETFX_CORE

using MarkerMetro.Unity.WinLegacy.Cryptography;
using Facebook;
using System;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#elif NETFX_CORE
using Windows.Storage;
#endif

namespace MarkerMetro.WinIntegration.Facebook
{

        internal static class FBStorage
        {

#if NETFX_CORE
            private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
#endif

            public static bool HasKey(string key)
            {
                
#if WINDOWS_PHONE
                var value = String.Empty;
                return IsolatedStorageSettings.ApplicationSettings.TryGetValue(key, out value);
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
#elif NETFX_CORE
                    roamingSettings.Values.Remove(key);
#endif
                }
            }

            public static void SetString(string key, string value)
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

    }
}

#endif