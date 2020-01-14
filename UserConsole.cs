using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketApp
{
    public class UserConsole
    {
        List<SockMgr> _connections = new List<SockMgr>();
        SockFactory _factory = new SockFactory();
        SocketRole _role;
        public void ConsoleEntry(string[] args)
        {
            _factory.AcceptEvent += OnAcceptEvent;

            Console.WriteLine("Server or Client?");
            Console.WriteLine("1. Server");
            Console.WriteLine("2. Client");
            Console.Write("> ");
            string choice = Console.ReadLine();
            if (choice.Contains("1"))  // as server
            {
                _role = SocketRole.Server;
                _factory.SetConfig("0.0.0.0", 11000);
                Socket listener = _factory.GetTcpListener();

                _factory.ServerAccept(listener);
            }
            else  // as client
            {
                _role = SocketRole.Client;
            }

            while (true)
            {
                GeneralConcole();
            }
        }

        void OnAcceptEvent(object sender, AcceptEventArgs e)
        {
            _connections.Add(e.Handler);
            e.Handler.SocketShutdownEvent += OnSocketShutdown;
        }

        void OnSocketShutdown(SockMgr source, SocketShutdownEventArgs e)
        {
            _connections.Remove(source);
        }

        void GeneralConcole()
        {
            Console.WriteLine("[General Console]");
            Console.WriteLine("1. List all Sockets");
            Console.WriteLine("2. Interface mode");
            if (_role == SocketRole.Client)
                Console.WriteLine("3. Add new connection");
            Console.Write("> ");
            string sel = Console.ReadLine();
            int selI = int.Parse(sel);
            switch (selI)
            {
                case 1:
                    int i = 0;
                    foreach (var sockMgr in _connections)
                    {
                        Console.Write(string.Format("{0}\t", i));
                        Console.WriteLine(sockMgr.GetSocket().RemoteEndPoint.ToString());
                        ++i;
                    }
                    break;
                case 2:
                    Console.WriteLine("Enter the index of the client");
                    Console.Write("> ");
                    int index = int.Parse(Console.ReadLine());
                    InterfaceMenu(_connections[index]);
                    break;
                case 3:
                    if (_role == SocketRole.Client)
                    {
                        Console.WriteLine("Enter server IP address");
                        Console.Write("> ");
                        string ipAddr = Console.ReadLine();
                        if (ipAddr == "")
                        {
                            _factory.SetConfig("127.0.0.1", 11000);
                        }
                        else
                        {
                            Console.WriteLine("Enter server port");
                            Console.Write("> ");
                            int port = int.Parse(Console.ReadLine());
                            _factory.SetConfig(ipAddr, port);
                        }

                        SockMgr client = _factory.GetTcpClient();

                        // remaining
                        client.SocketShutdownEvent += OnSocketShutdown;
                        _connections.Add(client);
                    }
                    break;
                default:
                    break;
            }
        }

        static void InterfaceMenu(SockMgr sockMgr)
        {
            Console.WriteLine("[Interface Menu]");
            Console.WriteLine("1. Send");

            Console.Write("> ");
            string sel = Console.ReadLine();
            int selI = int.Parse(sel);
            switch (selI)
            {
                case 1:
                    SendConsole(sockMgr);
                    break;
                default:
                    break;
            }
        }

        static void SendConsole(SockMgr sockMgr)
        {
            Console.WriteLine("Enter message to send");
            Console.Write("> ");
            string msg = Console.ReadLine();
            sockMgr.Send(msg);
        }
    }
}
