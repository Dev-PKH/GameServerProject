using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket socket;
        int disconnected = 0;

        public void Start(Socket socket)
        {
            this.socket = socket;

            SocketAsyncEventArgs reciveArgs = new SocketAsyncEventArgs();
            reciveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReciveCompleted);

            reciveArgs.SetBuffer(new byte[1024], 0, 1024); // 바이트 크기, 시작 위치, 총 개수

            RegisterRecive(reciveArgs);
        }

        public void Send(byte[] sendBuff)
        {
            socket.Send(sendBuff);
        }

        public void Disconnect()
        {
            // 누군가 이미 disconnected를 진행한 경우 (중복 Disconnect 방지용)
            if (Interlocked.Exchange(ref disconnected, 1) == 1)
                return;

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        #region 네트워크 통신
        void RegisterRecive(SocketAsyncEventArgs args)
        {
            bool pending = socket.ReceiveAsync(args);

            if (pending == false)
                OnReciveCompleted(null, args);
        }

        void OnReciveCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 통괴 조건(받은 바이트가 0이상이고, 소켓 에러가 없는 경우)
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string reciveData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred); // 버퍼 크기, 시작 위치, 바이트 개수
                    Console.WriteLine($"[Client Receive Data] {reciveData}");
                    RegisterRecive(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Recive Faild {e}");
                }
            }
            else
            {

            }
        }
        #endregion
    }
}
