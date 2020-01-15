using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    public class UserConsole
    {
        List<SockMgr> _clients = new List<SockMgr>();
        List<SockMgr> _listeners = new List<SockMgr>();
        SockFactory _factory = new SockFactory();
        public void ConsoleEntry(string[] args)
        {
            _factory.AcceptEvent += OnAcceptEvent;
            _factory.SetLists(_clients, _listeners);
            GeneralConcole();
        }

        void OnAcceptEvent(object sender, AcceptEventArgs e)
        {
            _clients.Add(e.Handler);
            e.Handler.SocketShutdownBeginEvent += OnSocketShutdownBegin;
        }

        void OnSocketShutdownBegin(SockMgr source, SocketShutdownBeginEventArgs e)
        {
            if (source.Role == SocketRole.Listener)
                _listeners.Remove(source);
            else
                _clients.Remove(source);
        }

        void GeneralConcole()
        {
            bool isExit = false;
            while (!isExit)
            {
                Console.WriteLine("[General Console]");
                Console.WriteLine("1. List all clients");
                Console.WriteLine("2. Interface mode");
                Console.WriteLine("3. Establish new connection");
                Console.WriteLine("4. Shutdown all");
                Console.WriteLine("5. List all Listeners");
                Console.WriteLine("6. Build new Listener");
                Console.WriteLine("7. Listener mode");  // manage listeners
                Console.WriteLine("8. Exit");
                Console.Write("> ");
                string sel = Console.ReadLine();
                switch (sel)
                {
                    case "1":
                        ListAllClients();
                        break;
                    case "2":  // manage client
                        Console.WriteLine("Enter the index of the client");
                        Console.Write("> ");
                        int index = int.Parse(Console.ReadLine());
                        InterfaceMenu(_clients[index]);
                        break;
                    case "3":  // Establish new connection
                        BuildClientConsole();
                        break;
                    case "4":
                        ShutdownAll();
                        break;
                    case "5":
                        ListAllListeners();
                        break;
                    case "6":
                        BuildListenerConsole();
                        break;
                    case "7":
                        Console.WriteLine("Enter the index of the listener");
                        Console.Write("> ");
                        int listenerIndex = int.Parse(Console.ReadLine());
                        ListenerMenu(_listeners[listenerIndex]);
                        break;
                    case "8":
                        ShutdownAll();
                        isExit = true;
                        break;
                    default:
                        break;
                }
            }
        }

        // shutdown all listeners and clients
        void ShutdownAll()
        {
            while (_clients.Count > 0)
            {
                _clients[0].Shutdown();
            }
            while (_listeners.Count > 0)
            {
                _listeners[0].Shutdown();
            }
        }

        void ListAllClients()
        {
            int i = 0;
            foreach (var sockMgr in _clients)
            {
                Console.Write(string.Format("{0}\t", i));
                Console.Write(string.Format("{0}\t", sockMgr.IsHost.ToString()));  // if it is a host
                Console.WriteLine(string.Format("{0}\t->\t{1}",
                    sockMgr.GetSocket().LocalEndPoint.ToString(),
                    sockMgr.GetSocket().RemoteEndPoint.ToString()));
                ++i;
            }
        }

        void ListAllListeners()
        {
            int i = 0;
            foreach (var sockMgr in _listeners)
            {
                Console.Write(string.Format("{0}\t", i));
                Console.WriteLine(sockMgr.GetSocket().LocalEndPoint.ToString());
                ++i;
            }
        }

        void BuildListenerConsole()
        {
            Console.WriteLine("[Build Listener]");
            Console.WriteLine("1. TCP Server");
            Console.WriteLine("2. UDP Server");

            Console.Write("> ");
            string sel = Console.ReadLine();

            Console.WriteLine("Enter listening IP address");
            Console.Write("> ");
            string localIpAddr = Console.ReadLine();
            if (localIpAddr == "")
                localIpAddr = "0.0.0.0";
            Console.WriteLine("Enter server port");
            Console.Write("> ");
            string localPortStr = Console.ReadLine();
            int localPort;
            if (localPortStr == "")
                localPort = 11000;
            else
                localPort = int.Parse(localPortStr);

            switch (sel)
            {
                case "1":
                    _factory.SetConfig(localIpAddr, localPort);
                    SockMgr listenerMgr = _factory.GetTcpListener();
                    listenerMgr.SocketShutdownBeginEvent += OnSocketShutdownBegin;
                    _factory.ServerAccept(listenerMgr);
                    _listeners.Add(listenerMgr);
                    break;
                case "2":
                    // TODO
                    break;
                default:
                    break;
            }
        }

        void BuildClientConsole()
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
                int remotePort = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter local port (leave blank for auto)");
                Console.Write("> ");
                string localPortStr = Console.ReadLine();
                try
                {
                    if (localPortStr == "")
                    {
                        _factory.SetConfig(ipAddr, remotePort);
                    }
                    else
                    {
                        _factory.SetConfig(ipAddr, remotePort, int.Parse(localPortStr));
                    }
                }
                catch (SocketException ex)
                {
                    switch ((SocketError)ex.ErrorCode)
                    {
                        case SocketError.InvalidArgument:
                            Console.WriteLine("[Error] An invalid IP address was specified");
                            break;
                        default:
                            Console.WriteLine("[Error] SocketException");
                            break;
                    }
                }
            }
            try
            {
                SockMgr client;
                client = _factory.GetTcpClient();
                // remaining
                client.SocketShutdownBeginEvent += OnSocketShutdownBegin;
                _clients.Add(client);
            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.ConnectionRefused:
                        Console.WriteLine("[Error] Connection Refused. Can not connect because the target machine has actively declined the connection.");
                        break;
                    default:
                        Console.WriteLine("[Error] SocketException");
                        break;
                }
            }
        }

        static void InterfaceMenu(SockMgr sockMgr)
        {
            Console.WriteLine("[Interface Menu]");
            Console.WriteLine("1. Send");
            Console.WriteLine("2. Close");
            Console.WriteLine("3. Is Host?");

            Console.Write("> ");
            string sel = Console.ReadLine();
            switch (sel)
            {
                case "1":
                    SendConsole(sockMgr);
                    break;
                case "2":
                    sockMgr.Shutdown();
                    break;
                case "3":
                    Console.WriteLine(sockMgr.IsHost.ToString());
                    break;
                default:
                    break;
            }
        }

        static void ListenerMenu(SockMgr sockMgr)
        {
            Console.WriteLine("[Interface Menu]");
            Console.WriteLine("1. Close");

            Console.Write("> ");
            string sel = Console.ReadLine();
            switch (sel)
            {
                case "1":
                    sockMgr.Shutdown();
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
