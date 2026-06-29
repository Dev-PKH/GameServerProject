using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new(() => null);
        public static int ChuckSize { get; set; } = 4096 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new(ChuckSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new(ChuckSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close (int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] buffer;
        int userSize = 0; // 사용된 데이터 크기

        public int FreeSize => buffer.Length - userSize;

        public SendBuffer(int chunkSize)
        {
            buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(buffer, userSize, reserveSize); 
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new(buffer, this.userSize, userSize);
            this.userSize += userSize;
            return segment;
        }
    }
}
