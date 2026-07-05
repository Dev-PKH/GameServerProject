using ServerCore;
using System.Net;
using System.Text;

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
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
                count += sizeof(int);

                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
                count += sizeof(short);

                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> span, ref ushort count)
            {
                id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);

                level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
                count += sizeof(short);

                duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new();

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

            // Skill List
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (SkillInfo skill in skills)
                success &= skill.Write(span, ref count);

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

            // Skill List
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(span, ref count);
                skills.Add(skill);
            }
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
                        Console.WriteLine($"PlayerInfoReq: {request.playerId} / {request.name}");

                        foreach(PlayerInfoRequest.SkillInfo skill in request.skills)
                        {
                            Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
                        }
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
