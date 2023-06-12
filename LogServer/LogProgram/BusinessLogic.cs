using RabbitMQ.Client;
using Common;
using Microsoft.AspNetCore.Mvc;

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

        internal List<Log> GetLogs(string? contains = null,
            string? from = null,
            string? until = null,
            string? status = null,
            string? action = null,
            string? userName = null)
        {
            List<Log> logs = instance.da.Logs;

            if (!string.IsNullOrEmpty(contains))
            {
                logs = logs.Where(log => log.Mensaje.Contains(contains)).ToList();
            }

            if (!string.IsNullOrEmpty(from))
            {
                DateTime fromDate;
                if (DateTime.TryParse(from, out fromDate))
                {
                    logs = logs.Where(log => log.Date >= fromDate).ToList();
                }
            }

            if (!string.IsNullOrEmpty(until))
            {
                DateTime untilDate;
                if (DateTime.TryParse(until, out untilDate))
                {
                    logs = logs.Where(log => log.Date <= untilDate).ToList();
                }
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out Status logStatus))
                {
                    logs = logs.Where(log => log.Status == logStatus).ToList();
                }
            }

            if (!string.IsNullOrEmpty(action))
            {
                if (Enum.TryParse(action, out Action logAction))
                {
                    logs = logs.Where(log => log.Action == logAction).ToList();
                }
            }

            if (!string.IsNullOrEmpty(userName))
            {
                logs = logs.Where(log => log.UserName == userName).ToList();
            }

            return logs;
        }

        public void AddLog(string message)
        {
            Log log = new Log(message);
            this.GetLogs().Add(log);
        }

    }
}
