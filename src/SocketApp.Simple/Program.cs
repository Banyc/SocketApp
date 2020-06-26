using System;

namespace SocketApp.Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            SockController controller = new SockController();
            UserConsole console = new UserConsole(controller);
            console.ConsoleEntry(args);
        }
    }
}
