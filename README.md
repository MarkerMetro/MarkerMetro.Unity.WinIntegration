## Getting Started

Unity Plugin for Windows Universal 8.1, Windows 8.1 and Windows Phone 8.0 used to support implementations of Windows or Windows Phone platform specific functionality missing within Unity.

See the [Unity FAQ on Universal Apps](http://docs.unity3d.com/Manual/WindowsUniversalApps-faq.html) which contains a complete breakdown of the platform conditional compilation you can use with Windows Apps and also broad guidance around the special plugin folders on Windows apps.

## Prerequisites

- Visual Studio 2013
- Unity 4.6.1f1 (tested only on this version but should work on other 4.x builds)

## Getting Latest

### Build Latest from Source

We recommend using the latest stable tagged release, or you can build straight off the head if you are brave.

Configure the solution to Release | Any CPU and Rebuild.

You can then copy the folder contents as follows:

- /MarkerMetro.Unity.WinIntegrationMetro/bin/Release > /Assets/Plugins/Metro/
- /MarkerMetro.Unity.WinIntegrationUnity/bin/Release > /Assets/Plugins/
- /MarkerMetro.Unity.WinIntegrationWP8/bin/Release > /Assets/Plugins/WP8/

### Download Latest Stable Binaries

Alternatively, you can download latest from [Nuget](https://www.nuget.org/api/v2/package/MarkerMetro.Unity.WinIntegration)

Extract the files from the package and copy the folder contents as follows:

- /lib/netcore45/ > /Assets/Plugins/Metro/
- /lib/net35 > /Assets/Plugins/
- /lib/windowsphone8 > /Assets/Plugins/WP8/

Note: The Metro output will work fine for Universal projects with both Windows 8.1 and Windows Phone 8.1

## Initialize the Plugin

Within your Windows application, just need to ensure you initialize the plugin appropriately with examples as follows:

For Windows Universal and Windows 8.1 Apps add the following method to App.xaml.cs and call it after the call to appCallbacks.InitializeD3DXAML().

For Windows Phone 8.0 add the following method to MainPage.xaml.cs at the end of DrawingSurfaceBackground_Loaded method within the if (!_unityStartedLoading) code branch at the bottom.

```csharp

void InitializePlugins()
{
    // wire up dispatcher for plugin
    MarkerMetro.Unity.WinIntegration.Dispatcher.InvokeOnAppThread = InvokeOnAppThread;
    MarkerMetro.Unity.WinIntegration.Dispatcher.InvokeOnUIThread = InvokeOnUIThread;
}

```
For Windows Universal and Windows 8.1 Apps the handlers should be as follows:

```csharp
public void InvokeOnAppThread(Action callback)
{
    appCallbacks.InvokeOnAppThread(() => callback(), false);
}

public void InvokeOnUIThread(Action callback)
{
    appCallbacks.InvokeOnUIThread(() => callback(), false);
}
```

For Windows Phone 8.0 Apps the handlers should be as follows:

```csharp

public void InvokeOnAppThread(System.Action callback)
{
    UnityApp.BeginInvoke(() => callback());
}

public void InvokeOnUIThread(System.Action callback)
{
    Dispatcher.BeginInvoke(() => callback());
}
```

You can see existing implementations in [WinShared](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared) here:

- [Windows Universal](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolutionUniversal/UnityProject/UnityProject.Shared/App.xaml.cs) 
- [Windows 8.1](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolution/WindowsStore/UnityProject/App.xaml.cs)
- [Windows Phone 8.0](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolution/WindowsPhone/UnityProject/MainPage.xaml.cs)

## Guidance for Usage

This plugin helps with a number of missing pieces of missing functionality within Unity. Use the Unity APIs if you can, and use WinIntegration where they are missing functionality.

Wherever possible you want to minimize the changes to existing code, therefore we recommend applying a using statement for the platforms you need to provide support for. 

We recommend you look at [WinShared](https://github.com/MarkerMetro/MarkerMetro.Unity.WinIntegration#use-winshared-to-make-things-easier) which uses WinIntegration to demonstrate capabilities.

### Facebook Integration

There is no Unity Plugin for Windows at this time. We have filled the gap by providing the most used functionality from the [Unity SDK for Facebook](https://developers.facebook.com/docs/unity/) in WinIntegration.

#### Windows Phone

Windows Phone (both 8.0 and 8.1) supports uses a native mobile internet explorer approach. For login, this provides a long lived SSO token which is checked and refreshed at most every 24 hours. This eliminates any problems with tokens or cookies expiring, so app request dialogs (also displayed in mobile IE) and graph calls will work without issues. 

Modify the app's manifest to add a protocal handler to ensure the native mobile IE facebook integration can work. 

- [Windows Phone 8.0](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolution/WindowsPhone/UnityProject/Properties/WMAppManifest.xml#L56)
- [Windows Phone 8.1](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolutionUniversal/UnityProject/UnityProject.WindowsPhone/Package.appxmanifest#L25)

#### Windows 8.1

Windows 8.1 uses a traditional web view approach which we will be looking to ugprade to in the future release if and when possible. 

##### Adding and initializing a web view

Add a web view control to your app to handle the facebook integration. [See example here](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/tree/master/WindowsSolutionUniversal/UnityProject/UnityProject.Shared/Controls) which you can customize to your requirements.

You also need to provide an instance of the web view to WinIntegration so that the facebook integration can work.

- [Windows 8.1 Universal](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolutionUniversal/UnityProject/UnityProject.Shared/MainPage.xaml.cs#L81)
- [Windows 8.1 Non Universal](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolution/Common/CommonMainPage.cs#L69)

#### Sample usage

We aim to mimic the [Unity SDK for Facebook](https://developers.facebook.com/docs/unity/) functionality as much as possible with a wrapper/facade class. 

For a complete implementation of Facebook Integration using WinIntegration check out our starter template  [MarkerMetro.Unity.WinShared](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared). 

To begin, add using statements to include the Facebook APIs as follows within your Unity script.

```csharp
#if UNITY_WINRT && !UNITY_EDITOR
using MarkerMetro.Unity.WinIntegration.Facebook;

#if (UNITY_WP8 || UNITY_WP_8_1) && !UNITY_EDITOR
using FBWin = MarkerMetro.Unity.WinIntegration.Facebook.FBNative;
#else
using FBWin = MarkerMetro.Unity.WinIntegration.Facebook.FB;
#endif

#endif
```

Facebook implementations are quite game specific, however you will always need to initialize the FB client with your App ID.

Here's an example of the basic init call:

```csharp
FBWin.Init(fbInitComplete, "[api key]", fbOnHideUnity); 

private void fbInitComplete()
{
    // handler for Unity to deal with FB initialization complete
}

private void fbOnHideUnity(bool isShow)
{
    // handler for UNity to deal with FB web browser visibility changes (only applies to Win 8.1)
}

```

Note: a redirect url may need to be explicitly passed in if the default FB.Init call does not work and you get "Given URL is not allowed by the Application configuration". In this case, the you will need to provide a valid redirectUrl via their facebook app page. This is at > Settings > Advanced > Valid OAuth redirect URIs within Facebook App. This value should NOT be a local address (e.g .http://localhost...) as that causes problems on Windows 8.1. It doesn't really matter what it is, just that it's a valid url.

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

For Windows 8.1, note that only the message and callback are supported at this time. For Windows Phone  message, to, data and title parameters are supported. The other parameters are included to provide API parity with the Unity Facebook SDK, but are not functional at this time.

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

For both Windows 8.1 and Windows Phone, note that only the toId, link, linkName, linkDescription and picture parameters are supported at this time, for Windows Phone message, to, data and title parameters are supported. The other parameters are included to provide API parity with the Unity Facebook sdk, but are not functional.

Note that on Windows Phone the app actually deactivates and resumes as it hands off to mobile IE for all facebook integration, however the callback will still fire on resume.

Lastly note, the FB.cs and FBNative.cs classes in WinIntegration are very similar and we are working on aligning more closely. It is expected that FB.cs will be fully deprecated after Windows 8.1 native mobile IE facebook integration is working.

### Store Integration

There is a single Store API for both Windows 8.1 and Windows Phone 8.x.

For a complete implementation of IAP Integration using WinIntegration check out our starter template  [MarkerMetro.Unity.WinShared](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared)

#### Setup and Initialization

Add an iapsimulator.xml file to the root of your project. This will be used when the store manager is in simulator mode.

- [Windows 8.1 and Windows Universal] (https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolutionUniversal/UnityProject/UnityProject.Shared/iap_simulator.xml)
- [Windows Phone 8.0](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared/blob/master/WindowsSolution/WindowsPhone/UnityProject/iap_simulator.xml)

Then ensure you have initialized the store with the following code

```csharp
            //Initialise Store system
#if QA || DEBUG
            StoreManager.Instance.Initialise(true);
#else
            StoreManager.Instance.Initialise(false);
#endif
```

For Windows Phone 8.0, place it in the Unity_Loaded method in MainPage.xaml.cs. 

For Windows Universal or Windows 8.1, place it in the InitializeUnity method within App.xaml.cs just before the call to construct MainPage.

Note the practice of using a conditional compilation symbol so that you can have IAP simulator or real store api interaction depending on which build you are delivering. 

#### Sample usage

To begin, add a using statement to include the Store APIs.

```csharp
using MarkerMetro.Unity.WinIntegration.Store;
```

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

Specifically for Windows Phone 8.0, the only other StatusCode used is NotReady when after a successful purchase the license information does not appear to be valid. Windows 8.1 uses all the other status codes as more information is available.

```csharp
void StoreManager.Instance.PurchaseProduct(PurchaseDelegate callback)
```
### Exception Logging (TBC)

WinIntegration supports both [Raygun.io](https://raygun.io/) and Bugsense via the ExceptionLogger class.

This is enabled via MarkerMetro.Unity.WinIntegration.ExceptionLogger. Integration is disabled by default. 

#### To enable exception logging

Go straight to 3. if you have an api key provided by the client.

1. Create a new project on the Exception Logger portal (e.g Raygun/Bugsense)
2. Get **API Key** from the Exception Logger portal
3. Replace the **API Key** in /WindowsSolution/Common/CommonApp.InitializeExceptionLogger() method and uncomment the lines.
4. In _Unity_ attach /Assets/Scripts/MarkerMetro/ExceptionLogger.cs to first object that starts, this will allow reporting of _Unity_ errors using 

#### To disable exception logging

Comment out the line to initialize the ExceptionLogger here: /WindowsSolution/Common/CommonApp.InitializeExceptionLogger()

#### To remove exception logging libraries

By default, binaries for the exception loggers will be included when you update WinIntegration from Nuget script. You should do the following to ensure that these binaries are not included. 

These steps are currently in development
 
#### Testing exceptions/crashes

In _WinShared_ project there are 3 locations from which test exceptions can be thrown. 

1. **Windows Store** project has extra Settings charm menu item **Crash** (only for Debug)
2. **Windows Phone** project has AppBar to allow crash testing (only for Debug)
3. **Unity** project has extra button in `UIStart.cs` in /Assets/WinIntegrationExample/FaceFlip.unity test scene in WinShared.

### Local Notifications (TBC)

Reminders can be managedf using LocalNotifications.ReminderManager.

ReminderManage will use system reminders on WP8, and scheduled notification toasts on Win8.1 allowing you to set deterministic timer based prompts to the user.

#### Usage Guidelines

- Win 8.1 Ensure that you have enabled "Is Toast Capable" in your manifest
- Add an option in settings screen to disable reminders
- Win 8.1 Add toggle in settings charm to disable reminders

### Video Player (TBC)

### Helper

Add a using statement to include the  APIs.

```csharp
using MarkerMetro.Unity.WinIntegration;
```

This class will help with various things such as gettign app version, the device id, store url and determining whether the device is low end. See code documentation for more details

https://github.com/MarkerMetro/MarkerMetro.Unity.WinIntegration/blob/master/SharedSource/Helper.cs


## Use WinShared to make things easier

If you are starting a new port and/or you want the best ongoing Unity integration with WinLegacy and related plugins, consider [MarkerMetro.Unity.WinShared](https://github.com/MarkerMetro/MarkerMetro.Unity.WinShared). 

This will provide features such as:

- Initialization included within Windows projects provided
- Test scene demonstrating end to end all the WinIntegration features above, including Facebook and IAP integration.
- Unity menu integration allowing you to get the latest stable version automatically from [Nuget](https://www.nuget.org/packages/MarkerMetro.Unity.WinLegacy/)
- Unity menu integration for using local solution with automatic copy of build output into correct Unity plugin folders

## Please Contribute

We're open source, so please help everyone out by [contributing](CONTRIBUTING.md) as much as you can.
