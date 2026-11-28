using System;
using System.Drawing;
using System.Threading.Tasks;
using FinOptima.Bot;
using FinOptima.Logica.Composicion;
using QRCoder;

namespace FinOptima.Vista
{
    public sealed class ServicioBotLocal : IDisposable
    {
        private readonly FabricaServicios servicios;
        private readonly object sincronizacion;
        private BotLocal bot;
        private string nombreUsuario;
        private Exception error;
        private bool detenido;
        private bool inicioEnCurso;

        public ServicioBotLocal(FabricaServicios fabrica)
        {
            servicios = fabrica;
            sincronizacion = new object();
        }

        public bool EstaActivo
        {
            get
            {
                lock (sincronizacion)
                {
                    return !detenido && bot != null && bot.EstaActivo;
                }
            }
        }

        public bool InicioEnCurso
        {
            get
            {
                lock (sincronizacion)
                {
                    return inicioEnCurso;
                }
            }
        }

        public string NombreUsuario
        {
            get
            {
                lock (sincronizacion)
                {
                    return nombreUsuario ?? "";
                }
            }
        }

        public string EnlaceTelegram
        {
            get
            {
                string nombre = NombreUsuario.TrimStart('@');
                return nombre.Length == 0 ? "" : "https://t.me/" + nombre + "?start=finoptima";
            }
        }

        public Exception Error
        {
            get
            {
                lock (sincronizacion)
                {
                    if (error != null)
                    {
                        return error;
                    }

                    return bot == null ? null : bot.Error;
                }
            }
        }

        public string Estado
        {
            get
            {
                if (EstaActivo)
                {
                    return "Bot activo";
                }

                if (InicioEnCurso)
                {
                    return "Iniciando bot...";
                }

                return Error == null ? "Bot inactivo" : "No se pudo iniciar el bot";
            }
        }

        public Task IniciarAsync()
        {
            BotLocal anterior;

            lock (sincronizacion)
            {
                if (detenido || inicioEnCurso || (bot != null && bot.EstaActivo))
                {
                    return Task.CompletedTask;
                }

                anterior = bot;
                bot = null;
                error = null;
                inicioEnCurso = true;
            }

            anterior?.Dispose();
            return Task.Run(EjecutarInicio);
        }

        private void EjecutarInicio()
        {
            BotLocal nuevo = null;
            bool descartar = false;

            try
            {
                string token = ConfiguracionBot.LeerToken();
                long chatId = ConfiguracionBot.LeerChatId();
                string nombre;

                using (ClienteTelegram cliente = new ClienteTelegram(token))
                {
                    nombre = cliente.ObtenerNombreUsuario();
                }

                lock (sincronizacion)
                {
                    if (detenido)
                    {
                        descartar = true;
                        return;
                    }

                    nombreUsuario = nombre;
                }

                nuevo = new BotLocal(servicios, token, chatId);
                nuevo.Iniciar();

                lock (sincronizacion)
                {
                    if (detenido)
                    {
                        descartar = true;
                    }
                    else
                    {
                        bot = nuevo;
                    }
                }
            }
            catch (Exception excepcion)
            {
                lock (sincronizacion)
                {
                    error = excepcion;
                }
            }
            finally
            {
                if (descartar && nuevo != null)
                {
                    nuevo.Dispose();
                }

                lock (sincronizacion)
                {
                    inicioEnCurso = false;
                }
            }
        }

        public Bitmap CrearQr(int tamano)
        {
            string enlace = EnlaceTelegram;

            if (enlace.Length == 0)
            {
                throw new InvalidOperationException("Todavia no se conoce el nombre del bot de Telegram.");
            }

            using (QRCodeGenerator generador = new QRCodeGenerator())
            using (QRCodeData datos = generador.CreateQrCode(enlace, QRCodeGenerator.ECCLevel.Q))
            using (QRCode codigo = new QRCode(datos))
            using (Bitmap imagen = codigo.GetGraphic(8, Color.Black, Color.White, true))
            {
                Bitmap resultado = new Bitmap(imagen, new Size(tamano, tamano));
                return resultado;
            }
        }

        public void Detener()
        {
            BotLocal actual;

            lock (sincronizacion)
            {
                if (detenido)
                {
                    return;
                }

                detenido = true;
                actual = bot;
                bot = null;
            }

            actual?.Dispose();
        }

        public void Dispose()
        {
            Detener();
        }
    }
}
