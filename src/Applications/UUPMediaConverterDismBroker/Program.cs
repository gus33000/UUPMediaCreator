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
using MediaCreationLib.DismOperations;
using MediaCreationLib.Planning.Applications;
using System;

namespace UUPMediaConverterDismBroker
{
    internal static class Program
    {
        private static void callback(bool IsIndeterminate, int Percentage, string Operation)
        {
            if (!IsIndeterminate)
            {
                Console.WriteLine(Percentage + "," + Operation);
            }
        }

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return 2;
            }

            try
            {
                switch (args[0])
                {
                    case "/PECompUninst":
                        {
                            if (args.Length < 2)
                            {
                                return 2;
                            }

                            _ = DismOperations.Instance.UninstallPEComponents(args[1], callback);
                            break;
                        }
                    case "/SetTargetEdition":
                        {
                            break;
                        }
                    case "/InstallAppXWorkload":
                        {
                            if (args.Length < 5)
                            {
                                return 2;
                            }

                            if (!DismOperations.Instance.PerformAppxWorkloadInstallation(args[1], args[2], args[3], System.Text.Json.JsonSerializer.Deserialize<AppxInstallWorkload>(args[4])))
                            {
                                return 3;
                            }
                            break;
                        }
                    case "/InstallAppXWorkloads":
                        {
                            if (args.Length < 5)
                            {
                                return 2;
                            }

                            if (!DismOperations.Instance.PerformAppxWorkloadsInstallation(args[1], args[2], args[3], System.Text.Json.JsonSerializer.Deserialize<AppxInstallWorkload[]>(args[4]), callback))
                            {
                                return 3;
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                callback(true, 0, ex.ToString());
                return 4;
            }

            return 0;
        }
    }
}