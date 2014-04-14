using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETFX_CORE
using Windows.ApplicationModel;
#elif WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
#endif


namespace MarkerMetro.Unity.WinIntegration
{
    /// <summary>
    /// Integration Helpers
    /// </summary>
    public class Helper
    {

        private static Helper _instance;
        private static readonly object _sync = new object();

        public static Helper Instance
        {
            get
            {
                lock (_sync)
                {
                    if (_instance == null)
                        _instance = new Helper();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Returns the application package version 
        /// </summary>
        /// <returns></returns>
        public string GetAppVersion()
        {
#if NETFX_CORE
            var major = Package.Current.Id.Version.Major;
            var minor = Package.Current.Id.Version.Minor.ToString();
            var revision = Package.Current.Id.Version.Revision.ToString();
            var build = Package.Current.Id.Version.Build.ToString();
            var version = String.Format("{0}.{1}.{2}.{3}", major, minor, build, revision);
            return version;
#elif WINDOWS_PHONE
            return XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
#else
            return String.Empty;
#endif

        }

//        /// <summary>
//        /// Returns something approximating a device id
//        /// </summary>
//        /// <returns></returns>
//        public string GetDeviceID()
//        {
//#if NETFX_CORE
//            // we leverage a guid value stored in roaming settings to identify the user's device
//            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
//            var deviceID = roamingSettings.Values["MarkerMetro.Unity.WinIntegration.Guid"];
//            Guid deviceIDGuid = Guid.Empty;
//            if (deviceID != null && Guid.TryParse(deviceID.ToString(), out deviceIDGuid))
//            {
//                return deviceIDGuid.ToString();
//            }
//            else
//            {
//                deviceIDGuid = Guid.NewGuid();
//                roamingSettings.Values["MarkerMetro.Unity.WinIntegration.Guid"] = deviceIDGuid;
//                return deviceIDGuid.ToString();
//            }
//#elif WINDOWS_PHONE
//            byte[] myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
//            return Convert.ToBase64String(myDeviceID);
//#else
//            return String.Empty;
//#endif

//        }


    }
}