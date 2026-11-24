using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FinOptima.Bot
{
    public class ClienteTelegram : ICanalTelegram
    {
        private string token;
        private HttpClient cliente;

        public ClienteTelegram(string tokenTelegram)
        {
            token = tokenTelegram;
            cliente = new HttpClient();
            cliente.Timeout = TimeSpan.FromSeconds(70);
        }

        public List<ActualizacionTelegram> ObtenerActualizaciones(int ultimoNumero, int esperaSegundos)
        {
            string url = Direccion("getUpdates")
                + "?timeout=" + esperaSegundos
                + "&offset=" + ultimoNumero
                + "&allowed_updates=%5B%22message%22%2C%22callback_query%22%5D";

            string cuerpo = Solicitar(url, null);
            List<ActualizacionTelegram> lista = new List<ActualizacionTelegram>();

            using (JsonDocument documento = JsonDocument.Parse(cuerpo))
            {
                JsonElement raiz = documento.RootElement;
                VerificarRespuesta(raiz);

                foreach (JsonElement elemento in raiz.GetProperty("result").EnumerateArray())
                {
                    ActualizacionTelegram actualizacion = new ActualizacionTelegram();
                    actualizacion.Numero = elemento.GetProperty("update_id").GetInt32();

                    JsonElement mensaje;

                    if (elemento.TryGetProperty("message", out mensaje))
                    {
                        actualizacion.ChatId = mensaje.GetProperty("chat").GetProperty("id").GetInt64();

                        JsonElement identificador;
                        if (mensaje.TryGetProperty("message_id", out identificador))
                        {
                            actualizacion.MensajeId = identificador.GetInt32();
                        }

                        JsonElement texto;
                        if (mensaje.TryGetProperty("text", out texto))
                        {
                            actualizacion.Texto = texto.GetString();
                        }
                    }
                    else
                    {
                        JsonElement callback;

                        if (elemento.TryGetProperty("callback_query", out callback))
                        {
                            actualizacion.EsCallback = true;
                            actualizacion.CallbackId = callback.GetProperty("id").GetString();

                            JsonElement datos;
                            if (callback.TryGetProperty("data", out datos))
                            {
                                actualizacion.DatosCallback = datos.GetString();
                            }

                            JsonElement mensajeCallback = callback.GetProperty("message");
                            actualizacion.MensajeId = mensajeCallback.GetProperty("message_id").GetInt32();
                            actualizacion.ChatId = mensajeCallback.GetProperty("chat").GetProperty("id").GetInt64();
                        }
                    }

                    if (actualizacion.ChatId != 0)
                    {
                        lista.Add(actualizacion);
                    }
                }
            }

            return lista;
        }

        public void EnviarMensaje(long chatId, string texto)
        {
            EnviarMensaje(chatId, texto, null);
        }

        public void EnviarMensaje(long chatId, string texto, TecladoTelegram teclado)
        {
            string json = "{\"chat_id\":" + chatId + ",\"text\":\"" + Escapar(texto) + "\"";

            if (teclado != null)
            {
                json = json + ",\"reply_markup\":" + teclado.ToJson();
            }

            Enviar("sendMessage", json + "}");
        }

        public void EditarMensaje(long chatId, int mensajeId, string texto, TecladoTelegram teclado)
        {
            string json = "{\"chat_id\":" + chatId
                + ",\"message_id\":" + mensajeId
                + ",\"text\":\"" + Escapar(texto) + "\"";

            if (teclado != null && teclado.EsInline)
            {
                json = json + ",\"reply_markup\":" + teclado.ToJson();
            }

            Enviar("editMessageText", json + "}");
        }

        public void ResponderCallback(string callbackId, string texto)
        {
            string json = "{\"callback_query_id\":\"" + Escapar(callbackId)
                + "\",\"text\":\"" + Escapar(texto) + "\"}";

            Enviar("answerCallbackQuery", json);
        }

        /// <summary>
        /// Deja solo /start en el menu de comandos de Telegram: todo lo demas se hace con botones.
        /// </summary>
        public void ConfigurarComandos()
        {
            string json = "{\"commands\":[{\"command\":\"start\",\"description\":\"Abrir el menú de FinOptima\"}]}";
            Enviar("setMyCommands", json);
        }

        private void Enviar(string metodo, string json)
        {
            string cuerpo = Solicitar(Direccion(metodo), json);

            using (JsonDocument documento = JsonDocument.Parse(cuerpo))
            {
                VerificarRespuesta(documento.RootElement);
            }
        }

        private string Solicitar(string url, string contenidoJson)
        {
            HttpRequestMessage solicitud;

            if (contenidoJson == null)
            {
                solicitud = new HttpRequestMessage(HttpMethod.Get, url);
            }
            else
            {
                solicitud = new HttpRequestMessage(HttpMethod.Post, url);
                solicitud.Content = new StringContent(contenidoJson, Encoding.UTF8, "application/json");
            }

            using (solicitud)
            {
                using (HttpResponseMessage respuesta = cliente.Send(solicitud))
                {
                    using (StreamReader lector = new StreamReader(respuesta.Content.ReadAsStream()))
                    {
                        return lector.ReadToEnd();
                    }
                }
            }
        }

        private string Direccion(string metodo)
        {
            return "https://api.telegram.org/bot" + token + "/" + metodo;
        }

        private static void VerificarRespuesta(JsonElement raiz)
        {
            JsonElement correcto;

            if (raiz.TryGetProperty("ok", out correcto) && correcto.GetBoolean())
            {
                return;
            }

            string descripcion = "Telegram rechazo la solicitud.";
            JsonElement detalle;

            if (raiz.TryGetProperty("description", out detalle))
            {
                descripcion = detalle.GetString();
            }

            throw new InvalidOperationException(descripcion);
        }

        private static string Escapar(string texto)
        {
            return JsonTelegram.Escapar(texto);
        }
    }
}
