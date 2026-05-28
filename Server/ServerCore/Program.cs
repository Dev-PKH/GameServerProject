namespace ServerCore
{
    class SessionManager
    {
        static object lockObj = new();

        public static void TestSession()
        {
            lock(lockObj)
            {

            }
        }

        public static void Test()
        {
            lock(lockObj)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object lockObj = new();

        public static void Test()
        {
            lock(lockObj)
            {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock(lockObj)
            {
                
            }
        }
    }

    class Program
    {
        static int number = 0;
        static object obj = new();

        static void Thread1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }

        static void Thread2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();  
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
