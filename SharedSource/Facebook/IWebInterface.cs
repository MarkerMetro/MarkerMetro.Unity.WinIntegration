using System;

namespace MarkerMetro.WinIntegration.Facebook
{
    public delegate void NavigationEventCallback(Uri url, object state);

    public delegate void NavigationErrorCallback(Uri url, int error, object state);

    public interface IWebInterface
    {
        bool IsActive { get; }

        /// <summary>
        /// Bring up the web view and navigate to a page.
        /// </summary>
        /// <param name="uri">The URI to navigate to</param>
        /// <param name="finishedCallback">Called when navigation finishes</param>
        /// <param name="startedCallback">Called when navigation starts</param>
        /// <param name="onError">Called when a navigation error occurs</param>
        void Navigate(
            Uri uri,
            NavigationEventCallback finishedCallback,
            NavigationErrorCallback onError,
            object state = null,
            NavigationEventCallback startedCallback = null
            );

        /// <summary>
        /// Stops navigation, closes the web view and disconnects the callbacks.
        /// </summary>
        void Finish();
    }
}