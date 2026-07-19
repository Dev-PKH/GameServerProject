using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }


        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료: {endPoint}");

            Program.Room.Push(() => Program.Room.Enter(this));
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnReceivePacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room  != null)
            {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }

            Console.WriteLine($"연결 해제: {endPoint}");
        }


        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"사용된 바이트 수: {numOfBytes}");
        }
    }
}
