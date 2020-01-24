
namespace SocketApp
{
    class Program
    {
        static void Main(string[] args)
        {
            UnitTest tester = new UnitTest();
            tester.Test();
            UserConsole console = new UserConsole();
            console.ConsoleEntry(args);
        }
    }
}
