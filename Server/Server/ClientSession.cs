using ServerCore;
using System.Net;

namespace Server
{
    class Packet()
    {
        public ushort size; // 패킷 전체 길이 (2byte -> 64KB까지 저장 가능)
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

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료: {endPoint}");

            /*Packet packet = new() { size = 100, packetId = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buff1 = BitConverter.GetBytes(packet.size);
            byte[] buff2 = BitConverter.GetBytes(packet.packetId);
            Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
            Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset + buff1.Length, buff2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buff1.Length + buff2.Length);

            Send(sendBuff);*/
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch((PacketID)id)
            {
                case PacketID.PlayerInfoRequest:
                    {
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;
                        Console.WriteLine($"PlayerInfoReq: {playerId}");
                    }
                    break;
            }

            Console.WriteLine($"PacketId: {id} / PacketSize: {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 해제: {endPoint}");
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"사용된 바이트 수: {numOfBytes}");
        }
    }
}
