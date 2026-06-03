using System;

namespace ServerCore
{
    class Lock
    {
        AutoResetEvent available = new(true); // 매개변수는 첫 접근 허용 여부

        public void Acquire()
        {
            available.WaitOne(); // 입장 시도 AutoResetEvent.Reset 포함
            // AutoResetEvent.Reset() // 접근 차단 (flag = false)
        }

        public void Release()
        {
            available.Set(); // 접근 허용 (flag = true)
        }
    }

    class Program
    {
        static int num = 0;
        static Lock spinLock = new Lock();

        // Spin Lock Thread
        static void Thread1()
        {
            for (int i = 0; i < 100000; i++)
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
            Task st1 = new Task(Thread1);
            Task st2 = new Task(Thread2);

            st1.Start();
            st2.Start();

            Task.WaitAll(st1, st2);

            Console.WriteLine(num);
        }
    }
}
