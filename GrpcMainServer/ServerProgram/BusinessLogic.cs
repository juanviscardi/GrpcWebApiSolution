using Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using static GrpcMainServer.Repuesto;

namespace GrpcMainServer.ServerProgram
{
    public class BusinessLogic
    {
        private static BusinessLogic instance;
        private DataAccess da;
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
                //await networkdatahelper.Send("exito");
                //Console.WriteLine("Se Creo un nuevo usuario");
                //Console.WriteLine(user.ToString());
                //Console.WriteLine();
            }
            else
            {
                respuesta = "el usuario ya existe";
                //await networkdatahelper.Send("el usuario ya existe");
            }

            _agregarUsuario.Release();
            return respuesta;
        }

        internal async Task<string> CreateRepuestoAsync(string name, string proveedor, string marca)
        {
            string respuesta = "";
            Common.Repuesto repu = new Common.Repuesto(
                                                          this.GetRepuestos().Count().ToString(),
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
    }
}
