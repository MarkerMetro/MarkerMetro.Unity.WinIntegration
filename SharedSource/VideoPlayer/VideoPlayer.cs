using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

#elif NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#endif

namespace MarkerMetro.Unity.WinIntegration.VideoPlayer
{
    public static class VideoPlayer
    {
        /// <summary>
        /// Get the state of Video is currently playing or not
        /// </summary>
        public static bool IsPlaying { get; private set; }

#if WINDOWS_PHONE || NETFX_CORE
        /// <summary>
        /// Callback on video ended
        /// </summary>
        private static Action _onVideoEnded;

        private static MediaElement _videoElement;
        private static Popup _videoPopup;
#endif

        /// <summary>
        /// Initialize VideoPlayer
        /// </summary>
        public static void Initialize()
        {
#if WINDOWS_PHONE || NETFX_CORE
            _videoPopup = new Popup();

            _videoPopup.VerticalOffset = 0;
            _videoPopup.HorizontalOffset = 0;

            _videoElement = new MediaElement();
            _videoPopup.Child = _videoElement;

            _videoElement.MediaEnded += _videoElement_MediaEnded;
            _videoElement.MediaOpened += _videoElement_MediaOpened;

#if WINDOWS_PHONE
            _videoPopup.Height = Application.Current.Host.Content.ActualHeight;
            _videoPopup.Width = Application.Current.Host.Content.ActualWidth;

            _videoElement.Tap += _videoElement_Tap;
            _videoElement.Stretch = Stretch.Fill;
#else
            _videoPopup.Height = Window.Current.Bounds.Height;
            _videoPopup.Width = Window.Current.Bounds.Width;

            _videoElement.Tapped += _videoElement_Tapped;
            _videoElement.Stretch = Stretch.Uniform;
#endif

            _videoElement.AutoPlay = false;

            _videoElement.Height = _videoPopup.Height;
            _videoElement.Width = _videoPopup.Width;

            _videoElement.Visibility = Visibility.Collapsed;
#endif
        }

#if WINDOWS_PHONE || NETFX_CORE
        /// <summary>
        /// Event handler for user tap when video is playing
        /// </summary>
#if WINDOWS_PHONE
        static void _videoElement_Tap(object sender, System.Windows.Input.GestureEventArgs e)
#else
        static void _videoElement_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
#endif
        {
            StopVideo();
        }

        static void _videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            _videoElement.Play();
        }

        static void _videoElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }
#endif

        /// <summary>
        /// Plays the video given path of the file
        /// </summary>
        public static void PlayVideo(string filename, Action onVideoEnded)
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (_videoPopup == null || _videoElement == null)
            {
                throw new Exception("VideoPlayer not initialized");
            }

            IsPlaying = true;
            _videoElement.Visibility = Visibility.Visible;
            _videoPopup.IsOpen = true;

#if NETFX_CORE
            _videoPopup.Height = Window.Current.Bounds.Height;
            _videoPopup.Width = Window.Current.Bounds.Width;
            _videoElement.Height = _videoPopup.Height;
            _videoElement.Width = _videoPopup.Width;
#endif

            _onVideoEnded = onVideoEnded;
            _videoElement.Source = new Uri(filename, UriKind.Absolute);
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Stop playing the video
        /// </summary>
        public static void StopVideo ()
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (_videoPopup == null || _videoElement == null)
            {
                throw new Exception("VideoPlayer not initialized");
            }

            _videoElement.Stop();
            _videoElement.Visibility = Visibility.Collapsed;

            if (_onVideoEnded != null)
                _onVideoEnded();

            IsPlaying = false;
            _videoPopup.IsOpen = false;
#else
            throw new PlatformNotSupportedException("");
#endif
        }
    }
}
