using System;

namespace ServerCore
{
    class Program
    {
        static volatile int count = 0;
        static RWLock rwLock = new();

        static void Main(string[] args)
        {
            Task t1 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    rwLock.WriteLock();
                    count++;
                    rwLock.WriteUnlock();
                }
            });

            Task t2 = new Task(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    rwLock.WriteLock();
                    count--;
                    rwLock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(count);
        }
    }
}
