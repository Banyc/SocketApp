using System.IO;
using System;
using SocketApp.Protocol;

namespace SocketApp
{
    // passively respond to socket events
    public class Responser
    {
        SockController _sockController;
        SockMgr _sockMgr;

        public Responser(SockController sockController, ProtocolStack protocolStack, SockMgr sockMgr)
        {
            _sockController = sockController;
            if (sockMgr.GetSockBase().Role == SocketRole.Client)  // listener does not need to send or receive
                LinkProtocolStackEvents(protocolStack);
            _sockMgr = sockMgr;
        }

        public void LinkProtocolStackEvents(ProtocolStack protocolStack)
        {
            UnlinkProtocolStack(protocolStack);
            protocolStack.NextHighLayerEvent += OnNextHighLayerEvent;
            protocolStack.NextLowLayerEvent += OnNextLowLayerEvent;
        }

        public void UnlinkProtocolStack(ProtocolStack protocolStack)
        {
            if (protocolStack == null)
                return;
            protocolStack.NextHighLayerEvent -= OnNextHighLayerEvent;
            protocolStack.NextLowLayerEvent -= OnNextLowLayerEvent;
        }

        // respond to event at the bottom of the protocol stack
        private void OnNextLowLayerEvent(Protocol.DataContent dataContent)
        {
            _sockMgr.GetSockBase().StartSend((byte[])dataContent.Data, dataContent.ExternalCallback, dataContent.ExternalCallbackState);
        }
        // respond to event at the top of the protocol stack
        private void OnNextHighLayerEvent(Protocol.DataContent dataContent)
        {
            switch (dataContent.Type)
            {
                case Protocol.DataProtocolType.Text:
                    // print:
                    // [Message] remote -> local | time
                    // data
                    // [MessageEnd]
                    Console.WriteLine();
                    Console.WriteLine(string.Format("[Message] {0} -> {1} | {2}",
                        _sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString(),
                        _sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                        DateTime.Now.ToString()));
                    Console.WriteLine((string)dataContent.Data);
                    Console.WriteLine(string.Format("[MessageEnd]"));
                    Console.Write("> ");
                    break;
                case Protocol.DataProtocolType.SmallFile:
                    string dirPath = "./recvFiles";
                    Console.WriteLine(string.Format("[File] {0} -> {1} | {2}",
                        _sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString(),
                        _sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                        DateTime.Now.ToString()));
                    Protocol.SmallFileDataObject dataObject = (Protocol.SmallFileDataObject)dataContent.Data;
                    Console.WriteLine($"Saving File \"{dataObject.Filename}\" to \"{dirPath}\" ...");
                    Directory.CreateDirectory(dirPath);
                    string filepath = Util.SaveFile.WriteFile(Path.Combine(dirPath, dataObject.Filename), dataObject.BinData);
                    Console.WriteLine($"File \"{filepath}\" saved");
                    Console.WriteLine(string.Format("[FileEnd]"));
                    Console.Write("> ");
                    break;
                case Protocol.DataProtocolType.Management:
                    TransportState state = dataContent.TransportState;
                    if (state.PendingLength == 0)
                    {
                        // this branch should not be reached
                        Console.WriteLine("[Transport] Done");
                        Console.Write("> ");
                    }
                    else
                    {
                        // don't print if less than 10 KB
                        if (state.PendingLength < 1024 * 10)
                            break;
                        double remainingSec;
                        remainingSec = (state.PendingLength - state.ReceivedLength) / 1024 / state.Speed;
                        Console.WriteLine(string.Format("[Transport] {0} -> {1} | {2}",
                            _sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString(),
                            _sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                            DateTime.Now.ToString()));
                        Console.WriteLine($"[Transport] Speed {state.Speed.ToString("0.0")} KB/s | Pending {Util.FormatConverter.ByteSizeToHumanReadable(state.PendingLength)} | Received {Util.FormatConverter.ByteSizeToHumanReadable(state.ReceivedLength)} | ETA {Util.FormatConverter.SecondToHumanReadable(remainingSec)}");
                        Console.Write("> ");
                    }
                    break;
            }
            _sockMgr.RaiseSockMgrProtocolTopEvent(dataContent);
        }

        // received new message
        public void OnSockMgrReceive(Object sender, SockMgrReceiveEventArgs e)
        {
            if (e.Handler != _sockMgr)
                throw new Exception("this Responser is not serving the right SockMgr");

            DataContent dataContent = new DataContent();
            dataContent.Data = e.Buffer;
            dataContent.SockMgr = _sockMgr;
            dataContent.SockController = _sockController;

            e.Handler.GetProtocolStack().FromLowLayerToHere(dataContent);
        }

        // the connection might be a failed one
        public void OnSockMgrConnect(Object sender, SockMgrConnectEventArgs e)
        {
            if (!e.Handler.GetSockBase().IsConnected)  // connection failed
            {
                Console.WriteLine(string.Format("[Connect] Failed | {0} times left | {1}", e.State.timesToTry, e.State.errorType.ToString()));
                Console.Write("> ");
                return;
            }
            _sockController.AddSockMgr(e.Handler, SocketRole.Client);
            // print: [Connect] local -> remote
            Console.WriteLine(string.Format("[Connect] {0} -> {1}",
                e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
            // send connection info to peer
            e.Handler.SendText(string.Format("{0} -> {1}",
                e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
        }

        public void OnSockMgrAccept(Object sender, SockMgrAcceptEventArgs e)
        {
            _sockController.AddSockMgr(e.Handler, SocketRole.Client);
            // print: [Accept] local -> remote
            Console.WriteLine(string.Format("[Accept] {0} -> {1}",
                e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
            // send connection info to peer
            e.Handler.SendText(string.Format("{0} -> {1}",
                e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
        }

        public void OnSockMgrShutdownBegin(Object sender, SockMgrShutdownBeginEventArgs e)
        {
            _sockController.RemoveSockMgr(e.Handler);
            // TODO: Dispose protocol stack
            try
            {
                // print: [Shutdown] local -> remote
                if (e.Handler.GetSockBase().Role == SocketRole.Client)
                    Console.WriteLine(string.Format("[Shutdown] {0} -> {1}",
                        e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                        e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString()));
                if (e.Handler.GetSockBase().Role == SocketRole.Listener)
                    Console.WriteLine(string.Format("[Shutdown] {0} <- *",
                        e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString()));
                Console.Write("> ");
            }
            catch (ObjectDisposedException) { }
        }
    }
}
