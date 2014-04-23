using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace MarkerMetro.Unity.WinIntegration
{
    /// <summary>
    /// Handles dispatching to the UI and App Threads
    /// </summary>
    public static class Dispatcher
    {
        private static Action<Action> _invokeOnUIThread = null;

#if !NETFX_CORE
        internal static Assembly AppAssembly;
#endif

        /// <summary>
        /// // needs to be set via the app so we can invoke onto App Thread (see App.xaml.cs)
        /// </summary>
        public static Action<Action> InvokeOnAppThread
        { get; set; }

        /// <summary>
        /// needs to be set via the app so we can invoke onto UI Thread (see App.xaml.cs)
        /// </summary>
        public static Action<Action> InvokeOnUIThread
        { 
            get
            {
                return _invokeOnUIThread;
            }
            set
            {
                _invokeOnUIThread = value;
#if !NETFX_CORE
                AppAssembly = Assembly.GetCallingAssembly();
#endif
            }
        }

        ///// <summary>
        ///// Reference to the calling Windows App Assembly (required by resource helper on Windows Phone)s
        ///// </summary>
        //public static Assembly AppAssembly
        //{ get; set;}
    }
}