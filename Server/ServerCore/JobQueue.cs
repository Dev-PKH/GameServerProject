namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> jobQueue = new();
        object lockObj = new();
        bool flush;

        public void Push(Action job)
        {
            bool flush = false;

            lock (lockObj)
            {
                jobQueue.Enqueue(job);
                if(this.flush == false)
                {
                    flush = this.flush = true;
                }
            }

            if(flush)
                Flush();
        }

        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock(lockObj)
            {
                if (jobQueue.Count == 0)
                {
                    flush = false;
                    return null;
                }

                return jobQueue.Dequeue();
            }
        }
    }
}
