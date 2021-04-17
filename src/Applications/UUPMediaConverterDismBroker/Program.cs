using MediaCreationLib.Dism;
using System;

namespace UUPMediaCreator.DismBroker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            switch (args[0])
            {
                case "/PECompUninst":
                    {
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
            }
        }
    }
}