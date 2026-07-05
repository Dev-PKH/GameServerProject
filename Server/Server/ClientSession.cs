using ServerCore;
using System.Net;

namespace Server
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
                new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), packetId);
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
                        PlayerInfoRequest request = new PlayerInfoRequest();
                        request.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {request.playerId}");
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
