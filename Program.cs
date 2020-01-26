
namespace SocketApp
{
    class Program
    {
        static void Main(string[] args)
        {
            UnitTest tester = new UnitTest();
            tester.Test();
            SockController controller = new SockController();
            UserConsole console = new UserConsole(controller);
            console.ConsoleEntry(args);
        }
    }
}
