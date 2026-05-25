namespace ServerCore
{
    class Program
    {
        static int a = 0;
        static int b = 0;
        static int ta = 0;
        static int tb = 0;

        static void FuncA()
        {
            a = 1; // Store a

            Thread.MemoryBarrier();

            tb = b; // Load b
        }

        static void FuncB()
        {
            b = 1; // Store b

            Thread.MemoryBarrier();

            ta = a; // Load a
        }

        static void Main(string[] args)
        {
            int count = 0;
            while(true)
            {
                a = b = ta = tb = 0;

                Task t1 = new Task(FuncA);
                Task t2 = new Task(FuncB);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                if(ta == 0 && tb == 0)
                {
                    count++;
                    break;
                }
            }

            Console.WriteLine($"{count}번 탈출");
        }
    }
}
