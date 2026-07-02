using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;

namespace Server
{
    class Packet()
    {
        public ushort size; // 패킷 전체 길이 (2byte -> 64KB까지 저장 가능)
        public ushort packetId;
    }


    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료: {endPoint}");

            /*Packet packet = new() { size = 100, packetId = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buff1 = BitConverter.GetBytes(packet.size);
            byte[] buff2 = BitConverter.GetBytes(packet.packetId);
            Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
            Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset + buff1.Length, buff2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buff1.Length + buff2.Length);

            Send(sendBuff);*/
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"PacketId: {id} / PacketSize: {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 해제: {endPoint}");
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"사용된 바이트 수: {numOfBytes}");
        }
    }

    class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            // DNS로 ip 추출 ex) www.google.com -> 142.251.152.119
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("=======Listening=======");

            while (true)
            {

            }
        }
    }
}
