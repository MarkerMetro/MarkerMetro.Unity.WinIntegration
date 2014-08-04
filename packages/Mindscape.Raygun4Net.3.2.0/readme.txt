Raygun4Net - Raygun.io Provider for .NET Framework
===================

=========================================================================================================
|                                         ! IMPORTANT CHANGE IN 3.0 !                                   |
|                                                                                                       |
| The ignoreFormDataNames configuration setting has been removed and replaced with 4 separate options:  |
|                                                                                                       |
| ignoreFormFieldNames                                                                                  |
| ignoreHeaderNames                                                                                     |
| ignoreCookieNames                                                                                     |
| ignoreServerVariableNames                                                                             |
|                                                                                                       |
| If you were once using the ignoreFormDataNames option to ignore headers, cookies or server variables, |
| make sure to use the new correct configuration options to ignore the appropriate bit.                 |
=========================================================================================================

Where is my app API key?
====================
When you create a new application on your Raygun.io dashboard, your app API key is displayed at the top of the instructions page.
You can also find the API key by clicking the "Application Settings" button in the side bar of the Raygun.io dashboard.

Namespace
====================
The main classes can be found in the Mindscape.Raygun4Net namespace.

Supported platforms/frameworks
====================

Projects built with the following frameworks are supported:

* .NET 2.0, 3.5, 4.0, 4.5
* ASP.NET
* MVC
* Web Api
* WinForms, WPF etc
* Windows Store apps (universal) for Windows 8.1 and Windows Phone 8.1
* Windows 8
* Windows Phone 7.1 and 8
* WinRT
* Xamarin.iOS and Xamarin.Android

The NuGet package will select the appropriate dll to use for your project.

Usage
====================

The Raygun4Net provider includes support for many .NET frameworks.
Scroll down to find information about using Raygun for your type of application.

ASP.NET
====================
Add a section to configSections:

<section name="RaygunSettings" type="Mindscape.Raygun4Net.RaygunSettings, Mindscape.Raygun4Net"/>

Add the Raygun settings configuration block from above:

<RaygunSettings apikey="YOUR_APP_API_KEY" />

You can then either create a new instance of the RaygunClient class and call Send(Exception) e.g.

protected void Application_Error()
{
  var exception = Server.GetLastError();
  new RaygunClient().Send(exception);
}

Or there is an HttpModule you can add.

For system.web:

<httpModules>
  <add name="RaygunErrorModule" type="Mindscape.Raygun4Net.RaygunHttpModule"/>
</httpModules>

For system.webServer:

<modules>
  <add name="RaygunErrorModule" type="Mindscape.Raygun4Net.RaygunHttpModule"/>
</modules>

Additional ASP.NET configuration options
========================================

Exclude errors by HTTP status code

