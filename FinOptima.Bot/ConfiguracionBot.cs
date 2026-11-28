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
            configuracion.Token = LeerToken();
            configuracion.Correo = LeerRequerida("FINOPTIMA_BOT_CORREO");
            configuracion.Contrasena = LeerRequerida("FINOPTIMA_BOT_CONTRASENA");
            configuracion.ChatId = LeerChatId();
            return configuracion;
        }

        public static string LeerToken()
        {
            string token = LeerRequerida("FINOPTIMA_TELEGRAM_TOKEN");
            int separador = token.IndexOf(':');
            long idBot;

            if (separador <= 0 || separador == token.Length - 1 ||
                !long.TryParse(token.Substring(0, separador), out idBot) || idBot <= 0)
            {
                throw new InvalidOperationException("FINOPTIMA_TELEGRAM_TOKEN no tiene el formato de un token de BotFather (123456789:ABC...).");
            }

            return token;
        }

        public static long LeerChatId()
        {
            string chat = LeerRequerida("FINOPTIMA_TELEGRAM_CHAT");
            long numero;
            bool esNumero = long.TryParse(chat, out numero);

            if (!esNumero || numero == 0)
            {
                throw new InvalidOperationException("FINOPTIMA_TELEGRAM_CHAT debe ser un numero distinto de cero (el id del chat autorizado).");
            }

            return numero;
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
