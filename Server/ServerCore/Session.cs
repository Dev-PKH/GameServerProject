using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket socket;
        int disconnected = 0;

        object lockObj = new(); // lock 오브젝트
        Queue<byte[]> sendQueue = new(); // send 버퍼
        bool sendPending; // 현재 send Pending 여부
        SocketAsyncEventArgs sendArgs = new();

        public void Start(Socket socket)
        {
            this.socket = socket;

            SocketAsyncEventArgs reciveArgs = new SocketAsyncEventArgs();
            reciveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReciveCompleted);
            reciveArgs.SetBuffer(new byte[1024], 0, 1024); // 바이트 크기, 시작 위치, 사용할 바이트 개수

            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecive(reciveArgs);
        }

        public void Send(byte[] sendBuff)
        {
            lock (lockObj) // 동시 접근 차단
            {
                sendQueue.Enqueue(sendBuff);
                if (sendPending == false) // 비동기 대기중일 때
                {
                    RegisterSend();
                }
            }
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
        void RegisterSend()
        {
            sendPending = true; // Pending 활성화

            byte[] buff = sendQueue.Dequeue();
            sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = socket.SendAsync(sendArgs);
            if(pending == false)
            {
                OnSendCompleted(null, sendArgs);
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // sendArgs.Completed가 실행될 때를 처리하기 위해 lock 설정
            // SendAsync로 대기하다가 OnSendCompleted가 호출될 때, 다른 스레드에서 Send하면,
            // sendPending이 false여서 sendArgs의 버퍼처리가 덧씌워질 수 있음
            lock (lockObj)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // 작업이 완료될 때 Queue에 다른 Send가 남아있는 경우 (내 작업중에 다른 클라이언트나 추가적인 Send 요청이 온 경우)
                        if (sendQueue.Count > 0) 
                        {
                            RegisterSend();
                        }
                        else
                            sendPending = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Send Faild {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

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
                Disconnect();
            }
        }
        #endregion
    }
}
