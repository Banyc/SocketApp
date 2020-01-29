using System;
using System.Collections.Generic;
using System.Text;
using SocketApp.Protocol;

namespace SocketApp
{
    // passively respond to socket events
    public class Responser
    {
        SockController _sockController;
        Protocol.ProtocolList _protocolList;
        SockMgr _sockMgr;

        public Responser(SockController sockController, Protocol.ProtocolList protocolList, SockMgr sockMgr)
        {
            _sockController = sockController;
            SetProtocolList(protocolList);
            _sockMgr = sockMgr;
        }

        public void SetProtocolList(Protocol.ProtocolList protocolList)
        {
            _protocolList = protocolList;
            // TODO: finish DEMO for File protocol
            // _protocolList.File.NextHighLayerEvent += OnNextHighLayerEvent;
            // _protocolList.File.NextLowLayerEvent += OnNextLowLayerEvent;
            _protocolList.Text.NextHighLayerEvent += OnNextHighLayerEvent;
            _protocolList.Text.NextLowLayerEvent += OnNextLowLayerEvent;
        }

        public void RemoveProtocolList()
        {
            _protocolList.Text.NextHighLayerEvent -= OnNextHighLayerEvent;
            _protocolList.Text.NextLowLayerEvent -= OnNextLowLayerEvent;
            _protocolList.Text.RemoveEventChains();
            _protocolList.Text = null;
        }

        // respond to event at the bottom of the protocol stack
        private void OnNextLowLayerEvent(Protocol.DataContent dataContent)
        {
            _sockMgr.GetSockBase().Send((byte[])dataContent.Data);
        }
        // respond to event at the top of the protocol stack
        private void OnNextHighLayerEvent(Protocol.DataContent dataContent)
        {
            switch (dataContent.Type)
            {
                case Protocol.DataProtocolType.Text:
                    Console.WriteLine((string)dataContent.Data);
                    Console.WriteLine(string.Format("[MessageEnd]"));
                    Console.Write("> ");
                    break;
            }
        }

        // received new message
        public void OnSockMgrReceive(Object sender, SockMgrReceiveEventArgs e)
        {
            ProcessDataFromBuffer(e.BufferMgr, e.Handler);
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
        
        private void ProcessDataFromBuffer(BufferMgr bufferMgr, SockMgr sockMgr)
        {
            byte[] data;

            data = bufferMgr.GetAdequateBytes();
            DataContent dataContent = new DataContent();
            dataContent.Data = data;
            while (data.Length > 0)
            {
                // print:
                // [Message] remote -> local | time
                // data
                // [MessageEnd]
                Console.WriteLine();
                Console.WriteLine(string.Format("[Message] {0} -> {1} | {2}",
                    sockMgr.GetSockBase().GetSocket().RemoteEndPoint.ToString(),
                    sockMgr.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    DateTime.Now.ToString()));
                dataContent.Type = DataProtocolType.Text;
                _protocolList.Text.FromLowLayerToHere(dataContent);

                data = bufferMgr.GetAdequateBytes();
                dataContent = new DataContent();
                dataContent.Data = data;
            }
        }
    }
}
