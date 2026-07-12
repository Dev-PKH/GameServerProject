using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> sessions = new();
        object lockObj = new();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = chat;
            ArraySegment<byte> segment = packet.Write();

            lock (lockObj)
            {
                foreach (ClientSession cs in sessions)
                    cs.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (lockObj)
            {
                sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session) 
        {
            lock (lockObj)
            {
                sessions.Remove(session);
            }
        }
    }
}
