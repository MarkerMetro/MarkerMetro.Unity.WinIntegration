using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETFX_CORE
using System.Runtime.InteropServices;
#endif

namespace MarkerMetro.Unity.WinIntegration.Logging
{

//    [Serializable]

    public class WrappedException : Exception
    {
        readonly string _stackTrace;
        public WrappedException() { }
        public WrappedException(string message) : base(message) { }
        public WrappedException(string message, string stackTrace)
            : base(message)
        {
            _stackTrace = stackTrace;
        }

        public override string StackTrace
        {
            get
            {
                if (_stackTrace == null)
                    return base.StackTrace;
                else
                    return _stackTrace;
            }
        }
    }


}
