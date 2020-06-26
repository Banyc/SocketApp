using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

namespace SocketApp.Simple.Util
{
    public class BufferMgr
    {
        private Mutex _mutex = new Mutex();

        List<byte> _buffer = new List<byte>();

        public BufferMgr() { }

        public void AddBytes(Byte[] bs, int length)
        {
            _mutex.WaitOne();

            _buffer.AddRange(bs.Take(length));

            _mutex.ReleaseMutex();
        }

        public Byte[] GetAdequateBytes()
        {
            List<byte> data = new List<byte>();
            _mutex.WaitOne();

            if (_buffer.Count >= 4)
            {
                int length = BitConverter.ToInt32(_buffer.ToArray(), 0);
                if (length + 4 <= _buffer.Count)
                {
                    data.AddRange(_buffer.Skip(4).Take(length));
                    _buffer.RemoveRange(0, length + 4);
                }
            }

            _mutex.ReleaseMutex();
            return data.ToArray();
        }

        public int GetPendingLength()
        {
            if (_buffer.Count < 4)
                return 0;
            int length = BitConverter.ToInt32(_buffer.ToArray(), 0);
            return length;
        }

        public int GetReceivedLength()
        {
            return _buffer.Count;
        }
    }
}
