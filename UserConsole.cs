using System.Net;
using System.IO;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    // Actively raise some operations to socket
    public class UserConsole
    {
        SockController _sockController;
        Protocol.DefaultProtocolFactoryOptions _protocolOptions = new Protocol.DefaultProtocolFactoryOptions();

        public UserConsole(SockController sockController)
        {
            _sockController = sockController;
            _protocolOptions.SockController = sockController;
        }

        public void ConsoleEntry(string[] args)
        {
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
                Console.WriteLine("9. Crypto Console");
                Console.WriteLine("10. Manage protocols");
                if (!_protocolOptions.SecondLowAESProtocolState.Enabled)
                {
                    Console.WriteLine("[Warning] Second lowest AES is not enabled");
                }
                if (!_protocolOptions.FirstLowAESProtocolState.Enabled)
                {
                    Console.WriteLine("[Warning] First lowest AES is not enabled");
                }

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
                        InterfaceMenu(_sockController.GetSockList().Clients[index]);
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
                        ListenerMenu(_sockController.GetSockList().Listeners[listenerIndex]);
                        break;
                    case "8":
                        ShutdownAll();
                        isExit = true;
                        break;
                    case "9":
                        CryptoConsole();
                        break;
                    case "10":
                        ProtocolConsole();
                        break;
                    default:
                        break;
                }
            }
        }

        // shutdown all listeners and clients
        void ShutdownAll()
        {
            _sockController.ShutdownAll();
        }

        void ListAllClients()
        {
            int i = 0;
            foreach (var sockMgr in _sockController.GetSockList().Clients)
            {
                Console.Write(string.Format("{0}\t", i));
                if (sockMgr.GetSockBase().IsHost)
                {
                    Console.Write(string.Format("Host\t"));
                }
                else
                {
                    Console.Write(string.Format("    \t"));
                }
                Console.WriteLine(string.Format("{0}\t->\t{1}",
                    sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
                ++i;
            }
        }

        void ListAllListeners()
        {
            int i = 0;
            foreach (var sockMgr in _sockController.GetSockList().Listeners)
            {
                Console.Write(string.Format("{0}\t", i));
                Console.WriteLine(sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString());
                ++i;
            }
        }

        void BuildListenerConsole()
        {
            SockFactoryOptions options = new SockFactoryOptions();
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
            options.ListenerIpAddress = IPAddress.Parse(localIpAddr);
            options.ListenerPort = localPort;
            options.ProtocolFactory = new Protocol.DefaultProtocolFactory(_protocolOptions);
            // begin to build
            switch (sel)
            {
                case "1":
                    _sockController.BeginBuildTcp(options, SocketRole.Listener);
                    break;
                case "2":
                    // TODO
                    break;
                default:
                    break;
            }
        }

        private void CallbackTest(object sender, SockMgrConnectEventArgs e)
        {
            // Console.WriteLine("[Test] Callback called");
            // Console.WriteLine((int)e.ExternalCallbackState);
        }

        void BuildClientConsole()
        {
            SockFactoryOptions options = new SockFactoryOptions();
            // collecting config
            Console.WriteLine("Enter server IP address (leave blank for 127.0.0.1:11000)");
            Console.Write("> ");
            string ipAddr = Console.ReadLine();
            int timesToTry = -1;
            if (ipAddr == "")
            {
                options.ListenerIpAddress = IPAddress.Parse("127.0.0.1");
                options.ListenerPort = 11000;
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
                        options.ListenerIpAddress = IPAddress.Parse(ipAddr);
                        options.ListenerPort = remotePort;
                    }
                    else
                    {
                        options.ListenerIpAddress = IPAddress.Parse(ipAddr);
                        options.ListenerPort = remotePort;
                        options.ClientPort = int.Parse(localPortStr);
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
            options.TimesToTry = timesToTry;
            options.ProtocolFactory = new Protocol.DefaultProtocolFactory(_protocolOptions);

            // begin to build
            _sockController.BeginBuildTcp(options, SocketRole.Client, CallbackTest, null, 1);
        }

        static void InterfaceMenu(SockMgr sockMgr)
        {
            bool isExit = false;
            while (!isExit && !sockMgr.IsShutdown)
            {
                Console.WriteLine(string.Format("[Interface Menu] {0} -> {1}",
                    sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
                Console.WriteLine("1. Send Text");
                Console.WriteLine("2. Close");
                Console.WriteLine("3. Is Host?");
                Console.WriteLine("4. Exit");
                Console.WriteLine("5. Config AES");
                Console.WriteLine("6. Send Small File");
                foreach (var proto in sockMgr.GetProtocolStack().GetState().MiddleProtocols)
                {
                    if (proto.GetType() == typeof(Protocol.AESProtocol) && ((Protocol.AESProtocol)proto).GetState().Enabled == false)
                        Console.WriteLine("[Warning] There exists an AES layer that is not enabled");
                }

                Console.Write("> ");
                string sel = Console.ReadLine();
                if (sockMgr.IsShutdown)
                    break;
                try
                {
                    switch (sel)
                    {
                        case "1":
                            SendTextConsole(sockMgr);
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
                        case "5":
                            InterfaceAesConsole(sockMgr);
                            break;
                        case "6":
                            SendSmallFileConsole(sockMgr);
                            break;
                        default:
                            break;
                    }
                }
                catch (NullReferenceException) { }  // in case the remote has shutdown
            }
        }

        static void InterfaceAesConsole(SockMgr sockMgr)
        {
            byte[] key;
            string input;
            Protocol.AESProtocolState state;
            List<Protocol.AESProtocol> aesProtocols = new List<Protocol.AESProtocol>();
            int i = 0;
            foreach (var proto in sockMgr.GetProtocolStack().GetState().MiddleProtocols)
            {
                if (proto.GetType() == typeof(Protocol.AESProtocol))
                {
                    aesProtocols.Add((Protocol.AESProtocol)proto);
                    Console.WriteLine($"{i}\t{proto.ToString()}");
                    ++i;
                }
            }
            if (aesProtocols.Count == 0)
                return;

            Console.WriteLine("[Interface-AES] Select one by index");
            Console.Write("> ");
            input = Console.ReadLine();
            int index = int.Parse(input);

            state = aesProtocols[index].GetState();

            Console.WriteLine("[Interface-AES]");
            Console.WriteLine("1. Set Key");
            Console.WriteLine("2. Disable Key");
            Console.WriteLine("3. Exit");
            Console.Write("> ");
            input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    key = SetKeyConsole();
                    if (key != null)
                    {
                        state.Key = key;
                        state.Enabled = true;
                        aesProtocols[index].SetState(state);
                    }
                    break;
                case "2":
                    state.Enabled = false;
                    aesProtocols[index].SetState(state);
                    break;
                case "3":
                    return;
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

        static void SendTextConsole(SockMgr sockMgr)
        {
            Console.WriteLine("Enter message to send");
            Console.Write("> ");
            string msg = Console.ReadLine();
            sockMgr.SendText(msg);
        }

        static void SendSmallFileConsole(SockMgr sockMgr)
        {
            Console.WriteLine("Enter filepath to send");
            Console.Write("> ");
            string filepath = Console.ReadLine();
            Protocol.SmallFileDataObject dataObject = new Protocol.SmallFileDataObject();
            dataObject.Filename = Path.GetFileName(filepath);
            Console.WriteLine("Reading File");
            try
            {
                dataObject.BinData = File.ReadAllBytes(filepath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("[Error] File Not Found");
                return;
            }
            Console.WriteLine("Sending File");
            sockMgr.SendSmallFile(dataObject, SendSmallFileCallback, filepath);
        }

        private static void SendSmallFileCallback(object sender, SockMgrSendEventArgs e)
        {
            Console.WriteLine($"[File] File \"{(string)e.ExternalCallbackState}\" Sent");
            Console.Write("> ");
        }

        void CryptoConsole()
        {
            while (true)
            {
                byte[] key;
                Console.WriteLine("[Crypto Console]");
                Console.WriteLine("1. Create Keys");
                Console.WriteLine("2. Set First Lowest Key");
                Console.WriteLine("3. Clean Keys");
                Console.WriteLine("4. Exit");
                Console.WriteLine("5. Set Second Lowest Key");
                Console.Write("> ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        KeyGenConsole();
                        break;
                    case "2":
                        key = SetKeyConsole("./AES.-1.key");
                        if (key != null)
                        {
                            _protocolOptions.FirstLowAESProtocolState.Key = key;
                            _protocolOptions.FirstLowAESProtocolState.Enabled = true;
                        }
                        break;
                    case "3":
                        _protocolOptions.FirstLowAESProtocolState.Key = null;
                        _protocolOptions.FirstLowAESProtocolState.Enabled = false;
                        _protocolOptions.SecondLowAESProtocolState.Key = null;
                        _protocolOptions.SecondLowAESProtocolState.Enabled = false;
                        break;
                    case "4":
                        return;
                    // break;
                    case "5":
                        key = SetKeyConsole("./AES.-2.key");
                        if (key != null)
                        {
                            _protocolOptions.SecondLowAESProtocolState.Key = key;
                            _protocolOptions.SecondLowAESProtocolState.Enabled = true;
                        }
                        break;
                }
            }
        }

        static void KeyGenConsole()
        {
            Console.WriteLine("Which?");
            // Console.WriteLine("1. RSA");
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
                    File.WriteAllBytes("./AES.-2.key", key);
                    aesAlg.GenerateKey();
                    File.WriteAllBytes("./AES.-1.key", aesAlg.Key);
                    break;
            }
        }

        // read key from file
        private static byte[] SetKeyConsole(string defaultPath = "./AES.key")
        {
            byte[] key = null;
            string input;
            Console.WriteLine($"Path to key of AES? (leave blank for \"{defaultPath}\")");
            Console.Write("> ");
            input = Console.ReadLine();
            if (input == "")
                input = defaultPath;
            try
            {
                key = File.ReadAllBytes(input);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("[Error] File not found.");
            }
            return key;
        }

        private void ProtocolConsole()
        {
            Console.WriteLine("[Protocol Console]");
            Console.WriteLine("1. Select protocol stack");
            Console.Write("> ");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    SelectProtocolStackConsole();
                    break;
            }
        }

        private void SelectProtocolStackConsole()
        {
            Console.WriteLine("[Please select]");
            int index = 0;
            foreach (Protocol.ProtocolStackType type in (Protocol.ProtocolStackType[])Enum.GetValues(typeof(Protocol.ProtocolStackType)))
            {
                Console.WriteLine($"{index}. {type.ToString()}");
                ++index;
            }
            Console.Write("> ");
            string input = Console.ReadLine();
            _protocolOptions.StackTypeOfChoice = (Protocol.ProtocolStackType)int.Parse(input);
        }
    }
}
