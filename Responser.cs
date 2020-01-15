using System.Text;
using System;
using System.Collections.Generic;

namespace SocketApp
{
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

        public void OnSocketConnected(SockMgr source)  // provided by SockFactory
        {
            // print: [Connect] local -> remote
            Console.WriteLine(string.Format("[Connect] {0} -> {1}",
                source.GetSocket().LocalEndPoint.ToString(),
                source.GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
            // send connection info to peer
            source.Send(string.Format("{0} -> {1}",
                source.GetSocket().LocalEndPoint.ToString(),
                source.GetSocket().RemoteEndPoint.ToString()));
        }

        public void OnSocketShutdownBegin(SockMgr source, SocketShutdownBeginEventArgs e)
        {
            // print: [Shutdown] local -> remote
            Console.WriteLine(string.Format("[Shutdown] {0} -> {1}",
                source.GetSocket().LocalEndPoint.ToString(),
                source.GetSocket().RemoteEndPoint.ToString()));
            Console.Write("> ");
        }
    }
}
