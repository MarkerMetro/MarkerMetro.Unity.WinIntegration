using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#endif

namespace MarkerMetro.Unity.WinIntegration.VideoPlayer
{
    public enum VideoStretch
    {
        // Summary:
        //     The content preserves its original size.
        None = 0,
        // Summary:
        //     The content is resized to fill the destination dimensions. The aspect ratio
        //     is not preserved.
        Fill = 1,
        // Summary:
        //     The content is resized to fit in the destination dimensions while it preserves
        //     its native aspect ratio.
        Uniform = 2,
        // Summary:
        //     The content is resized to fill the destination dimensions while it preserves
        //     its native aspect ratio. If the aspect ratio of the destination rectangle
        //     differs from the source, the source content is clipped to fit in the destination
        //     dimensions.
        UniformToFill = 3,
    }

    public static class VideoPlayer
    {
        /// <summary>
        /// Get the state of Video is currently playing or not.
        /// </summary>
        public static bool IsPlaying { get; private set; }

#if NETFX_CORE
        /// <summary>
        /// Callback on video ended.
        /// </summary>
        private static Action _onVideoEnded;

        private static MediaElement _videoElement;
        private static Popup _videoPopup;
#endif

        /// <summary>
        /// Initialize VideoPlayer.
        /// </summary>
        private static void Initialize(VideoStretch stretch)
        {
#if NETFX_CORE

            if (_videoPopup == null)
            {
                _videoPopup = new Popup();
            }
            _videoPopup.VerticalOffset = 0;
            _videoPopup.HorizontalOffset = 0;

            if (_videoElement == null)
            {
                _videoElement = new MediaElement();
            }
            _videoPopup.Child = _videoElement;

            _videoElement.MediaEnded += _videoElement_MediaEnded;
            _videoElement.MediaOpened += _videoElement_MediaOpened;

            _videoPopup.Height = Window.Current.Bounds.Height;
            _videoPopup.Width = Window.Current.Bounds.Width;

            _videoElement.Tapped += _videoElement_Tapped;
            _videoElement.Stretch = (Stretch)stretch;

            _videoElement.AutoPlay = false;

            _videoElement.Height = _videoPopup.Height;
            _videoElement.Width = _videoPopup.Width;

            _videoPopup.IsOpen = true;
#endif
        }

#if NETFX_CORE
        /// <summary>
        /// Event handler for user tap when video is playing.
        /// </summary>
        static void _videoElement_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            StopVideoFromUIThread();
        }

        static void _videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            _videoElement.Play();
        }

        static void _videoElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopVideoFromUIThread();
        }
#endif

        /// <summary>
        /// Plays the video given path of the file.
        /// There's a bug causing Video to remain on the screen after it finishes playing on WP8.
        /// Submitted a bug report to Unity: http://fogbugz.unity3d.com/default.asp?663800_4o1v5omb7fan6gfq
        /// </summary>
        public static void PlayVideo(string filename, Action onVideoEnded, VideoStretch stretch = VideoStretch.None)
        {
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
            {
                Initialize(stretch);

                IsPlaying = true;
#if NETFX_CORE
                //_videoPopup.Height = Window.Current.Bounds.Height;
                //_videoPopup.Width = Window.Current.Bounds.Width;
                //_videoElement.Height = _videoPopup.Height;
                //_videoElement.Width = _videoPopup.Width;
#endif

                _onVideoEnded = onVideoEnded;
                _videoElement.Source = new Uri(filename, UriKind.Absolute);
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        /// <summary>
        /// Stop playing the video.
        /// </summary>
        public static void StopVideo ()
        {
#if NETFX_CORE
            Dispatcher.InvokeOnUIThread(() =>
            {
                StopVideoFromUIThread();
            });
#else
            throw new PlatformNotSupportedException("");
#endif
        }

        static void StopVideoFromUIThread ()
        {
#if NETFX_CORE
            if (_videoPopup == null || _videoElement == null)
            {
                throw new Exception("VideoPlayer not initialized.");
            }

            _videoElement.MediaEnded -= _videoElement_MediaEnded;
            _videoElement.MediaOpened -= _videoElement_MediaOpened;
            _videoElement.Tapped -= _videoElement_Tapped;
            _videoElement.Stop();

            IsPlaying = false;

            if (_onVideoEnded != null)
            {
                Dispatcher.InvokeOnAppThread(() =>
                {
                    _onVideoEnded();
                });
            }

            _videoElement.Source = null;
            _videoElement = null;
            _videoPopup.Child = null;
            _videoPopup = null;
#else
            throw new PlatformNotSupportedException("");
#endif
        }
    }
}
