using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient
{
    public enum PacketID
    {
        PlayerInfoRequest = 1,
        Test = 2,

    }


    class PlayerInfoRequest
    {
        public byte testByte;
        public long playerId;
        public string name;
        public class Skill
        {
            public int id;
            public short level;
            public float duration;
            public class Attribute
            {
                public int att;

                public void Read(ReadOnlySpan<byte> span, ref ushort count)
                {
                    att = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                    count += sizeof(int);
                }

                public bool Write(Span<byte> span, ref ushort count)
                {
                    bool success = true;
                    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), att);
                    count += sizeof(int);
                    return success;
                }
            }
            public List<Attribute> attributes = new();

            public void Read(ReadOnlySpan<byte> span, ref ushort count)
            {
                id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
                attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
                count += sizeof(ushort);
                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(span, ref count);
                    attributes.Add(attribute);
                }
            }

            public bool Write(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
                count += sizeof(float);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)attributes.Count);
                count += sizeof(ushort);
                foreach (Attribute attribute in attributes)
                    success &= attribute.Write(span, ref count);
                return success;
            }
        }
        public List<Skill> skills = new();

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> span = new(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            testByte = (byte)segment.Array[segment.Offset + count];
            count += sizeof(byte);
            playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
            count += nameLen;
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(span, ref count);
                skills.Add(skill);
            }
        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> span = new(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInfoRequest);
            count += sizeof(ushort);
            segment.Array[segment.Offset + count] = (byte)testByte;
            count += sizeof(byte);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(name, 0, name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in skills)
                success &= skill.Write(span, ref count);
            success &= BitConverter.TryWriteBytes(span, count);
            if (!success)
                return null;
            return SendBufferHelper.Close(count);
        }
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Conneted: {endPoint}");

            PlayerInfoRequest packet = new() { playerId = 1001, name = "PKH" };
            var skill = new PlayerInfoRequest.Skill() { id = 101, level = 1, duration = 3.0f };
            skill.attributes.Add(new() { att = 77 });
            packet.skills.Add(skill);

            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 301, level = 3, duration = 3.0f });
            packet.skills.Add(new PlayerInfoRequest.Skill() { id = 401, level = 4, duration = 2.0f });

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> segment = packet.Write();

                if (segment != null)
                    Send(segment);
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
