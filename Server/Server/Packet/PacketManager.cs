using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
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
            onReceive.Add((ushort)PacketID.PlayerInfoRequest, MakePacket<PlayerInfoRequest>);
            handler.Add((ushort)PacketID.PlayerInfoRequest, PacketHandler.PlayerInfoReqeustHandler);
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
}
