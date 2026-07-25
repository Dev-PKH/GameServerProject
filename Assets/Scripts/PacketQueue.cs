using System.Collections.Generic;
using UnityEngine;

// 수신한 패킷의 정보를 관리 (패킷을 수신하여, Background 스레드에서 처리하는 것이 아닌 유니티에서 실행하도록 설정)
// ex - NetworkManager에서 GameObject.Find가 서버 쓰레드 풀에서 동작하여, 유니티 코드가 실행이 안되는걸 해결
public class PacketQueue
{
    public static PacketQueue Instance { get; } = new();

    Queue<IPacket> packetQueue = new();
    object lockObj = new();

    public void Push(IPacket packet)
    {
        lock(lockObj) 
        {
            packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock (lockObj)
        {
            if (packetQueue.Count == 0)
                return null;

            return packetQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        List<IPacket> list = new();

        lock (lockObj)
        {
            while (packetQueue.Count > 0)
            {
                list.Add(packetQueue.Dequeue());
            }
        }

        return list;
    }
}
