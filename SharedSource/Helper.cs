using MarkerMetro.Unity.WinIntegration.Resources;
using MarkerMetro.Unity.WinIntegration.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Contacts;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.Foundation;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Networking.PushNotifications;
using Windows.ApplicationModel.Store;
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
#else
            throw new PlatformNotSupportedException("ClearLocalState");
#endif
		}

		public void GetPushChannel(string channelName, Action<string> callback)
		{
#if NETFX_CORE 
			Dispatcher.InvokeOnUIThread(async () =>
			{
				try
				{
					var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
					if (callback != null)
					{
						Dispatcher.InvokeOnAppThread(() => callback(channel.Uri));
					}
				}
				catch
				{
					if (callback != null)
					{
						Dispatcher.InvokeOnAppThread(() => callback(String.Empty));
					}
				}
			});
#else
            throw new PlatformNotSupportedException("GetPushChannel(string channelName, Action<string> callback)");
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
            try
            {
                Dispatcher.InvokeOnUIThread(() => DataTransferManager.ShowShareUI());
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Unable to show the share UI because of: " + ex.Message);
#else
                ExceptionLogger.Send(ex);
#endif
            }
#endif
		}

		/// <summary>
		/// Show the Share UI with a link - WP8 only
		/// </summary>
		/// <param name="text"></param>
		/// <param name="linkURL"></param>
		public void ShowShareUI(string text, string linkURL)
		{
			ShowShareUI(text, text, linkURL);
		}

		/// <summary>
		/// Show the Share UI with a link - WP8 only
		/// </summary>
		/// <param name="text"></param>
		/// <param name="message"></param>
		/// <param name="linkURL"></param>
		public void ShowShareUI(string text, string message, string linkURL)
		{
#if NETFX_CORE
            throw new NotImplementedException("Not implemented for Windows Store Apps, use ShowShareUI() instead.");
#endif
		}

		/// <summary>
		/// Show the Rate UI
		/// </summary>
		public void ShowRateUI()
		{
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(
                async () =>
                {
                    try
                    {
                        Uri uri;
#if WINDOWS_PHONE_APP
                        uri = new Uri(String.Format("ms-windows-store:REVIEWAPP?appid={0}", CurrentApp.AppId));
#else
                        var data = Package.Current.Id.FamilyName;
                        uri = new Uri(String.Format("ms-windows-store:REVIEW?PFN={0}", data));
#endif

# if DEBUG
                        Debug.WriteLine(uri.ToString());
# endif
                        await Launcher.LaunchUriAsync(uri);
                    }
                    catch (Exception ex)
                    {
# if DEBUG
                        Debug.WriteLine("Unable to show MarketplaceReviewTask because of: " + ex.Message);
# else
                        ExceptionLogger.Send(ex);
# endif
                    }
                });
#else
            throw new NotImplementedException("Not implemented for unknown platform");
#endif
		}

		/// <summary>
		/// Uses a roaming Guid for Windows 8 and the device id for WP8
		/// </summary>
		/// <returns></returns>
		public string GetUserDeviceId()
		{
#if NETFX_CORE
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
		/// Returns the device manufacturer name.
		/// </summary>
		public string GetManufacturer()
		{
#if NETFX_CORE
            return new EasClientDeviceInformation().SystemManufacturer;
#else
            throw new PlatformNotSupportedException("GetManufacturer()");
#endif
		}

		/// <summary>
		/// Returns the device model (e.g. "NOKIA Lumia 720").
		/// </summary>
		/// <remarks>
		/// If we need a more friendly name on WP8, read this:
		/// http://stackoverflow.com/questions/17425016/information-about-windows-phone-model-number
		/// </remarks>
		public string GetModel()
		{
#if NETFX_CORE
            return new EasClientDeviceInformation().SystemProductName;
#else
            throw new PlatformNotSupportedException("GetManufacturer()");
#endif

		}

		/// <summary>
		/// Returns the application's store url. Note: This won't be valid until the apps are published
		/// </summary>
		/// <returns></returns>
		public string GetAppStoreUri()
        {
#if WINDOWS_PHONE_APP
            return "ms-windows-store:navigate?appid=" + CurrentApp.AppId;
#elif NETFX_CORE
            return "ms-windows-store:PDP?PFN=" + Package.Current.Id.FamilyName;
#else
            return "";
#endif
        }



#if NETFX_CORE && !WINDOWS_PHONE_APP
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
#if WINDOWS_UWP
        [System.Runtime.InteropServices.DllImport("api-ms-win-core-sysinfo-l1-2-0.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);
#else
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);
#endif
#endif

        /// <summary>
        /// Determine whether or not a Windows device is generally considered low end
        /// </summary>
        /// <returns>Windows 8 Arm or Windows Phone Low Mem returns true</returns>
        public bool IsLowEndDevice()
        {
#if WINDOWS_PHONE_APP
            long result = 0;
            try
            {
                result = (long)MemoryManager.AppMemoryUsageLimit;
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

        /// <summary>
        /// This is used to check whether the app is running on Windows Mobile.
        /// Should only be used for Windows 10 Universal project.
        /// </summary>
        public static bool IsWindowsMobile
        {
            get
            {
#if WINDOWS_UWP && NETFX_CORE
                return Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent ("Windows.Phone.PhoneContract", 1);
#else
                throw new PlatformNotSupportedException("IsWindowsMobile");
#endif
            }
        }

        public bool HasInternetConnection
		{
			get
			{
#if NETFX_CORE
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
#if NETFX_CORE
				var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null)
                {
                    return false;
                }
				return profile.GetConnectionCost().NetworkCostType != NetworkCostType.Unrestricted;
#else
                throw new PlatformNotSupportedException("IsMeteredConnection");
#endif
			}
		}

		public void ShowDialog(string contentKey, string titleKey, Action callback)
		{
#if NETFX_CORE
			if (string.IsNullOrWhiteSpace(contentKey))
				throw new ArgumentNullException("You must specify content resource key");

			Dispatcher.InvokeOnUIThread(() =>
			{
				var resourceHelper = ResourceHelper.GetInstance();
				var content = resourceHelper.GetString(contentKey);
				var title = string.Empty;
				if (titleKey != null)
					title = resourceHelper.GetString(titleKey);

#if NETFX_CORE
                ShowDialogAsync(contentKey, titleKey, callback).ContinueWith(t => { });
            });
# endif
#else
            throw new PlatformNotSupportedException("ShowDialog");
#endif
		}

		public void ShowDialog(string content, string title, Action<bool> callback, string okText = "", string cancelText = "")
		{
#if NETFX_CORE

            Dispatcher.InvokeOnUIThread(() =>
            {
                ShowDialogAsync(content, title, callback, okText, cancelText).ContinueWith(t => { });
            });
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
                Dispatcher.InvokeOnAppThread(() => callback());
        }

        async Task ShowDialogAsync(string content, string title, Action<bool> callback, string okText, string cancelText)
        {
            var dialog = string.IsNullOrWhiteSpace(title) ? new MessageDialog(content) : new MessageDialog(content, title);

            Action<bool> callbackOnApp = b => Dispatcher.InvokeOnAppThread(() => callback(b));

            if (!string.IsNullOrWhiteSpace(okText))
            {
                dialog.Commands.Add(new UICommand(okText, new UICommandInvokedHandler(
                    (command) => { if (callback != null) callbackOnApp(true); })));
            }
            if (!string.IsNullOrWhiteSpace(cancelText))
            {
                dialog.Commands.Add(new UICommand(cancelText, new UICommandInvokedHandler(
                    (command) => { if (callback != null) callbackOnApp(false); })));
            }

            if (!string.IsNullOrWhiteSpace(okText) && !string.IsNullOrWhiteSpace(cancelText))
            {
                // Set the command that will be invoked by default
                dialog.DefaultCommandIndex = 0;
                // Set the command to be invoked when escape is pressed
                dialog.CancelCommandIndex = 1;
            }

            await dialog.ShowAsync();
        }
#endif

        /// <summary>
        /// Win8 - Launches a mailto: URI.
        /// WP8 - Calls email compose task.
        /// </summary>
        /// <param name="to">A comma-separated list of email addresses.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        public void SendEmail(string to, string subject, string body)
        {
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(async () =>
            {
                subject = Uri.EscapeDataString(subject);
                body = Uri.EscapeDataString(body);

                var mailto = new Uri(String.Format("mailto:?to={0}&subject={1}&body={2}", to, subject, body));
                await Launcher.LaunchUriAsync(mailto);
            });
#endif
		}
	}
}
