using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace UUPMediaCreator.Broker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mutex = null;
            if (!Mutex.TryOpenExisting("UUPMediaCreatorMutex", out mutex))
            {
                mutex = new Mutex(false, "UUPMediaCreatorMutex");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UUPMediaCreatorApplicationContext());
                mutex.Close();
            }
        }
    }
}
