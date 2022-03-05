/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace TempManager
{
    public class TempManager : IDisposable
    {
        private readonly List<string> tempPaths = new();
        private bool disposed = false;
        private readonly string Temp = Environment.GetEnvironmentVariable("TEMP") ?? "";

        public TempManager()
        {

        }

        public TempManager(string Temp)
        {
            if (!string.IsNullOrEmpty(Temp))
            {
                this.Temp = Temp;
            }
        }

        public string GetTempPath()
        {
            string fullpath;
            string path;

            do
            {
                path = Path.GetTempFileName();
                fullpath = Path.Combine(Temp, Path.GetFileName(path));
            }
            while (File.Exists(fullpath));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            tempPaths.Add(fullpath);
            return fullpath;
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

                foreach (string path in tempPaths)
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
