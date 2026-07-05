using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient
{
    class Packet()
    {
        public ushort size;
        public ushort packetId;
    }

    class PlayerInfoRequest : Packet
    {
        public long playerId;
    }

    class PlayerInfoInGame : Packet
    {
        public int hp;
        public int attack;
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

            PlayerInfoRequest packet = new() { packetId = (ushort)PacketID.PlayerInfoRequest, playerId = 1001 };

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

                ushort count = 0;
                bool success = true;

                //success &= BitConverter.TryWriteBytes(
                //    new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), packet.size);
                count += 2;
                success &= BitConverter.TryWriteBytes(
                    new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.packetId);
                count += 2;
                success &= BitConverter.TryWriteBytes(
                    new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packet.playerId);
                count += 8;

                success &= BitConverter.TryWriteBytes(
                    new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), count);

                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

                if (success)
                    Send(sendBuff);
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
