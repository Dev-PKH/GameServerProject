using ServerCore;
using System.Net;
using System.Text;

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
			foreach(Attribute attribute in attributes)
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
		foreach(Skill skill in skills)
		    success &= skill.Write(span, ref count);
        success &= BitConverter.TryWriteBytes(span, count);
        if (!success)
            return null;
        return SendBufferHelper.Close(count);
    }
}
class Test
{
    public int testInt;

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> span = new(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        testInt = BitConverter.ToInt32(span.Slice(count, span.Length - count));
		count += sizeof(int);
    }
    
    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> span = new(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), testInt);
		count += sizeof(int);
        success &= BitConverter.TryWriteBytes(span, count);
        if (!success)
            return null;
        return SendBufferHelper.Close(count);
    }
}
