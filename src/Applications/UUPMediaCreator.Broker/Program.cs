using System;
using System.Threading;
using System.Windows.Forms;

namespace UUPMediaCreator.Broker
{
    internal static class Program
    {
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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UUPMediaCreatorApplicationContext());
                mutex.Close();
            }
        }
    }
}