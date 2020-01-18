using System.Text;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    // passively respond to socket events
    public class Responser
    {
        SockList _sockList;

        Func<byte[], object> _DeserializeMethod;
        
        public Responser(SockList sockList, Func<byte[], object> DeserializeMethod)
        {
            _sockList = sockList;
            _DeserializeMethod = DeserializeMethod;
        }

        public void SetDeserializeMethod(Func<byte[], object> DeserializeMethod)
        {
            _DeserializeMethod = DeserializeMethod;
        }

        public void OnSockMgrReceive(Object sender, SockMgrReceiveEventArgs e)
        {
            byte[] data;

            data = e.BufferMgr.GetAdequateBytes();
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
                Console.WriteLine((string)_DeserializeMethod(data));
                Console.WriteLine(string.Format("[MessageEnd]"));

                data = e.BufferMgr.GetAdequateBytes();
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
            e.Handler.Send(string.Format("{0} -> {1}",
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
            e.Handler.Send(string.Format("{0} -> {1}",
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
