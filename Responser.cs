using System.Text;
using System;

namespace SocketApp
{
    public class Responser
    {
        public void OnSocketReceive(SockMgr source, SocketReceiveEventArgs e)
        {
            byte[] data;

            data = e.BufferMgr.GetAdequateBytes();
            while (data.Length > 0)
            {
                Console.WriteLine(Encoding.UTF8.GetString(data));

                data = e.BufferMgr.GetAdequateBytes();
            }
        }
    }
}
