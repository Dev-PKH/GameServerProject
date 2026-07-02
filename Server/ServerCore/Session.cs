using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // [size(2)][packetId(2)][...]
        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while(true)
            {
                // 최소한 헤더 파싱 여부 확인(Size)
                if (buffer.Count < HeaderSize)
                    break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                
                // 버퍼가 전체 데이터 사이즈보다 작을 경우(데이터가 온전히 전달되지 않은 경우)
                if (buffer.Count < dataSize)
                    break;

                OnReceivePacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                // 수신한 패킷을 처리하여 재할당
                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return 0;
        }

        public abstract void OnReceivePacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket socket;
        int disconnected = 0;

        // Recive
        ReceiveBuffer receiveBuffer = new(1024);

        // Send 필드
        object lockObj = new(); // lock 오브젝트
        Queue<ArraySegment<byte>> sendQueue = new(); // send 버퍼
        List<ArraySegment<byte>> sendPendingList = new(); // ArraySegment는 구조체라서, 값이 복사되어 저장됨 

        SocketAsyncEventArgs sendArgs = new();
        SocketAsyncEventArgs receiveArgs = new();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnReceive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            this.socket = socket;

            // Receive Setting
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);

            // Send Setting
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive();
        }

        public void Send(ArraySegment<byte> sendBuff)
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
                ArraySegment<byte> buffer = sendQueue.Dequeue();
                sendPendingList.Add(buffer);
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

        void RegisterReceive()
        {
            receiveBuffer.Clear();
            ArraySegment<byte> segment = receiveBuffer.WriteSegment;
            receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = socket.ReceiveAsync(receiveArgs);

            if (pending == false)
                OnReceiveCompleted(null, receiveArgs);
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 통괴 조건(받은 바이트가 0이상이고, 소켓 에러가 없는 경우)
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // 데이터 쓰기 범위 에러 (Write 커서 이동 진행)
                    if (receiveBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 데이터 읽기 범위 에러
                    int processLength = OnReceive(receiveBuffer.ReadSegment);
                    if(processLength < 0 || receiveBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // 데이터 읽기 범위 에러 (Read 커서 이동 진행)
                    if(receiveBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }


                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Receive Faild {e}");
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
