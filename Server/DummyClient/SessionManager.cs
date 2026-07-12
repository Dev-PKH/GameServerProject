using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager session = new();
        public static SessionManager Instance { get { return session;} }

        List<ServerSession> sessions = new();
        object lockObj = new();

        public void SendForEach()
        {
            lock(lockObj)
            {
                foreach(ServerSession session in sessions)
                {
                    C_Chat chatPacket = new();
                    chatPacket.chat = $"챗팅 패킷 설정!";
                    ArraySegment<byte> segment = chatPacket.Write();

                    session.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock(lockObj)
            {
                ServerSession session = new();
                sessions.Add(session);
                return session;
            }
        }
    }
}
