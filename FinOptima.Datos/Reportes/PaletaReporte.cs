using PdfSharp.Drawing;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Colores de marca de FinOptima usados en el PDF.
    /// </summary>
    internal static class PaletaReporte
    {
        public static readonly XColor Marca = XColor.FromArgb(12, 38, 31);
        public static readonly XColor MarcaClara = XColor.FromArgb(22, 66, 51);
        public static readonly XColor Acento = XColor.FromArgb(23, 201, 100);
        public static readonly XColor AcentoSuave = XColor.FromArgb(224, 248, 234);
        public static readonly XColor Peligro = XColor.FromArgb(229, 72, 77);
        public static readonly XColor PeligroSuave = XColor.FromArgb(253, 232, 233);
        public static readonly XColor Aviso = XColor.FromArgb(245, 165, 36);
        public static readonly XColor AvisoSuave = XColor.FromArgb(254, 243, 222);
        public static readonly XColor Info = XColor.FromArgb(0, 132, 230);
        public static readonly XColor InfoSuave = XColor.FromArgb(224, 240, 253);
        public static readonly XColor Texto = XColor.FromArgb(24, 33, 44);
        public static readonly XColor TextoSecundario = XColor.FromArgb(104, 117, 133);
        public static readonly XColor TextoSobreMarca = XColor.FromArgb(255, 255, 255);
        public static readonly XColor TextoSuaveSobreMarca = XColor.FromArgb(160, 196, 181);
        public static readonly XColor Borde = XColor.FromArgb(226, 231, 237);
        public static readonly XColor FondoSuave = XColor.FromArgb(245, 247, 250);
        public static readonly XColor Pista = XColor.FromArgb(236, 240, 244);
        public static readonly XColor Blanco = XColor.FromArgb(255, 255, 255);

        private static readonly XColor[] Serie = new XColor[]
        {
            XColor.FromArgb(23, 201, 100),
            XColor.FromArgb(0, 132, 230),
            XColor.FromArgb(245, 165, 36),
            XColor.FromArgb(139, 92, 246),
            XColor.FromArgb(229, 72, 77),
            XColor.FromArgb(0, 184, 169),
            XColor.FromArgb(236, 72, 153),
            XColor.FromArgb(120, 134, 150)
        };

        public static XColor ColorSerie(int indice)
        {
            return Serie[indice % Serie.Length];
        }
    }
}
