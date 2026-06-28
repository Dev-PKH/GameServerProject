using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class Session
    {
        Socket socket;
        int disconnected = 0;

        // Send 필드
        object lockObj = new(); // lock 오브젝트
        Queue<byte[]> sendQueue = new(); // send 버퍼
        List<ArraySegment<byte>> sendPendingList = new(); // ArraySegment는 구조체라서, 값이 복사되어 저장됨 

        SocketAsyncEventArgs sendArgs = new();
        SocketAsyncEventArgs reciveArgs = new();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            this.socket = socket;

            // Recive Setting
            reciveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReciveCompleted);
            reciveArgs.SetBuffer(new byte[1024], 0, 1024); // 바이트 크기, 시작 위치, 사용할 바이트 개수
                                                           // -> 1024크기의 byte배열에 0번째부터 1024개를 사용함. (시작이 1이면 1~ 1025이므로 에러)

            // Send Setting
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecive();
        }

        public void Send(byte[] sendBuff)
        {
            lock (lockObj) // 동시 접근 차단
            {
                sendQueue.Enqueue(sendBuff);
                if (sendPendingList.Count == 0) // 실행할 Send가 없어, 비동기 대기중일 때
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            // 이미 disconnected를 진행한 경우 (중복 Disconnect 방지용)
            if (Interlocked.Exchange(ref disconnected, 1) == 1)
                return;

            OnDisconnected(socket.RemoteEndPoint);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            while (sendQueue.Count > 0)
            {
                byte[] buffer = sendQueue.Dequeue();
                sendPendingList.Add(new ArraySegment<byte>(buffer, 0, buffer.Length));
            }

            sendArgs.BufferList = sendPendingList;

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
                        sendArgs.BufferList = null;
                        sendPendingList.Clear();

                        OnSend(sendArgs.BytesTransferred);

                        // 작업이 완료될 때 Queue에 다른 Send가 남아있는 경우 (내 작업중에 다른 클라이언트나 추가적인 Send 요청이 온 경우)
                        if (sendQueue.Count > 0) 
                        {
                            RegisterSend();
                        }
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

        void RegisterRecive()
        {
            bool pending = socket.ReceiveAsync(reciveArgs);

            if (pending == false)
                OnReciveCompleted(null, reciveArgs);
        }

        void OnReciveCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 통괴 조건(받은 바이트가 0이상이고, 소켓 에러가 없는 경우)
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecive(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

                    RegisterRecive();
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
