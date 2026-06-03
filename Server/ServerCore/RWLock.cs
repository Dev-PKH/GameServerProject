using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    // ReaderWirterLock 구현
    class RWLock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)] // 4바이트(32비트를 쪼개 씀)
        int flag = EMPTY_FLAG;
        int writeCount = 0;


        // 아무도 WriteLock or ReadLock을 획득하지 않는다면 소유권 획득
        public void WriteLock()
        {
            // (재귀) 동일 스레드가 WriteLock을 이미 획득했는지 확인
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                writeCount++;
                return;
            }

            // 16비트만큼 밀어버리고, 남은 15비트(맨앞 1비트 제외)를 WRITE_MASK와 비교
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while(true)
            {
                for(int i=0; i<MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        writeCount = 1;
                        return;
                    }
                }

                Thread.Yield(); // 제한 시도 횟수를 넘어가면 대기
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --writeCount;
            if (lockCount == 0)
            {
                Interlocked.Exchange(ref flag, EMPTY_FLAG);
            }
        }

        // 아무도 WriteLock을 획득중이 아니라면 ReadCount 1증가
        public void ReadLock()
        {
            // (재귀) 동일 스레드가 WriteLock을 이미 획득했는지 확인
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref flag);
                return;
            }

            while (true)
            {
                for(int i=0; i<MAX_SPIN_COUNT; i++)
                {
                    int expected = flag & READ_MASK;

                    // flag가 expected와 동일한지 체크 == WriteLock이 없음
                    // -> WriteLock이 있으면, READ_MASK가 절대로 같을 수 없음(WriteLock에 대한 비트가 전부 0이므로)
                    if (Interlocked.CompareExchange(ref flag, expected + 1, expected) == expected) return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref flag);
        }

    }
}
