using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS로 ip 추출 ex) www.google.com -> 142.251.152.119
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("=======Listening=======");
            
            while(true)
            {
                
            }
        }
    }
}
