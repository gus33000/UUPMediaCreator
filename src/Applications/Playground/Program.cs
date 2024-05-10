namespace Playground
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new Windows.Internal.Flighting.PlatformCTAC("WU_OS", "10.0.26100.1").UriQuery);
        }
    }
}