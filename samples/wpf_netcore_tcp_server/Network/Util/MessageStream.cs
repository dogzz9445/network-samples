﻿// Reference from:
// https://stackoverflow.com/questions/8221136/fifo-queue-buffer-specialising-in-byte-streams
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network.Util
{
    public class MessageStream : Stream
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

        public MessageStream()
        {
            ReadTimeout = -1;
        }

        private readonly object _writeSyncRoot = new object();
        private readonly object _readSyncRoot = new object();
        private readonly LinkedList<ArraySegment<byte>> _pendingSegments = new LinkedList<ArraySegment<byte>>();
        private readonly ManualResetEventSlim _dataAvailableResetEvent = new ManualResetEventSlim();


        public override int ReadTimeout { get; set; }

        public bool CanReadMessage
        {
            get
            {
                lock (_readSyncRoot)
                {
                    if (_pendingSegments.Count < 4)
                    {
                        return false;
                    }
                    int length = BitConverter.ToInt32(_pendingSegments.First.Value);
                    if (_pendingSegments.Count < 4 + length)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //if (_dataAvailableResetEvent.Wait(ReadTimeout))
            //    throw new TimeoutException("No data available");

            lock (_readSyncRoot)
            {
                int currentCount = 0;
                int currentOffset = 0;

                while (currentCount != count)
                {
                    ArraySegment<byte> segment = _pendingSegments.First.Value;
                    _pendingSegments.RemoveFirst();

                    int index = segment.Offset;
                    for (; index < segment.Count && index < count; index++)
                    {
                        if (currentOffset < offset)
                        {
                            currentOffset++;
                        }
                        else
                        {
                            buffer[currentCount] = segment.Array[index];
                            currentCount++;
                        }
                    }

                    if (currentCount == count)
                    {
                        if (index < segment.Offset + segment.Count)
                        {
                            _pendingSegments.AddFirst(new ArraySegment<byte>(segment.Array, index, segment.Offset + segment.Count - index));
                        }
                    }

                    if (_pendingSegments.Count == 0)
                    {
                        _dataAvailableResetEvent.Reset();

                        return currentCount;
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

                _dataAvailableResetEvent.Set();
            }
        }

    }
}