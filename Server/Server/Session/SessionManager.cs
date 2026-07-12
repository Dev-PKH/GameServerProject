using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class SessionManager
    {
        static SessionManager session = new();
        public static SessionManager Instance { get { return session; } }

        int sessionId = 0;
        Dictionary<int, ClientSession> sessions = new();
        object lockObj = new();

        public ClientSession Generate()
        {
            lock(lockObj)
            {
                int sessionId = ++this.sessionId;

                ClientSession session = new();
                session.SessionId = sessionId;
                sessions.Add(sessionId, session);

                Console.WriteLine($"Generate Session: {sessionId}");
                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (lockObj)
            {
                ClientSession session = null;
                sessions.TryGetValue(sessionId, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (lockObj)
            {
                sessions.Remove(sessionId);
            }
        }
    }
}
