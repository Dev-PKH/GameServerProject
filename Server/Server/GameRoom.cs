using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> sessions = new();
        JobQueue jobQueue = new();

        public void Push(Action job)
        {
            jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = $"[{packet.playerId}] : {chat}";
            ArraySegment<byte> segment = packet.Write();


            foreach (ClientSession cs in sessions)
                cs.Send(segment);
        }

        public void Enter(ClientSession session)
        {
            sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            sessions.Remove(session);
        }
    }
}
