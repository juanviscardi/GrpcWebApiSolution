using Common;

namespace LogServer.LogProgram
{
    public class DataAccess
    {
        private static readonly object singletonlock = new object();
        private static DataAccess instance;
        public List<Log> Logs { get; set; }

        public static DataAccess GetInstance()
        {
            lock (singletonlock)
            {

                if (instance == null)
                {
                    instance = new DataAccess();
                    instance.Logs = new List<Log> ();
                }
            }
            return instance;
        }

        
    }
}