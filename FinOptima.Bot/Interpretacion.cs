using FinOptima.Entidades.Enums;

namespace FinOptima.Bot
{
    public class Interpretacion
    {
        public TipoInterpretacion Tipo { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public TipoSobre SobreSugerido { get; set; }
        public bool EsSueldo { get; set; }

        public Interpretacion()
        {
            Tipo = TipoInterpretacion.Desconocido;
            Monto = 0;
            Descripcion = "";
            Categoria = "";
            SobreSugerido = TipoSobre.Necesidades;
            EsSueldo = false;
        }
    }
}
