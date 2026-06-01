using System;

namespace ServerCore
{
    class SpinLock
    {
        volatile int locked = 0;

        public void Acquire()
        {
            while (true)
            {
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
        static int num = 0; // 
        static object lObj = new object(); // lock Object
        static SpinLock spinLock = new SpinLock();

        // Spin Lock Thread
        static void SThread1()
        {
            for (int i = 0; i < 100000; i++)
            {
                spinLock.Acquire();
                num++;
                spinLock.Release();
            }
        }

        static void SThread2()
        {
            for (int i = 0; i < 100000; i++)
            {
                spinLock.Acquire();
                num--;
                spinLock.Release();
            }
        }

        // Interlocked.Increment, Decrement Thread
        static void IThread1()
        {
            for (int i = 0; i < 100000; i++)
            {
                Interlocked.Increment(ref num);
            }
        }

        static void IThread2()
        {
            for (int i = 0; i < 100000; i++)
            {
                Interlocked.Decrement(ref num);
            }
        }

        // Locked Thread

        static void LThread1()
        {
            for (int i = 0; i < 100000; i++)
            {
                lock (lObj)
                {
                    num++;
                }
            }
        }

        static void LThread2()
        {
            for (int i = 0; i < 100000; i++)
            {
                lock (lObj)
                {
                    num--;
                }
            }
        }

        static void Main(string[] args)
        {
            long now = DateTime.Now.Ticks;
            Task st1 = new Task(SThread1);
            Task st2 = new Task(SThread2);

            st1.Start();
            st2.Start();

            Task.WaitAll(st1, st2);
            long cur = DateTime.Now.Ticks;
            Console.WriteLine($"SpinLock Tick: {cur - now}");

            num = 0; // 초기화 방어 코드

            now = DateTime.Now.Ticks;
            Task it1 = new Task(IThread1);
            Task it2 = new Task(IThread2);

            it1.Start();
            it2.Start();

            Task.WaitAll(it1, it2);
            cur = DateTime.Now.Ticks;
            Console.WriteLine($"Interlocked(In/Decrement) Tick: {cur - now}");

            num = 0; // 초기화 방어 코드

            now = DateTime.Now.Ticks;
            Task lt1 = new Task(LThread1);
            Task lt2 = new Task(LThread2);

            lt1.Start();
            lt2.Start();

            Task.WaitAll(lt1, lt2);
            cur = DateTime.Now.Ticks;
            Console.WriteLine($"Lock Tick: {cur - now}");
        }
    }
}
