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
using MediaCreationLib.Dism;
using System;

namespace UUPMediaCreator.DismBroker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return 2;
            }

            switch (args[0])
            {
                case "/PECompUninst":
                    {
                        if (args.Length < 2)
                        {
                            return 2;
                        }

                        static void callback(bool IsIndeterminate, int Percentage, string Operation)
                        {
                            if (!IsIndeterminate)
                            {
                                Console.WriteLine(Percentage + "," + Operation);
                            }
                        }

                        DismOperations.UninstallPEComponents(args[1], callback);
                        break;
                    }
                case "/SetTargetEdition":
                    {
                        break;
                    }
                case "/InstallAppXWorkload":
                    {
                        if (args.Length < 4)
                        {
                            return 2;
                        }

                        if (!DismOperations.PerformAppxWorkloadInstallation(args[1], args[2], AppxInstallWorkload.FromString(args[3])))
                        {
                            return 1;
                        }
                        break;
                    }
            }

            return 0;
        }
    }
}