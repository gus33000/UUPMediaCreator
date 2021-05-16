using System;
using System.Threading;

namespace UUPMediaCreator.Broker
{
    internal static class Program
    {
        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Mutex mutex = null;
            if (!Mutex.TryOpenExisting("UUPMediaCreatorMutex", out mutex))
            {
                mutex = new Mutex(false, "UUPMediaCreatorMutex");
                WinRT.ComWrappersSupport.InitializeComWrappers();
                _ = new UUPMediaCreatorApplicationContext();
                _Shutdown.WaitOne();
                mutex.Close();
            }
        }
    }
}