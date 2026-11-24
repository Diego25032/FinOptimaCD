using System.Text;

namespace FinOptima.Bot
{
    public static class JsonTelegram
    {
        public static string Escapar(string texto)
        {
            if (texto == null)
            {
                return "";
            }

            StringBuilder resultado = new StringBuilder();

            foreach (char letra in texto)
            {
                switch (letra)
                {
                    case '"':
                        resultado.Append("\\\"");
                        break;
                    case '\\':
                        resultado.Append("\\\\");
                        break;
                    case '\n':
                        resultado.Append("\\n");
                        break;
                    case '\r':
                        resultado.Append("\\r");
                        break;
                    case '\t':
                        resultado.Append("\\t");
                        break;
                    default:
                        if (letra < 32)
                        {
                            resultado.Append(' ');
                        }
                        else
                        {
                            resultado.Append(letra);
                        }
                        break;
                }
            }

            return resultado.ToString();
        }
    }
}
