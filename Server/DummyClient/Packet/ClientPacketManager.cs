using ServerCore;

class PacketManager
{
    #region Singleton
    static PacketManager instance;
    public static PacketManager Instance
    {
        get
        {
            if(instance == null)
                instance = new PacketManager();
            return instance;
        }
    }
    #endregion

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> onReceive = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new();

    public void Register()
    {
        onReceive.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
        handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);

    }

    public void OnReceivePacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if(onReceive.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T: IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}
