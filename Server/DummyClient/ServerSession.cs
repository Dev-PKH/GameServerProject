using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient
{
    public abstract class Packet()
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> segment);
    }

    class PlayerInfoRequest : Packet
    {
        public long playerId;
        public string name;

        public PlayerInfoRequest()
        {
            packetId = (ushort)PacketID.PlayerInfoRequest;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> span = new(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerId);
            count += sizeof(long);

            // string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(name, 0, name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(span, count);

            if (!success)
                return null;

            return SendBufferHelper.Close(count);
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> span = new(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);

            playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
            count += nameLen;
        }
    }

    public enum PacketID
    {
        None = 0,
        PlayerInfoRequest = 1,
        PlayerInfoInGame = 2,
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Conneted: {endPoint}");

            PlayerInfoRequest packet = new() { playerId = 1001, name = "PKH" };

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> sgement = packet.Write();

                if (sgement != null)
                    Send(sgement);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 해제: {endPoint}");
        }

        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string receiveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count); // 버퍼 크기, 시작 위치, 바이트 개수
            Console.WriteLine($"[서버가 받을 정보] {receiveData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"사용된 바이트 수: {numOfBytes}");
        }
    }
}
