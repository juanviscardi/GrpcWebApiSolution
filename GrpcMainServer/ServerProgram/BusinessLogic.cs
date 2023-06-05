using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GrpcMainServer.ServerProgram
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
                    instance = new BusinessLogic();
                instance.da = DataAccess.GetInstance();
            }
            return instance;
        }

        internal string CreateUser(string name)
        {
            System.Console.WriteLine("Voy a crear el usuario: {0}",name);
            List<User> users = da.Users;
            lock (users)
            {
                User newUser = new User()
                {
                    Name = name,
                    Id = da.NextUserID
                };
               
                bool alreadyExists = users.Contains(newUser);
                return alreadyExists ? "Ya existe el usuario" : "Usuario creado correctamente";
            }
        }

        internal bool DeleteUser(int id)
        {
            List<User> users = da.Users;
            bool success;
            lock (users)
            {
                User userToDelete = users.Find(i => i.Id == id);
                success = users.Remove(userToDelete);
            }
            return success;
        }
    }
}
