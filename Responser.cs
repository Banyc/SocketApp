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
        Protocol.ProtocolStackList _ProtocolStackList;
        SockMgr _sockMgr;

        public Responser(SockController sockController, Protocol.ProtocolStackList ProtocolStackList, SockMgr sockMgr)
        {
            _sockController = sockController;
            SetProtocolStackList(ProtocolStackList);
            _sockMgr = sockMgr;
        }

        public void SetProtocolStackList(Protocol.ProtocolStackList ProtocolStackList)
        {
            RemoveProtocolStackList();
            _ProtocolStackList = ProtocolStackList;
            // TODO: finish DEMO for File protocol
            // _ProtocolStackList.File.NextHighLayerEvent += OnNextHighLayerEvent;
            // _ProtocolStackList.File.NextLowLayerEvent += OnNextLowLayerEvent;
            _ProtocolStackList.Text.NextHighLayerEvent += OnNextHighLayerEvent;
            _ProtocolStackList.Text.NextLowLayerEvent += OnNextLowLayerEvent;
        }

        public void RemoveProtocolStackList()
        {
            if (_ProtocolStackList == null)
                return;
            _ProtocolStackList.Text.NextHighLayerEvent -= OnNextHighLayerEvent;
            _ProtocolStackList.Text.NextLowLayerEvent -= OnNextLowLayerEvent;
            _ProtocolStackList.Text.RemoveEventChains();
            _ProtocolStackList.Text = null;
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
            _sockMgr.RaiseSockMgrProtocolTopEvent(dataContent);
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
                _ProtocolStackList.Text.FromLowLayerToHere(dataContent);

                data = bufferMgr.GetAdequateBytes();
                dataContent = new DataContent();
                dataContent.Data = data;
            }
        }
    }
}
