using System.IO;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    // Actively raise some operations to socket
    public class UserConsole
    {
        SockList _sockList;
        SockFactory _factory = new SockFactory();
        Protocol.ProtocolFactoryOptions _protocolOptions = new Protocol.ProtocolFactoryOptions();
        public void ConsoleEntry(string[] args)
        {
            _sockList = _factory.GetSockList();
            GeneralConcole();
        }

        // notice: this is a workaround solution
        private void GeneralConcole()
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
                Console.WriteLine("9. Create Keys");
                Console.WriteLine("10. Set Keys");
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
                        InterfaceMenu(_sockList.Clients[index]);
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
                        ListenerMenu(_sockList.Listeners[listenerIndex]);
                        break;
                    case "8":
                        ShutdownAll();
                        isExit = true;
                        break;
                    case "9":
                        KeyGenConsole();
                        break;
                    case "10":
                        SetKeyConsole();
                        break;
                    default:
                        break;
                }
            }
        }

        // shutdown all listeners and clients
        void ShutdownAll()
        {
            while (_sockList.Clients.Count > 0)
            {
                _sockList.Clients[0].Shutdown();
            }
            while (_sockList.Listeners.Count > 0)
            {
                _sockList.Listeners[0].Shutdown();
            }
        }

        void ListAllClients()
        {
            int i = 0;
            foreach (var sockMgr in _sockList.Clients)
            {
                Console.Write(string.Format("{0}\t", i));
                Console.Write(string.Format("{0}\t", sockMgr.GetSockBase().IsHost.ToString()));  // if it is a host
                Console.WriteLine(string.Format("{0}\t->\t{1}",
                    sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
                ++i;
            }
        }

        void ListAllListeners()
        {
            int i = 0;
            foreach (var sockMgr in _sockList.Listeners)
            {
                Console.Write(string.Format("{0}\t", i));
                Console.WriteLine(sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString());
                ++i;
            }
        }

        void BuildListenerConsole()
        {
            // collect config
            Console.WriteLine("[Build Listener]");
            Console.WriteLine("1. TCP Server");
            Console.WriteLine("2. UDP Server");

            Console.Write("> ");
            string sel = Console.ReadLine();

            Console.WriteLine("Enter listening IP address (leave blank for 0.0.0.0)");
            Console.Write("> ");
            string localIpAddr = Console.ReadLine();
            if (localIpAddr == "")
                localIpAddr = "0.0.0.0";
            Console.WriteLine("Enter server port (leave blank for 11000)");
            Console.Write("> ");
            string localPortStr = Console.ReadLine();
            int localPort;
            if (localPortStr == "")
                localPort = 11000;
            else
                localPort = int.Parse(localPortStr);
            // begin to build
            _factory.SetProtocolOptions(_protocolOptions);
            switch (sel)
            {
                case "1":
                    _factory.SetConfig(localIpAddr, localPort);
                    SockMgr listenerMgr = _factory.GetTcpListener();
                    _factory.ServerAccept(listenerMgr);
                    _sockList.Listeners.Add(listenerMgr);
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
            // collecting config
            Console.WriteLine("Enter server IP address (leave blank for 127.0.0.1:11000)");
            Console.Write("> ");
            string ipAddr = Console.ReadLine();
            int timesToTry = -1;
            if (ipAddr == "")
            {
                _factory.SetConfig("127.0.0.1", 11000);
                timesToTry = 1;
            }
            else
            {
                Console.WriteLine("Enter server port");
                Console.Write("> ");
                int remotePort = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter local port (leave blank for auto)");
                Console.Write("> ");
                string localPortStr = Console.ReadLine();
                Console.WriteLine("Enter how many times to try (leave blank for trying once)");
                Console.Write("> ");
                string timesToTryStr = Console.ReadLine();
                if (timesToTryStr == "")
                    timesToTry = 1;
                else
                    timesToTry = int.Parse(timesToTryStr);

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

            // begin to build
            _factory.SetProtocolOptions(_protocolOptions);
            if (timesToTry > 0)
                _factory.BuildTcpClient(timesToTry);
            else
                return;
        }

        static void InterfaceMenu(SockMgr sockMgr)
        {
            bool isExit = false;
            while (!isExit && !sockMgr.IsShutdown)
            {
                Console.WriteLine(string.Format("[Interface Menu] {0} -> {1}",
                    sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
                Console.WriteLine("1. Send");
                Console.WriteLine("2. Close");
                Console.WriteLine("3. Is Host?");
                Console.WriteLine("4. Exit");

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
                        Console.WriteLine(sockMgr.GetSockBase().IsHost.ToString());
                        break;
                    case "4":
                        isExit = true;
                        break;
                    default:
                        break;
                }
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
            sockMgr.SendText(msg);
        }

        static void KeyGenConsole()
        {
            Console.WriteLine("Which?");
            Console.WriteLine("1. RSA");
            Console.WriteLine("2. AES");
            Console.Write("> ");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    // TODO
                    break;
                case "2":
                    System.Security.Cryptography.Aes aesAlg = System.Security.Cryptography.Aes.Create();
                    aesAlg.GenerateKey();
                    byte[] key = aesAlg.Key;
                    File.WriteAllBytes("./AES.key", key);
                    break;
            }
        }

        private void SetKeyConsole()
        {
            string input;
            Console.WriteLine("Path to key of AES? (leave blank for \"./Aes.key\")");
            Console.Write("> ");
            input = Console.ReadLine();
            if (input == "")
                input = "./Aes.key";
            _protocolOptions.AesKey = File.ReadAllBytes(input);
            _protocolOptions.EnableAes = true;
        }
    }
}
