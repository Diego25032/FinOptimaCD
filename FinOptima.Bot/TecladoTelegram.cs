using System.Collections.Generic;
using System.Text;

namespace FinOptima.Bot
{
    /// <summary>
    /// Construye los botones de Telegram. Hay dos tipos:
    /// - Inline: botones pegados al mensaje que envian un dato (callback) al bot.
    /// - Respuesta: teclado fijo en la parte inferior del chat que envia el texto del boton.
    /// </summary>
    public class TecladoTelegram
    {
        private List<List<string>> filas;
        private bool esInline;

        private TecladoTelegram(bool inline)
        {
            esInline = inline;
            filas = new List<List<string>>();
            filas.Add(new List<string>());
        }

        public static TecladoTelegram Inline()
        {
            return new TecladoTelegram(true);
        }

        public static TecladoTelegram Respuesta()
        {
            return new TecladoTelegram(false);
        }

        public bool EsInline
        {
            get { return esInline; }
        }

        public int CantidadBotones
        {
            get
            {
                int total = 0;

                foreach (List<string> fila in filas)
                {
                    total = total + fila.Count;
                }

                return total;
            }
        }

        public TecladoTelegram Fila()
        {
            if (filas[filas.Count - 1].Count > 0)
            {
                filas.Add(new List<string>());
            }

            return this;
        }

        public TecladoTelegram Boton(string texto, string dato)
        {
            string json;

            if (esInline)
            {
                json = "{\"text\":\"" + JsonTelegram.Escapar(texto) + "\",\"callback_data\":\"" + JsonTelegram.Escapar(dato) + "\"}";
            }
            else
            {
                json = "{\"text\":\"" + JsonTelegram.Escapar(texto) + "\"}";
            }

            filas[filas.Count - 1].Add(json);
            return this;
        }

        public TecladoTelegram Boton(string texto)
        {
            return Boton(texto, texto);
        }

        public string ToJson()
        {
            StringBuilder resultado = new StringBuilder();
            resultado.Append(esInline ? "{\"inline_keyboard\":[" : "{\"keyboard\":[");

            bool primeraFila = true;

            foreach (List<string> fila in filas)
            {
                if (fila.Count == 0)
                {
                    continue;
                }

                if (!primeraFila)
                {
                    resultado.Append(',');
                }

                resultado.Append('[');
                resultado.Append(string.Join(",", fila));
                resultado.Append(']');
                primeraFila = false;
            }

            resultado.Append(']');

            if (!esInline)
            {
                resultado.Append(",\"resize_keyboard\":true,\"is_persistent\":true,\"input_field_placeholder\":\"Ej: Gasté 25000 en mercado\"");
            }

            resultado.Append('}');
            return resultado.ToString();
        }
    }
}
