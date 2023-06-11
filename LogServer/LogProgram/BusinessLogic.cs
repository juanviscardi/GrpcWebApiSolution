using RabbitMQ.Client;
using Common;

namespace LogServer.LogProgram
{
    public class BusinessLogic
    {
        private static BusinessLogic instance;
        private DataAccess da;
        private static readonly object singletonlock = new object();
        public static BusinessLogic GetInstance()
        {
            lock (singletonlock)
            {
                if (instance == null)
                {
                    instance = new BusinessLogic();
                    instance.da = DataAccess.GetInstance();
                }
            }
            return instance;
        }

        public void AddLog(string message)
        {
            Log log = new Log(message);
            instance.da.Logs.Add(log);
        }

    }
}
