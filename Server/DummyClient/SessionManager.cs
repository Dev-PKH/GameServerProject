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
        Random rand = new();

        public void SendForEach()
        {
            lock(lockObj)
            {
                foreach(ServerSession session in sessions)
                {
                    C_Move mvPkt = new();
                    mvPkt.posX = rand.Next(-50, 50);
                    mvPkt.posY = 0;
                    mvPkt.posZ = rand.Next(-50, 50);
                    session.Send(mvPkt.Write());
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
