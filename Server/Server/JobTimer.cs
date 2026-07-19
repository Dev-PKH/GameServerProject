using ServerCore;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int executeTick; // 실행 시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.executeTick - executeTick;
        }
    }

    class JobTimer
    {
        PriorityQueue<JobTimerElem> pq = new();
        object lockObj = new();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.executeTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock(lockObj)
            {
                pq.Push(job);
            }
        }

        public void Flush()
        {
            while(true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;

                lock(lockObj)
                {
                    if (pq.Count == 0)
                        break;

                    job = pq.Peek();
                    
                    // 아직 실행시간 x
                    if(job.executeTick > now)
                        break;

                    job = pq.Pop();
                }
                
                job.action.Invoke();
            }
        }
    }
}
