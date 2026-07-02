using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Packet()
    {
        public ushort size;
        public ushort packetId;
    }


    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Conneted: {endPoint}");

            Packet packet = new() { size = 4, packetId = 7 };

            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] buff1 = BitConverter.GetBytes(packet.size);
                byte[] buff2 = BitConverter.GetBytes(packet.packetId);
                Array.Copy(buff1, 0, openSegment.Array, openSegment.Offset, buff1.Length);
                Array.Copy(buff2, 0, openSegment.Array, openSegment.Offset + buff1.Length, buff2.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 해제: {endPoint}");
        }

        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string receiveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count); // 버퍼 크기, 시작 위치, 바이트 개수
            Console.WriteLine($"[서버가 받을 정보] {receiveData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"사용된 바이트 수: {numOfBytes}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new();
            connector.Connect(endPoint, () => new GameSession());

            while(true)
            {
                try
                {
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}
