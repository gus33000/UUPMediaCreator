using System;
using System.Collections.Generic;
using System.IO;

namespace TempManager
{
    public class TempManager : IDisposable
    {
        private List<string> tempPaths = new List<string>();
        private bool disposed = false;
        public static TempManager Instance = new TempManager();

        public string GetTempPath()
        {
            var path = Path.GetTempFileName();
            if (File.Exists(path))
                File.Delete(path);
            tempPaths.Add(path);
            return path;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {

                }

                foreach (var path in tempPaths)
                {
                    if (Directory.Exists(path))
                    {
                        try
                        {
                            Directory.Delete(path, true);
                        }
                        catch { }
                    }
                    else if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch { }
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~TempManager()
        {
            Dispose(false);
        }
    }
}
