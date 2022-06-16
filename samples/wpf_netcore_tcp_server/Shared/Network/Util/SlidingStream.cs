// Reference from:
// https://stackoverflow.com/questions/8221136/fifo-queue-buffer-specialising-in-byte-streams
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SettingNetwork.Util
{
    public class SlidingStream : Stream
    {
        #region Other stream member implementations
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Length => _pendingSegments.Count;

        public override long Position 
        {
            get => 0;
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
            _pendingSegments.Clear();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion Other stream member implementations

        public SlidingStream()
        {
            ReadTimeout = -1;
        }

        private readonly object _writeSyncRoot = new object();
        private readonly object _readSyncRoot = new object();
        private readonly LinkedList<ArraySegment<byte>> _pendingSegments = new LinkedList<ArraySegment<byte>>();
        private readonly ManualResetEventSlim _dataAvailableResetEvent = new ManualResetEventSlim();


        public override int ReadTimeout { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_readSyncRoot)
            {
                int currentCount = 0;

                while (currentCount < count)
                {
                    ArraySegment<byte> segment = _pendingSegments.First.Value;
                    _pendingSegments.RemoveFirst();

                    int index = segment.Offset;
                    for (; index < segment.Count; index++)
                    {
                        if (currentCount == count)
                        {
                            break;
                        }
                        buffer[currentCount] = segment.Array[index];
                        currentCount++;
                    }

                    if (currentCount == count)
                    {
                        if (index < segment.Offset + segment.Count)
                        {
                            int remainCount = segment.Count - index;
                            byte[] copy = new byte[remainCount];
                            Array.Copy(segment.Array, index, copy, 0, remainCount);
                            _pendingSegments.AddFirst(new ArraySegment<byte>(copy, 0, remainCount));
                        }

                        return currentCount;
                    }

                    if (_pendingSegments.Count == 0)
                    {
                        lock (_writeSyncRoot)
                        {
                            byte[] copy = new byte[count];
                            Array.Copy(buffer, 0, copy, 0, currentCount);
                            _pendingSegments.AddFirst(new ArraySegment<byte>(copy));
                        }
                        return 0;
                    }
                }

                return currentCount;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_writeSyncRoot)
            {
                byte[] copy = new byte[count];
                Array.Copy(buffer, offset, copy, 0, count);

                _pendingSegments.AddLast(new ArraySegment<byte>(copy));
            }
        }

    }
}
