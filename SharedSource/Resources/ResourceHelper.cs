using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
#endif


namespace MarkerMetro.Unity.WinIntegration.Resources
{
    /// <summary>
    /// Resource Helper
    /// </summary>
    public class ResourceHelper
    {

        private static ResourceHelper _instance;
        private static readonly object _sync = new object();
#if NETFX_CORE
        private static ResourceLoader _resourceLoader;
#endif
        private static string _resourceBaseName;

        public static ResourceHelper GetInstance(string resourceBaseName = "")
        {
            lock (_sync)
            {
                if (_instance == null)
                    _instance = new ResourceHelper();
            }
            _resourceBaseName = resourceBaseName;
            return _instance;
        }

#if NETFX_CORE

        /// <summary>
        ///   Returns the cached ResourceLoader instance used by this class.
        /// </summary>
        private static ResourceLoader ResourceLoader
        {
            get
            {
                if (object.ReferenceEquals(_resourceLoader, null))
                {
                    ResourceLoader temp = new ResourceLoader();
                    _resourceLoader = temp;
                }
                return _resourceLoader;
            }
        }

#endif


        /// <summary>
        /// Returns a localized value based on a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string resourceKey)
        {
            string val = String.Empty;
#if NETFX_CORE
            ResourceLoader rl = new ResourceLoader(); 
            try
            {
                val = rl.GetString(resourceKey);
            }
            catch
            {
                val = String.Empty;
            }
#endif
            return val;
        }

    }
}