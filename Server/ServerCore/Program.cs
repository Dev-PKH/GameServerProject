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
                Session session = new();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Parking Server~!");
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();
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
