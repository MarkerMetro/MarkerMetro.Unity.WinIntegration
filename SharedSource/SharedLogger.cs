using MarkerMetro.Unity.WinIntegration.Resources;
using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using System.Collections;
#elif WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Phone.Info;
using Windows.ApplicationModel.Store;
using Windows.Networking.Connectivity;
#endif

namespace MarkerMetro.Unity.WinIntegration
{
    /// <summary>
    /// Shared Logger for Web Error Logging
    /// </summary>
    /// <remarks>
    /// Derive and initialize <see cref="Instance"/> with implementation connected to, for example Raygun.io
    /// See <see cref="RaygunSharedLogger"/> in WinShared project
    /// </remarks>
    public abstract class SharedLogger
    {
        static readonly object _sync = new object();

        static SharedLogger _instance;
        public static SharedLogger Instance
        {
            get 
            {
                lock (_sync)
                {
                    Debug.Assert(_instance!=null, "You must initialize SharedLogger before calling SharedLogger");

                    if(_instance==null)
                        throw new InvalidOperationException("You must initialize SharedLogger before calling SharedLogger");

                    return _instance;
                }
            }
            set
            {
                lock (_sync)
                {
                    _instance = value;
                }
            }
        }


        public abstract void Send(Exception ex);

        public abstract void Send(string message, string stackTrace);
    }
}
