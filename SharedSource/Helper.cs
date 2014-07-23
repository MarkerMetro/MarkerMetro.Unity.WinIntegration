using MarkerMetro.Unity.WinIntegration.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
#elif WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Phone.Info;
using Windows.ApplicationModel.Store;
using Windows.Networking.Connectivity;
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
        /// Fired when the App visibility has changed
        /// </summary>
        /// <remarks>
        /// This should be activated in CommonMainPage.cs, when a Window.Current.VisibilityChanged happens.
        /// </remarks>
        public Action<bool> OnVisibilityChanged;

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

        /// <summary>
        /// Clears all local state
        /// </summary>
        /// <remarks>
        /// used to clear all local state from the app
        /// </remarks>
        public void ClearLocalState(Action<bool> callback)
        {
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(async () =>
            {
                try
                {
                    await Windows.Storage.ApplicationData.Current.ClearAsync(Windows.Storage.ApplicationDataLocality.Local);   
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(true));
                    };            
                }
                catch
                {
                    if (callback != null)
                    {
                        Dispatcher.InvokeOnAppThread(() => callback(false));
                    };
                }
            });  
#elif WINDOWS_PHONE
            try
            {
                using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                {
                    store.Remove();
                }
                if (callback != null)
                {
                    Dispatcher.InvokeOnAppThread(() => callback(true));
                };  
            }
            catch
            {
                if (callback != null)
                {
                    Dispatcher.InvokeOnAppThread(() => callback(false));
                };
            }
#else
             throw new PlatformNotSupportedException("ClearLocalState");
#endif
        }

        /// <summary>
        /// Returns the application language
        /// </summary>
        /// <remarks>
        /// it's important to use this call rather than Unity APIs, which just return the top system language
        /// </remarks>
        public string GetAppLanguage()
        {
#if NETFX_CORE
            return Windows.Globalization.ApplicationLanguages.Languages[0];
#elif WINDOWS_PHONE
            return System.Globalization.CultureInfo.CurrentUICulture.Name;
#else
            return System.Globalization.CultureInfo.CurrentUICulture.Name;
#endif
        }

        /// <summary>
        /// Show the Share UI
        /// </summary>
        public void ShowShareUI()
        {
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(() => DataTransferManager.ShowShareUI());
#elif WINDOWS_PHONE
            var guidString = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("ProductID").Value;
            var productId = guidString.TrimStart('{').TrimEnd('}').ToLower();
            var task = new MarketplaceDetailTask();
            task.Show();
#endif
        }

        /// <summary>
        /// Show the Share UI with a link - WP8 only
        /// </summary>
        /// <param name="text"></param>
        /// <param name="linkURL"></param>
        public void ShowShareUI(string text, string linkURL)
        {
#if NETFX_CORE
            throw new NotImplementedException("Not implemented for Windows Store Apps, use ShowShareUI() instead.");
#elif WINDOWS_PHONE
            var task = new ShareLinkTask();
            task.Title = text;
            task.Message = text;
            task.LinkUri = new Uri(linkURL);
            task.Show();
#endif
        }

        /// <summary>
        /// Uses a roaming Guid for Windows 8 and the device id for WP8
        /// </summary>
        /// <returns></returns>
        public string GetUserDeviceId()
        {
#if WINDOWS_PHONE
            var anid = Math.Abs(UserExtendedProperties.GetValue("ANID2").GetHashCode()).ToString();
            var did = Math.Abs(DeviceExtendedProperties.GetValue("DeviceUniqueId").GetHashCode()).ToString();

            return anid + did;
#elif NETFX_CORE
            const string key = "UserDeviceId";

            var values = Windows.Storage.ApplicationData.Current.RoamingSettings.Values;

            if (!values.ContainsKey(key))
            {
                var value = Guid.NewGuid().ToString().Replace("-", "");
                values[key] = value;
            }

            return (string)values[key];
#else
            throw new PlatformNotSupportedException("GetUserDeviceId()");
#endif
        }

        /// <summary>
        /// Returns the application's store url. Note: This won't be valid until the apps are published
        /// </summary>
        /// <returns></returns>
        public string GetAppStoreUri()
        {
#if WINDOWS_PHONE
            return "http://www.windowsphone.com/s?appid=" + CurrentApp.AppId;
#elif NETFX_CORE
            return "ms-windows-store:PDP?PFN=" + Package.Current.Id.FamilyName;
#else
            return "";
#endif
        }

#if NETFX_CORE
        public enum ProcessorArchitecture : ushort
        {
            INTEL = 0,
            MIPS = 1,
            ALPHA = 2,
            PPC = 3,
            SHX = 4,
            ARM = 5,
            IA64 = 6,
            ALPHA64 = 7,
            MSIL = 8,
            AMD64 = 9,
            IA32_ON_WIN64 = 10,
            UNKNOWN = 0xFFFF
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);
#endif

        /// <summary>
        /// Determine whether or not a Windows device is generally considered low end
        /// </summary>
        /// <returns>Windows 8 Arm or Windows Phone Low Mem returns true</returns>
        public bool IsLowEndDevice()
        {
#if WINDOWS_PHONE
            long result = 0;
            try
            {
                result = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");
            }
            catch (ArgumentOutOfRangeException)
            {
                // The device does not support querying for this value. This occurs
                // on Windows Phone OS 7.1 and older phones without OS updates.
            }
            return result <= 188743680; // less than or equal to 180MB including failure scenario

#elif NETFX_CORE
            var systemInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(ref systemInfo);

            return systemInfo.wProcessorArchitecture == (uint)ProcessorArchitecture.ARM;
#else
            return false;
#endif
        }

        public bool HasInternetConnection
        {
            get
            {
#if WINDOWS_PHONE
                return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsNetworkAvailable;
#elif NETFX_CORE
                var profile = NetworkInformation.GetInternetConnectionProfile();
                return profile != null &&
                       profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
#else
                throw new PlatformNotSupportedException("HasInternetConnection");
#endif
            }
        }

        public bool IsMeteredConnection
        {
            get
            {
#if WINDOWS_PHONE || NETFX_CORE
                var profile = NetworkInformation.GetInternetConnectionProfile();
                return profile.GetConnectionCost().NetworkCostType != NetworkCostType.Unrestricted;
#else
                throw new PlatformNotSupportedException("IsMeteredConnection");
#endif
            }
        }

        public void ShowDialog(string contentKey, string titleKey, Action callback)
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (string.IsNullOrWhiteSpace(contentKey))
                throw new ArgumentNullException("You must specify content resource key");

            var resourceHelper = ResourceHelper.GetInstance();
            var content = resourceHelper.GetString(contentKey);
            var title = string.Empty;
            if(titleKey!=null)
                title = resourceHelper.GetString(titleKey);

# if WINDOWS_PHONE
            MessageBox.Show(content, title, MessageBoxButton.OK);

            if (callback != null)
                callback();
# elif NETFX_CORE
            ShowDialogAsync(contentKey, titleKey, callback).ContinueWith(t => { });
# endif

#else
            throw new PlatformNotSupportedException("ShowDialog");
#endif
        }

#if NETFX_CORE
        async Task ShowDialogAsync(string content, string title, Action callback)
        {
            var dialog = string.IsNullOrWhiteSpace(title) ? new MessageDialog(content) : new MessageDialog(content, title);

            await dialog.ShowAsync();

            if (callback != null)
                callback();
        }
#endif
    }
}
