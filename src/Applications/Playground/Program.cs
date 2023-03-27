namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new Windows.Internal.Flighting.PlatformCTAC("WU_OS", "10.0.22621.1").UriQuery);
        }
    }
}