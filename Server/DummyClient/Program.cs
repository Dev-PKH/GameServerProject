using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            for (int i = 0; i < 5; i++)
            {
                // 관리자(Server)로 데이터 송신
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Parking complete!~ {i}");
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
