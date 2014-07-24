MarkerMetro.Unity.WinIntegration
================================

Plugin Libraries to support implementations of Windows 8 or Windows Phone platform specific functionality

This plugin should be used by the MarkerMetro.Unity.WinShared Unity Project and Windows Apps Template:
https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared

Why?
================================
Not all platform specific features are implemented, or implemented well by either Unity or other commercial plugins. 

Where this is the case, we have implemented plugins to help


What?
================================
This plugin helps with: Facebook integration, Store Integration, Helper to Get App Version, (More TBC!)

## Facebook Integration

Add a using statement to include the Facebook APIs.
```csharp
using MarkerMetro.Unity.WinIntegration.Facebook;
```

Facebook implementations are quite game specific, however you will always need to initialize the FB client, for which you can use the Marker Metro test Faceboook app created by markermetro@live.com facebook account (see \MM Team - Administration\Logins\Facebook accounts.txt" for the password on dropbox).

Here's an example of the basic calls:

```csharp
FB.Init(fbInitComplete, "682783485145217", fbOnHideUnity); 
FB.Login("publish_actions", fbResult =>
{
    // Successful login, or deal with errors
});

private void fbInitComplete()
{
    // handler for Unity to deal with FB initializati complete
}

private void fbOnHideUnity(bool isShow)
{
    // handler for UNity to deal with FB web browser visibility changes
}

```

It is assumed you will be using MarkerMetro.Unity.WinShared  which includes the necessary web view/browser controls for displaying all necessary facebook dialogs as well as initializing the links between the app and Unity sides. 

## Store Integration

Add a using statement to include the Store APIs.

```csharp
using MarkerMetro.Unity.WinIntegration.Store;
```

It is assumed you will be using MarkerMetro.Unity.WinShared which will include an iap_simulator.xml file in the root of both Windows projects. You will just need to update the IAP codes for your particular game in the respective xml files for each project.

```csharp
void StoreManager.Initialise(bool useSimulator)
```

There is a single Store API for both Win 8.1 and WP8:

Determine whether the app has a currently active trial:

```csharp
bool StoreManager.Instance.IsActiveTrial
```

Attempt to purchase the application. The Receipt object returned in the delegate will have a StatusCode of Success or ExceptionThrown if anything went wrong.

```csharp
void StoreManager.Instance.PurchaseApplication(PurchaseDelegate callback)
```

Attempt to list all IAP products available. This will be a list of products or null if anything went wrong.

```csharp
void StoreManager.Instance.RetrieveProducts(ProductListDelegate callback)
```

Attempt to purchase an IAP product. The receipt object returned in the delegate will have a StatusCode of Success or ExceptionThrow if something went badly wrong. 

Specifically for WP8, the only other StatusCode used is NotReady when after a successful purchase the license information does not appear to be valid. Windows 8.1 uses all the other status codes as more information is available.

```csharp
void StoreManager.Instance.PurchaseProduct(PurchaseDelegate callback)
```

## Helper

Add a using statement to include the  APIs.

```csharp
using MarkerMetro.Unity.WinIntegration;
```

This class will help with various things such as gettign app version, the device id, store url and determining whether the device is low end. See code documentation for more details

https://github.com/MarkerMetro/MarkerMetro.Unity.WinIntegration/blob/master/SharedSource/Helper.cs


How?
================================
This library is published on the Marker Metro NuGet Feed (https://github.com/MarkerMetro/MarkerMetro.ProcessAutomation/wiki)

This plugin should be used by the MarkerMetro.Unity.WinShared Unity Project and Windows Apps Template:
https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared

The MarkerMetro.Unity.WinShared framework will take care of proper initialization of the plugin.

Updating and using this plugin on a project based on MarkerMetro.Unity.WinShared is easy. 

1. Push Changes to this repo
2. Run a new build via the http://mmbuild1.cloudapp.net/ build server
3. Run the bat file in /Nuget folder of your project based on MarkerMetro.Unity.WinShared which will copy new versions into Unity plugins folders
