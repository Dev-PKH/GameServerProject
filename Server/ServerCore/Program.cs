using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료: {endPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Parking Server~!");
            Send(sendBuff);

            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"연결 해제: {endPoint}");
        }

        public override void OnRecive(ArraySegment<byte> buffer)
        {
            string reciveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count); // 버퍼 크기, 시작 위치, 바이트 개수
            Console.WriteLine($"[Client Receive Data] {reciveData}");
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
            
            while(true)
            {
                
            }
        }
    }
}
