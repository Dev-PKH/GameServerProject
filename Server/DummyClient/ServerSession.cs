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

        public PlayerInfoRequest()
        {
            packetId = (ushort)PacketID.PlayerInfoRequest;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            //success &= BitConverter.TryWriteBytes(
            //    new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(
                new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), (ushort)PacketID.PlayerInfoRequest);
            count += 2;
            success &= BitConverter.TryWriteBytes(
                new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), playerId);
            count += 8;

            success &= BitConverter.TryWriteBytes(
                new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), count);

            if (!success)
                return null;

            return SendBufferHelper.Close(count);
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
            count += 2;

            playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(segment.Array, segment.Offset + count, segment.Count - count));
            count += 8;
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

            PlayerInfoRequest packet = new() { playerId = 1001 };

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
