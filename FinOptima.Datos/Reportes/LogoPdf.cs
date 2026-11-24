using PdfSharp.Drawing;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Logo de FinOptima para los reportes PDF: un sobre del que sale una flecha de crecimiento.
    /// Usa la misma cuadricula de 100 x 100 unidades que el logo de la aplicacion.
    /// </summary>
    internal static class LogoPdf
    {
        public static void Dibujar(XGraphics dibujo, double x, double y, double lado, XColor fondo, XColor frente)
        {
            double e = lado / 100.0;

            dibujo.DrawRoundedRectangle(new XSolidBrush(fondo), x, y, lado, lado, 60 * e, 60 * e);

            dibujo.DrawRoundedRectangle(new XSolidBrush(frente), x + 17 * e, y + 47 * e, 66 * e, 37 * e, 16 * e, 16 * e);

            XPen pliegue = PlumaRedonda(fondo, 5.5 * e);
            dibujo.DrawLines(pliegue, new XPoint[]
            {
                new XPoint(x + 19 * e, y + 51 * e),
                new XPoint(x + 50 * e, y + 70 * e),
                new XPoint(x + 81 * e, y + 51 * e)
            });

            XPen linea = PlumaRedonda(frente, 7 * e);
            dibujo.DrawLines(linea, new XPoint[]
            {
                new XPoint(x + 27 * e, y + 42 * e),
                new XPoint(x + 42 * e, y + 28 * e),
                new XPoint(x + 53 * e, y + 36 * e),
                new XPoint(x + 68 * e, y + 21 * e)
            });

            XPen bordePunta = PlumaRedonda(frente, 2 * e);
            dibujo.DrawPolygon(bordePunta, new XSolidBrush(frente), new XPoint[]
            {
                new XPoint(x + 58 * e, y + 15 * e),
                new XPoint(x + 79 * e, y + 12 * e),
                new XPoint(x + 76 * e, y + 33 * e)
            }, XFillMode.Winding);
        }

        private static XPen PlumaRedonda(XColor color, double grosor)
        {
            XPen pluma = new XPen(color, grosor);
            pluma.LineCap = XLineCap.Round;
            pluma.LineJoin = XLineJoin.Round;
            return pluma;
        }
    }
}
