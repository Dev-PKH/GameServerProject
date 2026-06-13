using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS로 ip 추출 ex) www.google.com -> 142.251.152.119
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 1. 차단봉(listen Socket) 생성
            // [공부] Socket에서 Stream 타입이 뭔가?, 다른 타입이 뭐가 있지
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 2. 차단봉 설치(Bind)
                listenSocket.Bind(endPoint);

                // 3. 차단봉 설정 완료, backlog: 최대 대기수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("=======Listening=======");

                    // 차 입장 (Client)
                    Socket clientSocket = listenSocket.Accept(); // <- 클라이언트가 입장 안하면 아래로 안내려감 (실제론 이렇게 구현하지 않음)

                    // 차의 데이터 수신
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[Client Receive Data] {recvData}");

                    // 차로 데이터 송신
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Parking Server~!");
                    clientSocket.Send(sendBuff);

                    // 차 퇴장(강제)
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
