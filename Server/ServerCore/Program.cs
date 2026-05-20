namespace ServerCore
{
    internal class Program
    {
        static void MainThread()
        {
            for(int i=0; i<5; i++)
                Console.WriteLine("Thread Running!");
        }

        static void Main(string[] args)
        {
            Thread t = new Thread(MainThread);
            t.Name = "Test Thread";
            t.IsBackground = true;
            t.Start();

            Console.WriteLine("Thread Start!");

            t.Join();
            Console.WriteLine("End Complete!");
        }
    }
}
