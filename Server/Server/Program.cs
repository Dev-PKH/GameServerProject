using System.Net;
using ServerCore;

namespace Server
{
    

    class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            // DNS로 ip 추출 ex) www.google.com -> 142.251.152.119
            string host = Dns.GetHostName(); // 내 local 컴퓨터의 host 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("=======Listening=======");

            while (true)
            {

            }
        }
    }
}
