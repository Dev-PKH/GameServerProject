using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class ReceiveBuffer
    {
        ArraySegment<byte> buffer;

        int readCursor; // 읽기 커서 위치(실제 데이터를 처리한 위치)
        int writeCursor; // 쓰기 커서 위치(실제 데이터가 저장된만큼의 길이)

        public ReceiveBuffer(int bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize => writeCursor - readCursor; // 현재 남은 데이터 사이즈 (받은 데이터에서 - 처리된 데이터)
        public int FreeSize => buffer.Count - writeCursor; // 데이터를 더 받을 수 있는 사이즈 (버퍼 남은 공간)

        // 유효 범위까지의 데이터 반환 (아직 처리가 안된 데이터를 반환)
        public ArraySegment<byte> ReadSegment => new(buffer.Array, buffer.Offset + readCursor, DataSize);

        // 데이터를 더 받을 수 있는 공간을 반환 
        public ArraySegment<byte> WriteSegment => new(buffer.Array, buffer.Offset + writeCursor, FreeSize);

        // 커서 위치 초기화
        public void Clear()
        {
            int dataSize = DataSize;

            // 처리할 데이터가 없는 경우
            if( dataSize == 0 )
            {
                readCursor = writeCursor = 0;
            }
            else
            {
                // 현재 처리되지 않은 데이터를 맨 앞으로 땡김
                // (SourceArray, copyStartIndex, DestinationArray, coppiedStartIndex, Length)
                // [][][r][][][w][][] -> [r][][][w][][][][]
                Array.Copy(buffer.Array, buffer.Offset + readCursor, buffer.Array, buffer.Offset, dataSize);
                readCursor = 0;
                writeCursor = dataSize;
            }
        }

        public bool OnRead(int index)
        {
            // 현재 남은 데이터보다 더 많이 읽으려는 경우
            if (index > DataSize)
                return false;

            readCursor += index;
            return true;
        }

        public bool OnWrite(int index)
        {
            // 현재 수용 가능한 사이즈보다 더 많은 데이터를 쓰려는 경우
            if (index > FreeSize)
                return false;

            writeCursor += index;
            return true;
        }
    }
}
