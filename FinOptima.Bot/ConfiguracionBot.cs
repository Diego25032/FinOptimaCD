using System;

namespace FinOptima.Bot
{
    public class ConfiguracionBot
    {
        public string Token { get; private set; }
        public long ChatId { get; private set; }
        public string Correo { get; private set; }
        public string Contrasena { get; private set; }

        public ConfiguracionBot()
        {
            Token = "";
            ChatId = 0;
            Correo = "";
            Contrasena = "";
        }

        public static ConfiguracionBot Leer()
        {
            ConfiguracionBot configuracion = new ConfiguracionBot();
            configuracion.Token = LeerRequerida("FINOPTIMA_TELEGRAM_TOKEN");
            configuracion.Correo = LeerRequerida("FINOPTIMA_BOT_CORREO");
            configuracion.Contrasena = LeerRequerida("FINOPTIMA_BOT_CONTRASENA");

            string chat = LeerRequerida("FINOPTIMA_TELEGRAM_CHAT");
            long numero;
            bool esNumero = long.TryParse(chat, out numero);

            if (!esNumero)
            {
                throw new InvalidOperationException("FINOPTIMA_TELEGRAM_CHAT debe ser un numero (el id del chat autorizado).");
            }

            configuracion.ChatId = numero;
            return configuracion;
        }

        private static string LeerRequerida(string nombre)
        {
            string valor = Environment.GetEnvironmentVariable(nombre);

            if (valor == null)
            {
                throw new InvalidOperationException("Falta la variable de entorno " + nombre + ".");
            }

            string texto = valor.Trim();

            if (texto.Length == 0)
            {
                throw new InvalidOperationException("La variable de entorno " + nombre + " esta vacia.");
            }

            return texto;
        }
    }
}
