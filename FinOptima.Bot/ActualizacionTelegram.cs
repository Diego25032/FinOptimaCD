namespace FinOptima.Bot
{
    public class ActualizacionTelegram
    {
        public int Numero { get; set; }
        public long ChatId { get; set; }
        public int MensajeId { get; set; }
        public string Texto { get; set; }
        public bool EsCallback { get; set; }
        public string CallbackId { get; set; }
        public string DatosCallback { get; set; }

        public ActualizacionTelegram()
        {
            Numero = 0;
            ChatId = 0;
            MensajeId = 0;
            Texto = "";
            EsCallback = false;
            CallbackId = "";
            DatosCallback = "";
        }
    }
}
