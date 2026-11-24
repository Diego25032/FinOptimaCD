namespace FinOptima.Bot
{
    public class AsignacionSobre
    {
        public int SobreId { get; set; }
        public string SobreNombre { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }

        public AsignacionSobre()
        {
            SobreId = 0;
            SobreNombre = "Sin sobre";
            Monto = 0;
            Porcentaje = 0;
        }
    }
}
