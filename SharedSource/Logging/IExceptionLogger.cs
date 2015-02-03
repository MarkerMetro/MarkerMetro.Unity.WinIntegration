using System;

namespace MarkerMetro.Unity.WinIntegration
{
    public interface IExceptionLogger
    {
        void Send(string message, string stackTrace);
        void Send(Exception ex);
        bool IsEnabled {get;set;}
    }
}
