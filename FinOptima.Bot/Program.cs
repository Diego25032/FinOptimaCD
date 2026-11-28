using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;

namespace FinOptima.Bot
{
    public static class Program
    {
        public static int Main(string[] argumentos)
        {
            Console.OutputEncoding = Encoding.UTF8;

            ConfiguracionBot configuracion;

            try
            {
                configuracion = ConfiguracionBot.Leer();
            }
            catch (InvalidOperationException error)
            {
                MostrarError(error.Message);
                return 1;
            }

            Console.WriteLine("FinOptima Bot - conectando con la base de datos...");

            FabricaServicios servicios = new FabricaServicios();

            try
            {
                servicios.ProbarConexion();
            }
            catch (Exception error)
            {
                MostrarError("No se pudo conectar a la base de datos: " + error.Message);
                return 1;
            }

            ResultadoLogin sesion = servicios.Autenticacion.IniciarSesion(configuracion.Correo, configuracion.Contrasena);

            if (!sesion.Exito)
            {
                MostrarError("No se pudo iniciar sesion con la cuenta del bot: " + sesion.Mensaje);
                return 1;
            }

            Console.WriteLine("Sesion iniciada como " + sesion.Usuario.NombreCompleto + " (" + sesion.Usuario.Correo + ").");
            Console.WriteLine("Chat autorizado: " + configuracion.ChatId);
            Console.WriteLine("Comprobando la conexion con Telegram...");

            ClienteTelegram cliente = new ClienteTelegram(configuracion.Token);

            try
            {
                cliente.ObtenerActualizaciones(0, 0);
            }
            catch (Exception error)
            {
                MostrarError("No se pudo activar Telegram: " + error.Message + ". Verifica el token, cierra otras copias del bot y elimina cualquier webhook activo; este bot funciona por polling.");
                return 1;
            }

            Console.WriteLine("Bot escuchando mensajes. Envia /start en Telegram para ver los botones. Presiona Ctrl+C para salir.");

            try
            {
                cliente.ConfigurarComandos();
            }
            catch (Exception error)
            {
                Console.WriteLine("Aviso: no se pudo actualizar el menu de comandos de Telegram: " + error.Message);
            }

            ManejadorBot manejador = new ManejadorBot(cliente, servicios, configuracion.ChatId);
            int ultimoNumero = 0;

            while (true)
            {
                try
                {
                    List<ActualizacionTelegram> actualizaciones = cliente.ObtenerActualizaciones(ultimoNumero, 25);

                    foreach (ActualizacionTelegram actualizacion in actualizaciones)
                    {
                        try
                        {
                            manejador.Procesar(actualizacion);
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine("Error al procesar la actualizacion " + actualizacion.Numero + ": " + error.Message);
                        }

                        ultimoNumero = actualizacion.Numero + 1;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine("Error de conexion con Telegram: " + error.Message);
                    Thread.Sleep(4000);
                }
            }
        }

        private static void MostrarError(string mensaje)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + mensaje);
            Console.WriteLine();
            Console.WriteLine("Variables de entorno necesarias:");
            Console.WriteLine("  FINOPTIMA_TELEGRAM_TOKEN   Token del bot (BotFather)");
            Console.WriteLine("  FINOPTIMA_TELEGRAM_CHAT    Id del chat autorizado (obtenlo con @userinfobot)");
            Console.WriteLine("  FINOPTIMA_BOT_CORREO       Correo de la cuenta de FinOptima");
            Console.WriteLine("  FINOPTIMA_BOT_CONTRASENA   Contrasena de esa cuenta");
            Console.WriteLine("  FINOPTIMA_CONEXION         Cadena de conexion (opcional; usa conexion.txt o localhost:5433)");
        }
    }
}
