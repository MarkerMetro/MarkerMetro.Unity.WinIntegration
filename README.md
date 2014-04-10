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
