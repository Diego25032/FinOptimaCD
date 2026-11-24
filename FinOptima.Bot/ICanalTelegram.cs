namespace FinOptima.Bot
{
    /// <summary>
    /// Lo que el manejador necesita de Telegram. Permite cambiar el cliente real por uno
    /// falso en las pruebas.
    /// </summary>
    public interface ICanalTelegram
    {
        void EnviarMensaje(long chatId, string texto, TecladoTelegram teclado);
        void EditarMensaje(long chatId, int mensajeId, string texto, TecladoTelegram teclado);
        void ResponderCallback(string callbackId, string texto);
    }
}
