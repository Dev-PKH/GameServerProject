using System;

namespace ServerCore
{
    class Program
    {
        static ThreadLocal<string> name = new(
            () => { return $"Name: {Thread.CurrentThread.ManagedThreadId}"; });
        // tls에서 새롭게 할당받을 때 마다, 현재 스레드의 id를 반환

        static void GetName()
        {
            bool repeat = name.IsValueCreated; // 현재 Thread가 별도의 공간을 마련했는지
            if (repeat)
                Console.WriteLine(name.Value + "(repeat)"); // 해당 변수를 사용
            else
                Console.WriteLine(name.Value); // 자신만의 공간을 만들고 사용
        }


        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(GetName, GetName, GetName, GetName, GetName); // task 5개 실행과 동일

            name.Dispose();
        }
    }
}
