using Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using static GrpcMainServer.Repuesto;
using RabbitMQ.Client;
using System.Text;

namespace GrpcMainServer.ServerProgram
{
    public class BusinessLogic
    {
        private static BusinessLogic instance;
        private DataAccess da;
        private IModel channel;
        private static readonly object singletonlock = new object();
        private static readonly SemaphoreSlim _agregarUsuario = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _agregarRepuesto = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _agregarCategoria = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _asociarCategoria = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public static BusinessLogic GetInstance()
        {
            lock (singletonlock)
            {
                if (instance == null)
                {
                    instance = new BusinessLogic();
                    instance.da = DataAccess.GetInstance();
                    var factory = new ConnectionFactory()
                     {
                         HostName = "localhost", // Dirección del servidor RabbitMQ
                         Port = 5672, // Puerto del servidor RabbitMQ
                         UserName = "guest", // Nombre de usuario
                         Password = "guest", // Contraseña
                         VirtualHost = "/", // Virtual host (por defecto: "/")
                         RequestedConnectionTimeout = TimeSpan.FromSeconds(10), // Tiempo de espera para la conexión
                         RequestedHeartbeat = TimeSpan.FromSeconds(10), // Intervalo de latido del corazón
                     };
                    var connection = factory.CreateConnection();
                    instance.channel = connection.CreateModel();
                    instance.channel.QueueDeclare(queue: "logs",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                }
            }
            return instance;
        }

        internal List<Common.Repuesto> GetRepuestos()
        {
            return da.repuestos;
        }

        internal Common.Repuesto GetRepuestoById(string id)
        {
            return da.repuestos.FirstOrDefault(x => x.Id == id);
        }

        internal List<Common.Usuario> GetUsuarios()
        {
            return da.usuarios;
        }

        internal List<Common.Mensaje> GetMensajes()
        {
            return da.mensajes;
        }

        internal List<string> GetCategorias()
        {
            return da.categorias;
        }

        internal async Task<string> CreateUserAsync(string name, string password)
        {
            Usuario user = new Usuario(name, password, "mecanico");
            await _agregarUsuario.WaitAsync();
            string respuesta = "";
            if (!this.GetUsuarios().Contains(user))
            {
                this.GetUsuarios().Add(user);
                respuesta = "exito";
                _ = CreateLog($"Se ha creado el usuario {name}", Action.Create, "admin");
            }
            else
            {
                respuesta = "el usuario ya existe";
                _ = CreateLog($"El usuario {name} ya existe", Action.Create, "admin", Status.Error);
            }

            _agregarUsuario.Release();
            return respuesta;
        }

        internal async Task<string> CreateRepuestoAsync(string name, string proveedor, string marca)
        {
            string respuesta = "";
            Common.Repuesto repu = new Common.Repuesto(
                                                           this.da.NextRepuestoID.ToString(),
                                                           name,
                                                           proveedor,
                                                           marca);
            await _agregarRepuesto.WaitAsync();

            if (!this.GetRepuestos().Contains(repu))
            {
                this.GetRepuestos().Add(repu);
                respuesta = "exito";
            }
            else
            {
                respuesta = "el repuesto ya existe";
            }
            _agregarRepuesto.Release();
            return respuesta;
        }

        internal async Task<string> DeleteRepuestoAsync(string id)
        {
            Common.Repuesto respuestoAEliminar = this.GetRepuestoById(id);
            if (respuestoAEliminar == null)
            {
                return "No existe";
            }
            await _asociarCategoria.WaitAsync();
            da.repuestos.Remove(respuestoAEliminar);
            _asociarCategoria.Release();
            return "Eliminado";
        }

        internal async Task<string> PutRepuestoAsync(RepuestoDTO repuestoDTO)
        {
            Common.Repuesto respuestoAModificar = this.GetRepuestoById(repuestoDTO.Id);
            if (respuestoAModificar == null)
            {
                return "No existe";
            }
            await _asociarCategoria.WaitAsync();
            da.repuestos.ForEach(x =>
            {
                if (x.Id == repuestoDTO.Id)
                {
                    x.Name = repuestoDTO.Name;
                    x.Proveedor = repuestoDTO.Proveedor;
                    x.Marca = repuestoDTO.Marca;
                }
            });
            _asociarCategoria.Release();
            return "Modificado";
        }

        internal async Task<string> CreateCategoriaAsync(string categoria)
        {
            string respuesta = "";
            await _agregarCategoria.WaitAsync();

            if (!this.GetCategorias().Contains(categoria))
            {
                this.GetCategorias().Add(categoria);
                respuesta = "exito";
            }
            else
            {
                respuesta = "la categoria ya existe";
            }
            _agregarCategoria.Release();
            return respuesta;
        }

        internal async Task<string> AsociarCategoriaAsync(string respuestoName, string categoria)
        {
            string respuesta = "";
            await _asociarCategoria.WaitAsync();

            var repuesto = this.GetRepuestos().Find(x => string.Equals(respuestoName, x.Name));
            if (repuesto != null)
            {
                if (!repuesto.Categorias.Contains(categoria)) // Verifica si la categoría ya está presente en el Repuesto
                {
                    if (!this.GetCategorias().Contains(categoria))
                    {
                        respuesta = "La categoría no existe";
                        _asociarCategoria.Release();
                        return respuesta;
                    }
                    repuesto.Categorias.Add(categoria); // Agrega la nueva categoría al Repuesto
                    respuesta = "exito";
                }
                else
                {
                    respuesta = "La categoría ya está presente en el Repuesto.";
                }
            }
            else
            {
                respuesta = "El repuesto no existe.";
            }

            _asociarCategoria.Release();
            return respuesta;
        }

        internal async Task<string> PatchRepuestoAsync(string id)
        {
            Common.Repuesto respuestoAModificar = this.GetRepuestoById(id);
            if (respuestoAModificar == null)
            {
                return "No existe";
            }
            await _asociarCategoria.WaitAsync();
            da.repuestos.ForEach(x =>
            {
                if (x.Id == id)
                {
                    x.Foto = "";
                }
            });
            _asociarCategoria.Release();
            return "Modificado";
        }

        internal async Task CreateLog(string mensaje, Action action, string userName, Status status = Status.Ok)
        {
            Log newLog = new Log(mensaje, action, userName, status);
            var body = Encoding.UTF8.GetBytes(newLog.ToString());
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "",
                routingKey: "logs",
                basicProperties: properties,
                body: body);
        }
    }
}
