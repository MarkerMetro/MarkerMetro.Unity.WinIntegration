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

Ensure you set the app id in Assets/Plugins/MarkerMetro/Constants.cs

For Windows Phone, ensure you modify the WMAppManifest.xml to change the protocal handler to fb[appid] in the Extensions element. This will ensure the native mobile IE facebook integration can work.

Add using statements to include the Facebook APIs as follows:

```csharp
#if UNITY_WINRT && !UNITY_EDITOR
using MarkerMetro.Unity.WinIntegration.Facebook;

#if UNITY_WP8
using FBWin = MarkerMetro.Unity.WinIntegration.Facebook.FBNative;
#else
using FBWin = MarkerMetro.Unity.WinIntegration.Facebook.FB;
#endif

#endif
```

Windows Phone uses a native mobile internet explorer approach. For login, this provides a long lived SSO token which is checked and refreshed at most every 24 hours. This eliminates any problems with tokens or cookies expiring, so app request dialogs (also displayed in mobile IE) and graph calls will work without issue. 

Windows 8.1 uses a traditional web view approach which we will be looking to ugprade to a  in the future.

Facebook implementations are quite game specific, however you will always need to initialize the FB client, for which you can use the Marker Metro test Faceboook app created by markermetro@live.com facebook account (see \MM Team - Administration\Logins\Facebook accounts.txt" for the password on dropbox).

Here's an example of the basic init call:

```csharp
FBWin.Init(fbInitComplete, "682783485145217", fbOnHideUnity); 

private void fbInitComplete()
{
    // handler for Unity to deal with FB initializati complete
}

private void fbOnHideUnity(bool isShow)
{
    // handler for UNity to deal with FB web browser visibility changes
}

```

Note: a redirect url may need to be explicitly passed in if the default FB.Init call does not work and you get "Given URL is not allowed by the Application configuration". In this case, the client will need to provide a value redirectUrl via their facebook app page. This is at > settings > advanced > Valid OAuth redirect URIs. Client should provide  a value from their we can use, and it should NOT be a local address (e.g .http://localhost...) as that causes problems on Windows Store. It doesn't really matter what it is, just that it's a valid url.

A callback approach is used for login:

```csharp
FBWin.Login("publish_actions", fbResult =>
{
    // Successful login, or deal with errors
});
```

A callback approach is used for app request dialogs:

```csharp
FBWin.AppRequest(
                string message,
                string[] to = null,
                string filters = "",
                string[] excludeIds = null,
                int? maxRecipients = null,
                string data = "",
                string title = "",
                fbResult =>
{
    // Successful app request with friends json, or deal with errors
});
```

For FB.cs, note that only the message and callback are supported at this time, for FBNative message, to, data and title parameters are supported. The other parameters are included to provide api parity with the Unity facebook sdk, but are not functional.

A callback approach is used for feed dialog requests:

```csharp
FBWin.Feed(
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null,
                fbResult =>
{
    // Successful feed request, or deal with errors
});
```

For both FB.cs and FBNative.cs, note that only the toId, link, linkName, linkDescription and picture parameters are supported at this time, for FBNative message, to, data and title parameters are supported. The other paramters are included to provide api parity with the Unity facebook sdk, but are not functional.

Note that on Windows Phone 8 the app actually deactivates and resumes as it hands off to mobile IE for all facebook integration, however the callback will still fire on resume.

For Windows 8.1, it is assumed you will be using MarkerMetro.Unity.WinShared  which includes the necessary web view/browser controls for displaying all necessary facebook dialogs for Window as well as initializing the links between the app and Unity sides. 

The FB and FBNative classes in WinIntegration are very similar and we are working on aligning more closely. It is expected that FB will be fully deprecated after we get Windows 8.1 native mobile IE facebook integration working. 

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
## Exception Logging

WinIntegration supports both [Raygun.io](https://raygun.io/) and Bugsense via the ExceptionLogger class.

This is enabled via MarkerMetro.Unity.WinIntegration.ExceptionLogger. Integration is disabled by default. 

### To enable exception logging

Go straight to 3. if you have an api key provided by the client.

1. Create a new project on the Exception Logger portal (e.g Raygun/Bugsense)
2. Get **API Key** from the Exception Logger portal
3. Replace the **API Key** in /WindowsSolution/Common/CommonApp.InitializeExceptionLogger() method and uncomment the lines.
4. In _Unity_ attach /Assets/Scripts/MarkerMetro/ExceptionLogger.cs to first object that starts, this will allow reporting of _Unity_ errors using 

### To disable exception logging

Comment out the line to initialize the ExceptionLogger here: /WindowsSolution/Common/CommonApp.InitializeExceptionLogger()

### To remove exception logging libraries

By default, binaries for the exception loggers will be included when you update WinIntegration from Nuget script. You should do the following to ensure that these binaries are not included. 

These steps are currently in development
 
### Testing exceptions/crashes

In _WinShared_ project there are 3 locations from which test exceptions can be thrown. 

1. **Windows Store** project has extra Settings charm menu item **Crash** (only for Debug)
2. **Windows Phone** project has AppBar to allow crash testing (only for Debug)
3. **Unity** project has extra button in `UIStart.cs` in /Assets/WinIntegrationExample/FaceFlip.unity test scene in WinShared.

## Local Notifications

Reminders can be managedf using LocalNotifications.ReminderManager.

ReminderManage will use system reminders on WP8, and scheduled notification toasts on Win8.1 allowing you to set deterministic timer based prompts to the user.

Usage Guidelines:

- Win 8.1 Ensure that you have enabled "Is Toast Capable" in your manifest
- Add an option in settings screen to disable reminders
- Win 8.1 Add toggle in settings charm to disable reminders

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
