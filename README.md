MarkerMetro.Unity.WinIntegration
================================

Plugin Libraries to support implementations of Windows 8 or Windows Phone platform specific functionality

Why?
================================
Not all platform specific features are implemented, or implemented well by either Unity or other commercial plugins. 

Where this is the case, we have implemented plugins to help


What?
================================
This plugin helps with: Store Integration, Helper to Get App Version, (More TBC!)

# Store Integration

Add a using statement to include the Store APIs.

using MarkerMetro.Unity.WinIntegration.Store;

Firstly, ensure you initialise the store manager in App.xaml.cs, specifying whether you want the simulated IAP or not. You can do this alongside the call to initialize the plugin's Dispatcher (see "How?" below).

StoreManager.Initialise(bool useSimulator)

There is a single Store API for both Windows 8.1 and Windows Phone 8, with methods like the following:

bool Store.StoreManager.Instance.IsActiveTrial

void MarkerMetro.Unity.WinIntegration.Store.StoreManager.Instance.PurchaseApplication(PurchaseDelegate callback)

void MarkerMetro.Unity.WinIntegration.Store.StoreManager.Instance.RetrieveProducts(ProductListDelegate callback)

void MarkerMetro.Unity.WinIntegration.Store.StoreManager.Instance.PurchaseProduct(PurchaseDelegate callback)

# Get App Version

Add a using statement to include the  APIs.

using MarkerMetro.Unity.WinIntegration;

The following method will return the manifest version of the app:

Helper.Instance.GetAppVersion()


How?
================================
This library is published on the Marker Metro NuGet Feed (https://github.com/MarkerMetro/MarkerMetro.ProcessAutomation/wiki)

Always use App.xaml.cs (in both Windows 8.1 and Windows Phone to initialize the plugin's Dispatcher for marshalling threads:
https://github.com/MarkerMetro/SportsJeopardy/blob/windows/WindowsSolution/WindowsStoreApps/Sports%20Jeopardy!/App.xaml.cs

To update and use a NuGet plugin on a project see an example here:
https://github.com/MarkerMetro/SportsJeopardy#marker-metro-nuget-access

Generally speaking once set up, you can push changes, run a new build via the build server

1. Push Changes
2. Run a new build via the http://mmbuild1.cloudapp.net/ build server
3. Run the bat file in your project which will copy new versions into Unity plugins folders
