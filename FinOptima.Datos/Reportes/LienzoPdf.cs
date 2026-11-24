using System;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Maneja las paginas del PDF: cursor vertical, saltos de pagina, cabecera de las
    /// paginas siguientes, pie con numeracion y utilidades de dibujo.
    /// </summary>
    internal class LienzoPdf
    {
        public const double MargenX = 40;
        public const double MargenInferior = 58;

        private PdfDocument documento;
        private PdfPage pagina;
        private XGraphics dibujo;
        private string nombreUsuario;
        private string fechaTexto;

        public double Y { get; set; }

        public XGraphics Dibujo
        {
            get { return dibujo; }
        }

        public double Ancho
        {
            get { return pagina.Width.Point; }
        }

        public double Alto
        {
            get { return pagina.Height.Point; }
        }

        public double AnchoUtil
        {
            get { return Ancho - MargenX * 2; }
        }

        public LienzoPdf(PdfDocument documentoNuevo, string usuario, string fecha)
        {
            documento = documentoNuevo;
            nombreUsuario = usuario;
            fechaTexto = fecha;
            CrearPagina(false);
        }

        /// <summary>
        /// Garantiza que quepan "alto" puntos. Si no caben, abre otra pagina y devuelve true.
        /// </summary>
        public bool AsegurarEspacio(double alto)
        {
            if (Y + alto <= Alto - MargenInferior)
            {
                return false;
            }

            CrearPagina(true);
            return true;
        }

        public void Guardar(string ruta)
        {
            dibujo.Dispose();
            dibujo = null;
            DibujarPies();
            documento.Save(ruta);
            documento.Dispose();
        }

        private void CrearPagina(bool continuacion)
        {
            if (dibujo != null)
            {
                dibujo.Dispose();
            }

            pagina = documento.AddPage();
            pagina.Size = PageSize.A4;
            dibujo = XGraphics.FromPdfPage(pagina);
            Y = 40;

            if (continuacion)
            {
                DibujarCabeceraContinua();
            }
        }

        private void DibujarCabeceraContinua()
        {
            XFont marca = Fuentes.Negrita(10);
            XFont texto = Fuentes.Normal(8);

            Rectangulo(0, 0, Ancho, 34, PaletaReporte.Marca);
            LogoPdf.Dibujar(dibujo, MargenX, 9, 16, PaletaReporte.Acento, PaletaReporte.Marca);
            Texto("FinOptima", marca, PaletaReporte.TextoSobreMarca, MargenX + 22, 11);
            TextoDerecha(nombreUsuario + "  ·  " + fechaTexto, texto, PaletaReporte.TextoSuaveSobreMarca, MargenX, 12, AnchoUtil);
            Y = 56;
        }

        private void DibujarPies()
        {
            int total = documento.PageCount;
            XFont fuente = Fuentes.Normal(7.5);

            for (int indice = 0; indice < total; indice = indice + 1)
            {
                PdfPage actual = documento.Pages[indice];

                using (XGraphics grafico = XGraphics.FromPdfPage(actual, XGraphicsPdfPageOptions.Append))
                {
                    double ancho = actual.Width.Point;
                    double alto = actual.Height.Point;
                    double y = alto - 34;

                    grafico.DrawLine(new XPen(PaletaReporte.Borde, 0.6), MargenX, y, ancho - MargenX, y);

                    XSolidBrush pincel = new XSolidBrush(PaletaReporte.TextoSecundario);
                    grafico.DrawString("FinOptima  ·  Reporte financiero personal  ·  Documento confidencial", fuente, pincel,
                        new XRect(MargenX, y + 6, ancho - MargenX * 2, 12), XStringFormats.TopLeft);
                    grafico.DrawString("Página " + (indice + 1) + " de " + total, fuente, pincel,
                        new XRect(MargenX, y + 6, ancho - MargenX * 2, 12), XStringFormats.TopRight);
                }
            }
        }

        // ---------- Utilidades de dibujo ----------

        public void Rectangulo(double x, double y, double ancho, double alto, XColor color)
        {
            dibujo.DrawRectangle(new XSolidBrush(color), x, y, ancho, alto);
        }

        public void RectanguloRedondeado(double x, double y, double ancho, double alto, double radio, XColor color)
        {
            if (ancho <= 0 || alto <= 0)
            {
                return;
            }

            double r = radio;

            if (r * 2 > ancho)
            {
                r = ancho / 2;
            }

            if (r * 2 > alto)
            {
                r = alto / 2;
            }

            dibujo.DrawRoundedRectangle(new XSolidBrush(color), x, y, ancho, alto, r * 2, r * 2);
        }

        public void Tarjeta(double x, double y, double ancho, double alto)
        {
            dibujo.DrawRoundedRectangle(new XPen(PaletaReporte.Borde, 0.8), new XSolidBrush(PaletaReporte.Blanco), x, y, ancho, alto, 12, 12);
        }

        public void Linea(double x1, double y1, double x2, double y2, XColor color, double grosor)
        {
            dibujo.DrawLine(new XPen(color, grosor), x1, y1, x2, y2);
        }

        public void Texto(string texto, XFont fuente, XColor color, double x, double y)
        {
            dibujo.DrawString(texto, fuente, new XSolidBrush(color), new XRect(x, y, 1000, fuente.GetHeight()), XStringFormats.TopLeft);
        }

        public void TextoAjustado(string texto, XFont fuente, XColor color, double x, double y, double ancho)
        {
            Texto(Recortar(texto, fuente, ancho), fuente, color, x, y);
        }

        /// <summary>
        /// Escribe el texto reduciendo el tamano de letra hasta que quepa en el ancho.
        /// </summary>
        public void TextoQueQuepa(string texto, bool negrita, double tamanoMaximo, double tamanoMinimo, XColor color, double x, double y, double ancho)
        {
            double tamano = tamanoMaximo;
            XFont fuente = negrita ? Fuentes.Negrita(tamano) : Fuentes.Normal(tamano);

            while (tamano > tamanoMinimo && Medir(texto, fuente) > ancho)
            {
                tamano = tamano - 0.5;
                fuente = negrita ? Fuentes.Negrita(tamano) : Fuentes.Normal(tamano);
            }

            double desplazamiento = (tamanoMaximo - tamano) * 0.6;
            TextoAjustado(texto, fuente, color, x, y + desplazamiento, ancho);
        }

        public void TextoDerecha(string texto, XFont fuente, XColor color, double x, double y, double ancho)
        {
            dibujo.DrawString(Recortar(texto, fuente, ancho), fuente, new XSolidBrush(color), new XRect(x, y, ancho, fuente.GetHeight()), XStringFormats.TopRight);
        }

        public void TextoCentrado(string texto, XFont fuente, XColor color, double x, double y, double ancho, double alto)
        {
            dibujo.DrawString(Recortar(texto, fuente, ancho), fuente, new XSolidBrush(color), new XRect(x, y, ancho, alto), XStringFormats.Center);
        }

        public double Medir(string texto, XFont fuente)
        {
            return dibujo.MeasureString(texto, fuente).Width;
        }

        public string Recortar(string texto, XFont fuente, double ancho)
        {
            if (texto == null)
            {
                return "";
            }

            if (ancho <= 0 || Medir(texto, fuente) <= ancho)
            {
                return texto;
            }

            string recorte = texto;

            while (recorte.Length > 1 && Medir(recorte + "…", fuente) > ancho)
            {
                recorte = recorte.Substring(0, recorte.Length - 1);
            }

            return recorte.TrimEnd() + "…";
        }

        public void Etiqueta(string texto, double x, double y, XColor fondo, XColor color)
        {
            XFont fuente = Fuentes.Negrita(7);
            double ancho = Medir(texto, fuente) + 12;
            RectanguloRedondeado(x, y, ancho, 13, 6.5, fondo);
            TextoCentrado(texto, fuente, color, x, y, ancho, 13);
        }

        public void Barra(double x, double y, double ancho, double alto, decimal fraccion, XColor color)
        {
            RectanguloRedondeado(x, y, ancho, alto, alto / 2, PaletaReporte.Pista);

            double valor = (double)fraccion;

            if (valor <= 0)
            {
                return;
            }

            if (valor > 1)
            {
                valor = 1;
            }

            double relleno = ancho * valor;

            if (relleno < alto)
            {
                relleno = alto;
            }

            RectanguloRedondeado(x, y, relleno, alto, alto / 2, color);
        }

        public void Sector(double x, double y, double diametro, double inicio, double barrido, XColor color)
        {
            dibujo.DrawPie(new XSolidBrush(color), x, y, diametro, diametro, inicio, barrido);
        }

        public void Circulo(double x, double y, double diametro, XColor color)
        {
            dibujo.DrawEllipse(new XSolidBrush(color), x, y, diametro, diametro);
        }
    }

    internal static class Fuentes
    {
        public static XFont Normal(double tamano)
        {
            return new XFont(ResolvedorFuentes.Familia, tamano, XFontStyleEx.Regular);
        }

        public static XFont Negrita(double tamano)
        {
            return new XFont(ResolvedorFuentes.Familia, tamano, XFontStyleEx.Bold);
        }
    }
}