If using the HTTP module then you can exclude errors by their HTTP status code by providing a comma separated list of status codes to ignore in the configuration. For example if you wanted to exclude errors that return the [I'm a teapot](http://tools.ietf.org/html/rfc2324) response code, you could use the configuration below.

<RaygunSettings apikey="YOUR_APP_API_KEY" excludeHttpStatusCodes="418" />

Exclude errors that originate from a local origin

Toggle this boolean and the HTTP module will not send errors to Raygun.io if the request originated from a local origin. ie. A way to prevent local debug/development from notifying Raygun without having to resort to Web.config transforms.

<RaygunSettings apikey="YOUR_APP_API_KEY" excludeErrorsFromLocal="true" />

Remove sensitive request data

If you have sensitive data in an HTTP request that you wish to prevent being transmitted to Raygun, you can provide lists of possible keys (names) to remove.
Keys to ignore can be specified on the RaygunSettings tag in web.config, (or you can use the equivalent methods on RaygunClient if you are setting things up in code).
The available options are:

ignoreFormFieldNames
ignoreHeaderNames
ignoreCookieNames
ignoreServerVariableNames

These can be set to be a comma separated list of keys to ignore. Setting an option as * will indicate that all the keys will not be sent to Raygun.
Placing * before, after or at both ends of a key will perform an ends-with, starts-with or contains operation respectively.
For example, ignoreFormFieldNames="*password*" will cause Raygun to ignore all form fields that contain "password" anywhere in the name.
These options are not case sensitive.

Remove wrapper exceptions (available in all .NET Raygun providers)

If you have common outer exceptions that wrap a valuable inner exception which you'd prefer to group by, you can specify these by using the multi-parameter method:

raygunClient.AddWrapperExceptions(typeof(TargetInvocationException));

In this case, if a TargetInvocationException occurs, it will be removed and replaced with the actual InnerException that was the cause. Note that HttpUnhandledException and the above TargetInvocationException are already defined; you do not have to add these manually. This method is useful if you have your own custom wrapper exceptions, or a framework is throwing exceptions using its own wrapper.

MVC
====================

If you are installing manually, make sure you are using the Mindscape.Raygun4Net.dll that targets either .Net 4.0 or .Net 4.5. These can be located in the Net4 and Net45 folders if you downloaded the assemblies from GitHub.
If you are installing via NuGet, the appropriate dll will be selected for you.

Setting up Raygun4Net in an MVC project is exactly the same as with ASP.NET as described in the section above.

Web Api
====================

If you are installing manually, make sure you are using the Mindscape.Raygun4Net.dll that targets .Net 4.5. This can be located in the Net45 folder if you downloaded the assemblies from GitHub.
If you are installing via NuGet, the appropriate dll will be selected for you.

Use RaygunSettings to provide your API key in the same way as with ASP.NET as described in the section above.

Then, in the WebApiConfig.Register method of your project, simply call the static RaygunWebApiClient.Attach method:

RaygunWebApiClient.Attach(config);

This method takes an optional function that you can use to provide a custom instance of RaygunWebApiClient. This custom instance could be used to listen to the SendingMessage event, or set any of the various options.

WPF
====================
Create an instance of RaygunClient by passing your app API key in the constructor.
Attach an event handler to the DispatcherUnhandledException event of your application.
In the event handler, use the RaygunClient.Send method to send the Exception.

private RaygunClient _client = new RaygunClient("YOUR_APP_API_KEY");

public App()
{
  DispatcherUnhandledException += OnDispatcherUnhandledException;
}

void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
{
  _client.Send(e.Exception);
}

WinForms
====================
Create an instance of RaygunClient by passing your app API key in the constructor.
Attach an event handler to the Application.ThreadException event BEFORE calling Application.Run(...).
In the event handler, use the RaygunClient.Send method to send the Exception.

private static readonly RaygunClient _raygunClient = new RaygunClient("YOUR_APP_API_KEY");

[STAThread]
static void Main()
{
  Application.EnableVisualStyles();
  Application.SetCompatibleTextRenderingDefault(false);

  Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

  Application.Run(new Form1());
}

private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
{
  _raygunClient.Send(e.Exception);
}

Windows Store Apps (Windows 8.1 and Windows Phone 8.1)
====================

In the App.xaml.cs constructor (or any central entry point in your application), call the static RaygunClient.Attach method using your API key. This will catch and send all unhandled exception to Raygun.io for you.

public App()
{
  RaygunClient.Attach("YOUR_APP_API_KEY");
}

At any point after calling the Attach method, you can use RaygunClient.Current to get the static instance. This can be used for manually sending messages (via the Send methods) or changing options such as the User identity string.

You can manually send exceptions with the SendAsync method. When manually sending, currently the compiler does not allow you to use `await` in a catch block. You can however call SendAsync in a blocking way:

try
{
  throw new Exception("foo");
}
catch (Exception e)
{
  RaygunClient.Current.SendAsync(e);
}

WinRT
====================
In the App.xaml.cs constructor (or any main entry point to your application), call the static RaygunClient.Attach method using your API key.

public App()
{
  RaygunClient.Attach("YOUR_APP_API_KEY");
}

At any point after calling the Attach method, you can use RaygunClient.Current to get the static instance. This can be used for manually sending messages (via the Send methods) or changing options such as the User identity string.

Limitations of WinRT UnhandledException event and Wrap() workarounds
====================
The options available in WinRT for catching unhandled exceptions at this point in time are more limited
compared to the options in the more mature .NET framework. The UnhandledException event will be raised when
invalid XAML is parsed, in addition to other runtime exceptions that happen on the main UI thread. While
many errors will be picked up this way and therefore be able to be sent to Raygun, others will be missed by
this exception handler. In particular asynchronous code or Tasks that execute on background threads will
not have their exceptions caught.

A workaround for this issue is provided with the Wrap() method. These allow you to pass the code you want
to execute to an instance of the Raygun client - it will simply call it surrounded by a try-catch block.
If the method you pass in does result in an exception being thrown this will be transmitted to Raygun, and
the exception will again be thrown. Two overloads are available; one for methods that return void and
another for methods that return an object.

Windows Phone 7.1 and 8
=======================
In the App.xaml.cs constructor (or any main entry point to your application), call the static RaygunClient.Attach method using your API key.

RaygunClient.Attach("YOUR_APP_API_KEY");

At any point after calling the Attach method, you can use RaygunClient.Current to get the static instance. This can be used for manually sending messages (via the Send methods) or changing options such as the User identity string.

Xamarin for Android
====================
In the main/entry Activity of your application, use the static RaygunClient.Attach method using your app API key.
There is also an overload for the Attach method that lets you pass in a user-identity string which is useful for tracking affected users in your Raygun.io dashboard.

RaygunClient.Attach("YOUR_APP_API_KEY");

At any point after calling the Attach method, you can use RaygunClient.Current to get the static instance. This can be used for manually sending messages or changing options such as the User identity string.

Xamarin for iOS
====================
In the main entry point of the application, use the static RaygunClient.Attach method using your app API key.
There is also an overload for the Attach method that lets you pass in a user-identity string which is useful for tracking affected users in your Raygun.io dashboard.

static void Main (string[] args)
{
  RaygunClient.Attach("YOUR_APP_API_KEY");

  UIApplication.Main (args, null, "AppDelegate");
}

At any point after calling the Attach method, you can use RaygunClient.Current to get the static instance. This can be used for manually sending messages or changing options such as the User identity string.

Unique (affected) user tracking
================================

There is a property named *User* on RaygunClient which you can set to be the current user's ID or email address. This allows you to see the count of affected users for each error in the Raygun dashboard. If you provide an email address, and the user has an associated Gravatar, you will see their avatar in the error instance page.

This feature is optional if you wish to disable it for privacy concerns.

Version numbering and tags
==========================

* If you are plugging this provider into a classic .NET application, the version number that will be transmitted will be the AssemblyVersion. If you need to provide your own custom version value, you can do so by setting the ApplicationVersion property of the RaygunClient (in the format x.x.x.x where x is a postive integer).

* If you are using WinRT, the transmitted version number will be that of the Windows Store package, set in in Package.appxmanifest (under Packaging).

* You can also set an arbitrary number of tags (as an array of strings), i.e. for tagging builds. This is optional and will be transmitted in addition to the version number above.

Custom data
===========

Providing additional name-value custom data is also available as an overload on Send().


====================
Troubleshooting
====================
If the solution fails to build due to missing dependencies (Newtonsoft etc), in Visual Studio 2012 ensure
you have the NuGet extension installed and that the Tools -> Options -> Package Manager ->
'Allow Nuget to download missing packages during build' box is checked. Then, go to the directory that
you cloned this repository into and run build.bat.
