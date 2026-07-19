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
            //Console.WriteLine($"Flush {pendingList.Count}");
            pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            pendingList.Add(segment);  
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가
            sessions.Add(session);
            session.Room = this;

            // 신규 유저에게 현재 접속중인 유저 목록 전송
            S_PlayerList players = new();
            foreach (ClientSession cs in sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (session == cs),
                    PlayerId = cs.SessionId,
                    posX = cs.PosX,
                    posY = cs.PosY,
                    posZ = cs.PosZ,
                });
            }
            session.Send(players.Write());

            // 접속중인 유저에게 신규 유저 진입을 알림
            S_BroadcastEnterGame enter = new();
            enter.PlayerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());

        }

        public void Leave(ClientSession session)
        {
            // 플레이어 이탈
            sessions.Remove(session);

            // 접속중인 유저에게 이를 전달
            S_BroadcastLeaveGame leave = new();
            leave.playerId= session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 변경
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 전달
            S_BroadcastMove move = new();
            move.PlayerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}
