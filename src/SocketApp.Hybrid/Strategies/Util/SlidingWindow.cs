using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketApp.Util
{
    // Selective Repeat ARQ
    public class SlidingWindow
    {
        private List<bool> _bitmap = new List<bool>();  // 1: unused/valid; 0: used/invalid;
        private int _windowSize;
        private int _startSeq;

        public SlidingWindow(int startSeq, int windowSize)
        {
            _windowSize = windowSize;
            _startSeq = startSeq;
            if (windowSize <= 0)
                throw new Exception("window size must bigger than 0");
            if (windowSize > int.MaxValue)
                throw new Exception("SR-ARQ does not allow a window size better than half of sequence size");
            InitBitmap(_bitmap, startSeq, windowSize);
        }

        public bool IsValid(int seq)
        {
            if (seq - _startSeq >= _windowSize || seq < _startSeq)
            {
                return false;
            }
            return _bitmap[seq - _startSeq];
        }

        public void Update(int seq)
        {
            if (!IsValid(seq))
                return;
            _bitmap[seq - _startSeq] = false;

            // renew window location
            MoveWindowLocation();
        }

        private static void InitBitmap(List<bool> bitmap, int start, int length)
        {
            int i = start;
            for (i = start; i < start + length; ++i)
            {
                bitmap.Add(true);
            }
        }

        private void MoveWindowLocation()
        {
            while (_bitmap.First() == false)
            {
                _bitmap.RemoveAt(0);
                _bitmap.Add(true);
                ++_startSeq;
            }
        }
    }
}
