using System.Drawing;

namespace FinOptima.Vista.Controles
{
    public class SegmentoDona
    {
        public string Nombre { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
        public Color Color { get; set; }

        public SegmentoDona()
        {
            Nombre = "";
            Monto = 0;
            Porcentaje = 0;
            Color = System.Drawing.Color.Gray;
        }

        public SegmentoDona(string nombre, decimal monto, decimal porcentaje, Color color)
        {
            Nombre = nombre;
            Monto = monto;
            Porcentaje = porcentaje;
            Color = color;
        }
    }
}
