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
      makeFunc.Add((ushort)PacketID.S_BroadcastEnterGame, MakePacket<S_BroadcastEnterGame>);
        handler.Add((ushort)PacketID.S_BroadcastEnterGame, PacketHandler.S_BroadcastEnterGameHandler);
      makeFunc.Add((ushort)PacketID.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
        handler.Add((ushort)PacketID.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);
      makeFunc.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
        handler.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);
      makeFunc.Add((ushort)PacketID.S_BroadcastMove, MakePacket<S_BroadcastMove>);
        handler.Add((ushort)PacketID.S_BroadcastMove, PacketHandler.S_BroadcastMoveHandler);

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
