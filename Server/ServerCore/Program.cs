namespace ServerCore
{
    class SpinLock
    {
        volatile int locked = 0;

        public void Acquire()
        {
            while (true)
            {
                // Interlocked.Exchange(ref int, value): int의 현재 값을 반환하며, int값을 value로 바꿈
                // ex) locked 0일때, int original = Interlocked.Exchange(ref locked, 1) 실행 결과
                // original: 0(Exchange이전 locked값), locekd = 1(Exchange Value(1)값을 적용)
                int original = Interlocked.Exchange(ref locked, 1);
                if (original == 0) break;
            }
        }

        public void Release()
        {
            locked = 0;
        }
    }

    class Program
    {
        static int num = 0;
        static SpinLock spinLock = new SpinLock();

        static void Thread1()
        {
            for(int i=0; i<100000; i++)
            {
                spinLock.Acquire();
                num++;
                spinLock.Release();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 100000; i++)
            {
                spinLock.Acquire();
                num--;
                spinLock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(num);
        }
    }
}
