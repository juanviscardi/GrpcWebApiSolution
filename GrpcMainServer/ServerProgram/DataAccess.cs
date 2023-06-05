using System.Collections.Generic;

namespace GrpcMainServer.ServerProgram
{
    public class DataAccess
    {
        public List<User> Users { get; private set; }
        private static DataAccess instance;
        private static readonly object singletonlock = new object();
        private int userId;

        public static DataAccess GetInstance()
        {
            lock (singletonlock)
            {

                if (instance == null)
                    instance = new DataAccess();
                    instance.Users = new List<User>();
            }
            return instance;
        }

        public int NextUserID
        {
            get
            {
                int res = userId;
                userId++;
                return res;

            }
            private set
            {
                userId = value;
            }
        }

    }
}
