using System.Text;
using System;
using System.Collections.Generic;

namespace SocketApp
{
    // passively respond to socket events
    public class Responser
    {
        List<SockMgr> _clients;
        List<SockMgr> _listeners;
        
        public Responser(List<SockMgr> clients, List<SockMgr> listeners)
        {
            _clients = clients;
            _listeners = listeners;
        }

        public void OnSocketReceive(SockMgr source, SocketReceiveEventArgs e)
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
                    source.GetSocket().RemoteEndPoint.ToString(),
                    source.GetSocket().LocalEndPoint.ToString(),
                    DateTime.Now.ToString()));
                Console.WriteLine(Encoding.UTF8.GetString(data));
                Console.WriteLine(string.Format("[MessageEnd]"));

                data = e.BufferMgr.GetAdequateBytes();
            }
            Console.Write("> ");
        }

        // the connection might be a failed one
        public void OnSocketConnect(object sender, SocketConnectEventArgs e)
        {
            if (!e.Handler.IsConnected)  // connection failed
            {
                Console.WriteLine(string.Format("[Connect] Failed | {0} times left | {1}", e.State.timesToTry, e.State.errorType.ToString()));
                Console.Write("> ");
                return;
            }
            _clients.Add(e.Handler);
            // print: [Connect] local -> remote
            Console.WriteLine(string.Format("[Connect] {0} -> {1}",
                e.Handler.GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
            // send connection info to peer
            e.Handler.Send(string.Format("{0} -> {1}",
                e.Handler.GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSocket().RemoteEndPoint.ToString()));
        }

        public void OnSocketAccept(SockMgr sender, SocketAcceptEventArgs e)  // from SockFactory
        {
            _clients.Add(e.Handler);
            // print: [Accept] local -> remote
            Console.WriteLine(string.Format("[Accept] {0} -> {1}",
                e.Handler.GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
            // send connection info to peer
            e.Handler.Send(string.Format("{0} -> {1}",
                e.Handler.GetSocket().LocalEndPoint.ToString(),
                e.Handler.GetSocket().RemoteEndPoint.ToString()));
        }

        public void OnSocketShutdownBegin(SockMgr source, SocketShutdownBeginEventArgs e)
        {
            if (source.Role == SocketRole.Listener)
                _listeners.Remove(source);
            else
                _clients.Remove(source);

            // print: [Shutdown] local -> remote
            if (source.Role == SocketRole.Client)
                Console.WriteLine(string.Format("[Shutdown] {0} -> {1}",
                    source.GetSocket().LocalEndPoint.ToString(),
                    source.GetSocket().RemoteEndPoint.ToString()));
            if (source.Role == SocketRole.Listener)
                Console.WriteLine(string.Format("[Shutdown] {0} <- *",
                    source.GetSocket().LocalEndPoint.ToString()));
            Console.Write("> ");
        }
    }
}
