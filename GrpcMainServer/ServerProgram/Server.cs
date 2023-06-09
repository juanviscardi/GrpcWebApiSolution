﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using GrpcMainServer;
using GrpcMainServer.ServerProgram;
using Microsoft.AspNetCore.Hosting.Server;
using Repuesto = Common.Repuesto;


namespace GrpcSGrpcMainServer.ServerProgram
{
    public class Server
    {
        public bool acceptingConnections;
        private readonly TcpListener tcpListener;
        static readonly SettingsManager settingsMng = new SettingsManager();
        static readonly BusinessLogic session = BusinessLogic.GetInstance();


        public Server(string serverIpAddress, string serverPort)
        {

            var ipEndPoint = new IPEndPoint(
                 IPAddress.Parse(serverIpAddress), int.Parse(serverPort));

            tcpListener = new TcpListener(ipEndPoint);
            acceptingConnections = true;
        }

        public async Task StartReceivingConnections()
        {

            Console.WriteLine("######### Server Tcp Iniciado y aceptando conexiones ###########");
            tcpListener.Start(ProtocolSpecification.FixedBackpack);

            while (acceptingConnections)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var task = Task.Run(async () => await HandleClient(tcpClientSocket).ConfigureAwait(false));
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server no longer accepts requests");
                    acceptingConnections = false;
                }

            }


        }

        private static async Task HandleClient(TcpClient tcpClientSocket)
        {

            bool clientIsConnected = true;
            string userType = "error";
            string usernameConnected = "";
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(tcpClientSocket);
            string[] ipAddressPuerto = tcpClientSocket.Client.RemoteEndPoint.ToString().Split(":");
            Console.WriteLine("Tratando de Conectar desde IP: {0} usando el puerto: {1}", ipAddressPuerto[0], ipAddressPuerto[1]);

            while (clientIsConnected)
            {
                try
                {
                    if (string.Equals(userType, "error"))
                    {
                        // log in
                        string messageUsuarioContrasena = await networkdatahelper.Receive();
                        string[] usuarioContrasena = messageUsuarioContrasena.Split(ProtocolSpecification.fieldsSeparator);
                        string usuario = usuarioContrasena[0];
                        string pass = usuarioContrasena[1];
                        string adminUsername = settingsMng.ReadSetting(ServerConfig.usernameConfigKey);
                        string adminPassword = settingsMng.ReadSetting(ServerConfig.passwordConfigKey);
                        if (string.Equals(usuario, adminUsername) && string.Equals(pass, adminPassword))
                        {
                            usernameConnected = usuario;
                            userType = "admin";

                            Console.WriteLine();
                            Console.WriteLine("Usuario: {0} hizo login como Administrador", usuario);
                            Console.WriteLine();
                            session.CreateLog($"El usuario {usuario} hizo login como Administrador", Action.Retrive, usuario);
                            await networkdatahelper.Send("admin");
                        }
                        else
                        {
                            bool existeYEsValido = false;
                            session.GetUsuarios().ToList().ForEach(x =>
                            {
                                if (string.Equals(x.userName, usuario) && string.Equals(x.userPassword, pass))
                                {
                                    existeYEsValido = true;
                                    usernameConnected = usuario;
                                }
                            });
                            if (existeYEsValido)
                            {
                                userType = "mecanico";
                                Console.WriteLine("Usuario: {0} hizo login como Mecanico", usuario);
                                session.CreateLog($"El usuario {usuario} hizo login como Mecanico", Action.Retrive, usuario);
                                await networkdatahelper.Send("mecanico");
                            }
                            else
                            {
                                session.CreateLog($"El usuario {usuario} quiso ingresar con un usuario o password invalida", Action.Retrive, usuario);
                                Console.WriteLine("Usuario: {0} no se conecto", usuario);
                                await networkdatahelper.Send("error");
                            }
                        }
                        continue;
                    }
                    string cmd = await networkdatahelper.Receive();
                    switch (userType)
                    {
                        case "admin":
                            {
                                switch (cmd)
                                {
                                    case "1":
                                        //CRF1 Alta de usuario
                                        // Alta de usuario. Se debe poder dar de alta a un usuario (mecánico). 
                                        // Esta funcionalidad solo puede realizarse desde el usuario administrador.
                                        string altaUsuarioRequest = await networkdatahelper.Receive();
                                        string[] altaUsuarioRequestConTodo = altaUsuarioRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var userName = altaUsuarioRequestConTodo[0];
                                        var userPassword = altaUsuarioRequestConTodo[1];
                                        string respuestaCreateUserAsync = await session.CreateUserAsync(userName, userPassword);
                                        await networkdatahelper.Send(respuestaCreateUserAsync);
                                        break;
                                    case "2":
                                        // Salir;
                                        clientIsConnected = false;
                                        break;
                                }
                                break;
                            }
                        case "mecanico":
                            {

                                switch (cmd)
                                {
                                    case "1":
                                        Console.WriteLine("CRF2 Alta de repuesto.");
                                        Console.WriteLine("Se debe poder dar de alta a un repuesto en el sistema, incluyendo");
                                        Console.WriteLine("id, nombre, proveedor y marca.");
                                        string altaRepuestoRequest = await networkdatahelper.Receive();
                                        string[] altaRepuestoRequestConTodo = altaRepuestoRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var repuestoName = altaRepuestoRequestConTodo[0];
                                        var repuestoProveedor = altaRepuestoRequestConTodo[1];
                                        var repuestoMarca = altaRepuestoRequestConTodo[2];
                                        string respuestaCreateRepuestoAsync = await session.CreateRepuestoAsync(repuestoName, repuestoProveedor, repuestoMarca, usernameConnected);
                                        await networkdatahelper.Send(respuestaCreateRepuestoAsync);
                                        break;


                                    case "2":
                                        // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                        // SRF3.Crear Categoría de repuesto. El sistema debe permitir crear una categoría para los repuestos.
                                        // CRF3. Alta de Categoría de repuesto. El sistema debe permitir crear una Categoría para los repuestos.
                                        string categoria = await networkdatahelper.Receive();
                                        string respuestaCreateCategoriaAsync = await session.CreateCategoriaAsync(categoria, usernameConnected);
                                        await networkdatahelper.Send(respuestaCreateCategoriaAsync);
                                        break;
                                    case "3":
                                        //SRF4. Asociar Categorías a un repuesto. El sistema debe permitir asociar categorías a los repuestos.
                                        //CRF4. Asociar Categorías a los repuestos. El sistema debe permitir asociar categorías a losrepuestos.
                                        // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                        // envio nombre de repuestos existentes para que se listen en el cliente
                                        List<string> repuestosExistentesNamesResponse = new List<string>();
                                        session.GetRepuestos().ToList().ForEach(x =>
                                        {
                                            repuestosExistentesNamesResponse.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesNamesResponse));
                                        // envio nombre de categorias existentes para que se listen en el cliente
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, session.GetCategorias()));
                                        // recibo el nombre del repuesto elegio y la categoria elegida
                                        string asociarCategoriaRequest = await networkdatahelper.Receive();
                                        if (string.Equals(asociarCategoriaRequest, "exit"))
                                        {
                                            break;
                                        }
                                        string[] asociarCategoriaRequestConTodo = asociarCategoriaRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        string repuestoName4 = asociarCategoriaRequestConTodo[0];
                                        string categoria4 = asociarCategoriaRequestConTodo[1];
                                        string respuestaAsociarCategoriaAsync = await session.AsociarCategoriaAsync(repuestoName4, categoria4, usernameConnected);
                                        await networkdatahelper.Send(respuestaAsociarCategoriaAsync);
                                        break;
                                    case "4":
                                        //SRF5 Asociar una foto al repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                        //CRF5. Asociar foto a repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.

                                        List<string> repuestosExistentesParaFotoResponse = new List<string>();
                                        session.GetRepuestos().ToList().ForEach(x =>
                                        {
                                            repuestosExistentesParaFotoResponse.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesParaFotoResponse));
                                        string nombreRepuestoOExit = await networkdatahelper.Receive();
                                        if (string.Equals(nombreRepuestoOExit, "exit"))
                                        {
                                            break;
                                        }
                                        var repuesto5 = session.GetRepuestos().Find(x => string.Equals(nombreRepuestoOExit, x.Name));
                                        if (repuesto5 != null)
                                        {
                                            await networkdatahelper.Send("El repuesto existe.");
                                        }
                                        else
                                        {
                                            session.CreateLog($"El repuesto {nombreRepuestoOExit} al que se le quiere asociar una foto no existe", Action.Modify, usernameConnected, Status.Error);
                                            await networkdatahelper.Send("El repuesto no existe.");
                                            break;
                                        }
                                        FileCommsHandler fileCommsHandler = new FileCommsHandler(tcpClientSocket);
                                        string nombreArchivo = await fileCommsHandler.ReceiveFile();
                                        // el archivo queda guardado en el bin
                                        repuesto5.Foto = nombreArchivo;
                                        session.CreateLog($"Al repuesto {nombreRepuestoOExit} se le asocio una foto", Action.Modify, usernameConnected);
                                        await networkdatahelper.Send("Se asocio la foto al repuesto.");
                                        break;
                                    case "5":
                                        // SRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes,incluyendo búsquedas por palabras claves.
                                        // CRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.

                                        string opcionListado = await networkdatahelper.Receive();
                                        List<string> repuestosExistentesResponse = new List<string>();
                                        switch (opcionListado)
                                        {
                                            case "1":
                                                // Console.WriteLine("1 - Listar todos");
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    repuestosExistentesResponse.Add(x.ToStringListar());
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "2":
                                                // Console.WriteLine("2 - Buscar por nombre repuesto");
                                                string opcionListadoNombre = await networkdatahelper.Receive();
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Name, opcionListadoNombre))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto con el nombre {opcionListadoNombre}", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "3":
                                                // Console.WriteLine("3 - Buscar por categoria");
                                                string opcionListadoCategoria = await networkdatahelper.Receive();
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (x.Categorias.Contains(opcionListadoCategoria))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto con la categoria {opcionListadoCategoria}", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "4":
                                                // Console.WriteLine("4 - Buscar por nombre archivo foto");
                                                string opcionListadoFoto = await networkdatahelper.Receive();
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Foto, opcionListadoFoto))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto con la foto deseada", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "5":
                                                // Console.WriteLine("5 - Buscar por nombre de proveedor");
                                                string opcionListadoProveedor = await networkdatahelper.Receive();
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Proveedor, opcionListadoProveedor))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto con el proveedor {opcionListadoProveedor}", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "6":
                                                // Console.WriteLine("6 - Buscar por nombre de marca")
                                                string opcionListadoMarca = await networkdatahelper.Receive();
                                                session.GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Marca, opcionListadoMarca))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                session.CreateLog($"Se imprimen todos los repuesto con la marca {opcionListadoMarca}", Action.Retrive, usernameConnected);
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                        }
                                        break;
                                    case "6":
                                        // SRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.

                                        // CRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.

                                        List<string> nombreRepuestosExistentes = new List<string>();
                                        session.GetRepuestos().ToList().ForEach(x =>
                                        {
                                            nombreRepuestosExistentes.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreRepuestosExistentes));
                                        string nombreRepuestoQuieroDetalles = await networkdatahelper.Receive();
                                        if (string.Equals(nombreRepuestoQuieroDetalles, "exit"))
                                        {
                                            break;
                                        }
                                        var repuesto6 = session.GetRepuestos().Find(x => string.Equals(nombreRepuestoQuieroDetalles, x.Name));
                                        if (repuesto6 != null)
                                        {
                                            bool tieneFoto = true;
                                            if (string.IsNullOrEmpty(repuesto6.Foto))
                                            {
                                                tieneFoto = false;
                                            }
                                            string response = repuesto6.ToStringListar() + ProtocolSpecification.fieldsSeparator + tieneFoto;
                                            await networkdatahelper.Send(response);
                                        }
                                        else
                                        {
                                            session.CreateLog($"El repuesto de nombre  {nombreRepuestoQuieroDetalles} no existe", Action.Retrive, usernameConnected, Status.Error);
                                            await networkdatahelper.Send("El repuesto no existe.");
                                        }
                                        string enviarFoto = await networkdatahelper.Receive();
                                        if (string.Equals(enviarFoto, "NO")) break;
                                        FileCommsHandler fileCommsHandler2 = new FileCommsHandler(tcpClientSocket);
                                        await fileCommsHandler2.SendFile(repuesto6.Foto);
                                        session.CreateLog($"Se descargo la foto del repuesto {nombreRepuestoQuieroDetalles}", Action.Retrive, usernameConnected);

                                        break;
                                    case "7":

                                        //SRF8. Enviar y recibir mensajes entre mecánicos. El sistema debe permitir que un mecánico
                                        //envíe mensajes a otro, y que el mecánico receptor chequee sus mensajes sin leer, así como
                                        //también revisar su historial de mensajes.

                                        // mandar todos los mecanicos
                                        List<string> nombreMecanicos = new List<string>();
                                        session.GetUsuarios().ToList().ForEach(x =>
                                        {
                                            nombreMecanicos.Add(x.userName);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreMecanicos));
                                        string requestEnviarMensaje = await networkdatahelper.Receive();
                                        // validar inputs
                                        if (string.Equals(requestEnviarMensaje, "exit")) break;
                                        Mensaje mensajeEnviado = new Mensaje(requestEnviarMensaje);
                                        if (string.Equals(mensajeEnviado.remitente, mensajeEnviado.destinatario))
                                        {
                                            session.CreateLog($"El usuario {mensajeEnviado.destinatario} no puede enviarse un mensaje a si mismo", Action.Create, usernameConnected, Status.Error);
                                            await networkdatahelper.Send("No se puede enviar un mensaje a si mismo");
                                            break;
                                        }
                                        var usuarioDestintario = session.GetUsuarios().Find(x => string.Equals(mensajeEnviado.destinatario, x.userName));
                                        if (usuarioDestintario == null)
                                        {
                                            session.CreateLog($"El usuario {mensajeEnviado.destinatario} no es un mecanico valido", Action.Create, usernameConnected, Status.Error);
                                            await networkdatahelper.Send("El destinatario no es un mecanico valido.");
                                            break;
                                        }
                                        if (string.IsNullOrEmpty(mensajeEnviado.cuerpoMensaje))
                                        {
                                            session.CreateLog($"El cuerpo del mensaje no puede estar vacio", Action.Create, usernameConnected, Status.Error);
                                            await networkdatahelper.Send("El cuerpo del mensaje no puede estar vacio.");
                                            break;
                                        }
                                        // enviar respuesta a imprimir en el cliente
                                        session.GetMensajes().Add(mensajeEnviado);
                                        mensajeEnviado.ImprimirServer();
                                        session.CreateLog($"El usuario {mensajeEnviado.remitente} le envio un mensaje a {mensajeEnviado.destinatario}", Action.Create, usernameConnected);
                                        await networkdatahelper.Send("Mensaje enviado. ");
                                        // CRF8.Enviar y recibir mensajes. El sistema debe permitir que un mecánico envíe mensajes
                                        // a otro, y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.

                                        break;
                                    case "8":
                                        // Console.WriteLine("8 - Leer Mensajes");

                                        // receive opcion 1 para mensajes nuevo o 2 para todos, exit en otro caso
                                        // devolver los mensajes pedidos y marcarlos como leido = true
                                        string opcionElegidoLeer = await networkdatahelper.Receive();
                                        if (string.Equals(opcionElegidoLeer, "exit"))
                                        {
                                            break;
                                        }
                                        // Console.WriteLine("1 - Leer nuevos mensajes");
                                        List<Mensaje> mensajesNuevos;
                                        if (string.Equals(opcionElegidoLeer, "1"))
                                        {
                                            session.CreateLog($"El usuario consulta sus mensajes nuevos", Action.Retrive, usernameConnected);
                                            mensajesNuevos = session.GetMensajes().FindAll(x => string.Equals(x.destinatario, usernameConnected) && !x.visto);
                                            await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
                                        }
                                        else
                                        {
                                            session.CreateLog($"El usuario consulta todos sus mensajes", Action.Retrive, usernameConnected);
                                            mensajesNuevos = session.GetMensajes().FindAll(x => string.Equals(x.destinatario, usernameConnected));
                                            await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
                                        }
                                        mensajesNuevos.ForEach(x => x.visto = true);
                                        Console.WriteLine("Mensajes enviados al cliente: ");
                                        mensajesNuevos.ForEach(x => x.ImprimirServer());
                                        break;
                                    case "9":
                                        // Console.WriteLine("9 - Salir");
                                        break;
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                catch
                {
                    Console.WriteLine("ERROR: Cliente desconectado de manera forzada \n" + tcpClientSocket.Client.RemoteEndPoint.ToString() + "\n");
                    clientIsConnected = false;
                }

            }
            tcpClientSocket.Client.Close();
            Console.WriteLine("Cliente desconectado.");

        }
    }
}
