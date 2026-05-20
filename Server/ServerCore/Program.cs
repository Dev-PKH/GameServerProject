namespace ServerCore
{
    class Program
    {
        static void MainThread(object state)
        {
            for(int i=0; i<5; i++)
                Console.WriteLine("Thread Running!");
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1); // 최소 스레드 1
            ThreadPool.SetMaxThreads(5, 5); // 최대 스레드 5

            for (int i = 0; i < 5; i++)
                ThreadPool.QueueUserWorkItem((obj) => { Console.WriteLine("Queue Running"); });

            ThreadPool.QueueUserWorkItem(MainThread); // 완료된 스레드가 

            while(true) // QueueUserWorkItem은 기본적으로 Background(IsBackground = true)이므로, 종료 방지
            {

            }

            /*Thread t = new Thread(MainThread);
            t.Name = "Test Thread";
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Thread Start!");

            t.Join();
            Console.WriteLine("End Complete!");*/
        }
    }
}
