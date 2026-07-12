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
        List<ArraySegment<byte>> pendingList = new();

        public void Push(Action job)
        {
            jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession cs in sessions)
                cs.Send(pendingList);

            Console.WriteLine($"Flush {pendingList.Count}");
            pendingList.Clear();
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = $"[{packet.playerId}] : {chat}";
            ArraySegment<byte> segment = packet.Write();

            pendingList.Add(segment);  
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
