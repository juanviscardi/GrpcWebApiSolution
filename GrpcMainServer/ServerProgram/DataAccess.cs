using Common;
using System.Collections.Generic;

namespace GrpcMainServer.ServerProgram
{
    public class DataAccess
    {
        public List<Usuario> usuarios { get; set; }
        public List<string> categorias { get; set; }
        public List<Common.Repuesto> repuestos { get; set; }
        public List<Mensaje> mensajes { get; set; }
        private static DataAccess instance;
        private static readonly object singletonlock = new object();
        private int repuestoId;

        public static DataAccess GetInstance()
        {
            lock (singletonlock)
            {

                if (instance == null)
                {
                    instance = new DataAccess();
                    instance.usuarios = new List<Usuario>();
                    instance.categorias = new List<string>();
                    instance.repuestos = new List<Common.Repuesto>();
                    instance.mensajes = new List<Mensaje>();
                }
            }
            return instance;
        }

        public int NextRepuestoID
        {
            get
            {
                int res = repuestoId;
                repuestoId++;
                return repuestoId;

            }
            private set
            {
                repuestoId = value;
            }
        }

    }
}
