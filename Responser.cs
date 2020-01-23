using System.Net.Mime;
using System.Text;
using System;
using System.Collections.Generic;
using SocketApp.Protocol;

namespace SocketApp
{
    // passively respond to socket events
    public class Responser
    {
        SockList _sockList;

        Protocol.ProtocolList _protocolList;
        SockMgr _sockMgr;

        public Responser(SockList sockList, Protocol.ProtocolList protocolList, SockMgr sockMgr)
        {
            _sockList = sockList;
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

        private void OnNextLowLayerEvent(Protocol.DataContent dataContent)
        {
            _sockMgr.GetSockBase().Send((byte[])dataContent.Data);
        }
        private void OnNextHighLayerEvent(Protocol.DataContent dataContent)
        {
            switch (dataContent.Type)
            {
                case Protocol.DataProtocolType.Text:
                    Console.WriteLine((string)dataContent.Data);
                    Console.WriteLine(string.Format("[MessageEnd]"));
                    break;
            }
        }

        public void OnSockMgrReceive(Object sender, SockMgrReceiveEventArgs e)
        {
            byte[] data;

            data = e.BufferMgr.GetAdequateBytes();
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
                    e.Handler.GetSockBase().GetSocket().RemoteEndPoint.ToString(),
                    e.Handler.GetSockBase().GetSocket().LocalEndPoint.ToString(),
                    DateTime.Now.ToString()));
                dataContent.Type = DataProtocolType.Text;
                _protocolList.Text.FromLowLayerToHere(dataContent);

                data = e.BufferMgr.GetAdequateBytes();
                dataContent = new DataContent();
                dataContent.Data = data;
            }
            Console.Write("> ");
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
            _sockList.Clients.Add(e.Handler);
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
            _sockList.Clients.Add(e.Handler);
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
            if (e.Handler.GetSockBase().Role == SocketRole.Listener)
                _sockList.Listeners.Remove(e.Handler);
            else
                _sockList.Clients.Remove(e.Handler);
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
