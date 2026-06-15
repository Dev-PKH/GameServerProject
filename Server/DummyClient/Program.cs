using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while(true)
            {
                // 입장권 생성 (Client 전용 Socket)
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 차단봉(서버 listen Socket) 접근
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected TO {socket.RemoteEndPoint.ToString()}");

                    // 관리자(Server)로 데이터 송신
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Parking complete!~");
                    int sendBytes = socket.Send(sendBuff);

                    // 관리자의 데이터 수신
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    // 퇴장
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
