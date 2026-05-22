namespace ServerCore
{
    class Program
    {
        volatile static bool stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("Thread Start!");

            while(!stop)
            {

            }

            Console.WriteLine("Thread End!");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);

            stop = true;

            Console.WriteLine("Stop Call");
            Console.WriteLine("Stop Waiting!");

            t.Wait(); // Thread의 Join과 동일 (Task 작업이 끝날 때 까지 기다림)
            Console.WriteLine("Success");
        }
    }
}
