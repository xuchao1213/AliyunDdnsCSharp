using System.Collections.Generic;

namespace AliyunDdnsCSharp.Core
{
    public class DdnsWorkerManager
    {
        #region static internal class Singleton
        private DdnsWorkerManager() { workers=new Dictionary<string, DddnsWorker>();}
        private class IntanceHolder
        {
            public static readonly DdnsWorkerManager Instance = new DdnsWorkerManager();
            static IntanceHolder() { }
        }
        public static DdnsWorkerManager GetIns()
        {
            return IntanceHolder.Instance;
        }
        #endregion

        private readonly Dictionary<string, DddnsWorker> workers;

        public void Add(string name ,DddnsWorker worker)
        {
            if (workers.ContainsKey(name))
            {
                workers.Remove(name);
            }
            workers.Add(name,worker);
        }

        public void Remove(string name)
        {
            if (workers.ContainsKey(name))
            {
                workers.Remove(name);
            }
        }

        public void RunAll()
        {
            foreach (var worker in workers)
            {
                worker.Value.Run();
            }
        }

        public void StopAll()
        {
            foreach (var worker in workers)
            {
                worker.Value.Stop();
            }
        }
    }
}
