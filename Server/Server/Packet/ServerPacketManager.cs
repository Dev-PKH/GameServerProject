using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
    #region Singleton
    static PacketManager instance = new PacketManager();
    public static PacketManager Instance { get { return instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> makeFunc = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new();

    public void Register()
    {
      makeFunc.Add((ushort)PacketID.C_LeaveGame, MakePacket<C_LeaveGame>);
        handler.Add((ushort)PacketID.C_LeaveGame, PacketHandler.C_LeaveGameHandler);
      makeFunc.Add((ushort)PacketID.C_Move, MakePacket<C_Move>);
        handler.Add((ushort)PacketID.C_Move, PacketHandler.C_MoveHandler);

    }

    public void OnReceivePacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onReceiveCallback = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (makeFunc.TryGetValue(id, out func))
        {
            IPacket packet =  func.Invoke(session, buffer);

            if (onReceiveCallback != null)
                onReceiveCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T: IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if(handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
