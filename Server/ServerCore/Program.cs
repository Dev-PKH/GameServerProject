namespace ServerCore
{
    class Program
    {
        static int number = 0;
        static object obj = new();

        static void Thread1()
        {
            for (int i = 0; i < 10000; i++)
            {
                Monitor.Enter(obj);
                number++;
                Monitor.Exit(obj);
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 10000; i++)
            {
                Monitor.Enter(obj);
                number--;
                Monitor.Exit(obj);
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread1);
            Task t2 = new Task(Thread2);
        
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
