using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista.Controles
{
    public class GraficoLinea : Control
    {
        private List<decimal> valores;
        private List<string> etiquetas;
        private Color colorLinea;
        private Color colorGrilla;
        private Color colorTexto;
        private string textoVacio;
        private Bitmap cache;
        private int anchoCache;
        private int altoCache;

        public GraficoLinea()
        {
            Tema tema = TemaActual.Actual;
            valores = new List<decimal>();
            etiquetas = new List<string>();
            colorLinea = tema.Exito;
            colorGrilla = tema.BordeSuave;
            colorTexto = tema.TextoSecundario;
            textoVacio = "Sin movimientos en el periodo";
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorLinea
        {
            get { return colorLinea; }
            set
            {
                colorLinea = value;
                LiberarCache();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorGrilla
        {
            get { return colorGrilla; }
            set
            {
                colorGrilla = value;
                LiberarCache();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorTexto
        {
            get { return colorTexto; }
            set
            {
                colorTexto = value;
                LiberarCache();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TextoVacio
        {
            get { return textoVacio; }
            set
            {
                textoVacio = value;
                LiberarCache();
                Invalidate();
            }
        }

        public void EstablecerDatos(List<decimal> nuevosValores, List<string> nuevasEtiquetas)
        {
            valores = new List<decimal>();
            etiquetas = new List<string>();

            if (nuevosValores != null)
            {
                foreach (decimal valor in nuevosValores)
                {
                    valores.Add(valor);
                }
            }

            if (nuevasEtiquetas != null)
            {
                foreach (string etiqueta in nuevasEtiquetas)
                {
                    etiquetas.Add(etiqueta);
                }
            }

            LiberarCache();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            if (Width < 60 || Height < 60)
            {
                return;
            }

            if (cache == null || anchoCache != Width || altoCache != Height)
            {
                CrearCache();
            }

            if (cache != null)
            {
                CompositingMode anterior = argumentos.Graphics.CompositingMode;
                argumentos.Graphics.CompositingMode = CompositingMode.SourceCopy;
                argumentos.Graphics.DrawImage(cache, new Rectangle(0, 0, Width, Height), 0, 0, Width, Height, GraphicsUnit.Pixel);
                argumentos.Graphics.CompositingMode = anterior;
            }
        }

        private void Dibujar(Graphics grafico)
        {
            grafico.SmoothingMode = SmoothingMode.AntiAlias;

            int izquierda = 12;
            int derecha = Width - 12;
            int arriba = 14;
            int abajo = Height - 26;

            if (abajo - arriba < 30)
            {
                return;
            }

            using (Pen lapizGrilla = new Pen(colorGrilla, 1))
            {
                lapizGrilla.DashStyle = DashStyle.Dash;
                lapizGrilla.DashPattern = new float[] { 4f, 5f };

                for (int linea = 0; linea <= 4; linea = linea + 1)
                {
                    int y = arriba + (abajo - arriba) * linea / 4;
                    grafico.DrawLine(lapizGrilla, izquierda, y, derecha, y);
                }
            }

            if (valores.Count == 0)
            {
                using (Font fuente = new Font("Segoe UI", 9))
                {
                    Size medida = TextRenderer.MeasureText(textoVacio, fuente);
                    TextRenderer.DrawText(
                        grafico,
                        textoVacio,
                        fuente,
                        new Rectangle(izquierda, (arriba + abajo - medida.Height) / 2, derecha - izquierda, medida.Height),
                        colorTexto,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                return;
            }

            decimal maximo = valores[0];
            decimal minimo = valores[0];

            foreach (decimal valor in valores)
            {
                if (valor > maximo)
                {
                    maximo = valor;
                }

                if (valor < minimo)
                {
                    minimo = valor;
                }
            }

            if (minimo > 0)
            {
                minimo = 0;
            }

            if (maximo < 0)
            {
                maximo = 0;
            }

            if (maximo == minimo)
            {
                maximo = minimo + 1;
            }

            PointF[] puntos = new PointF[valores.Count];

            for (int indice = 0; indice < valores.Count; indice = indice + 1)
            {
                float x;

                if (valores.Count == 1)
                {
                    x = (izquierda + derecha) / 2f;
                }
                else
                {
                    x = izquierda + (float)(derecha - izquierda) * indice / (valores.Count - 1);
                }

                float proporcion = (float)((valores[indice] - minimo) / (maximo - minimo));
                float y = abajo - proporcion * (abajo - arriba);
                puntos[indice] = new PointF(x, y);
            }

            using (GraphicsPath relleno = new GraphicsPath())
            {
                if (puntos.Length > 2)
                {
                    relleno.AddCurve(puntos, 0.45f);
                }
                else if (puntos.Length == 2)
                {
                    relleno.AddLine(puntos[0], puntos[1]);
                }
                else
                {
                    relleno.AddLine(puntos[0].X - 1, puntos[0].Y, puntos[0].X + 1, puntos[0].Y);
                }

                relleno.AddLine(puntos[puntos.Length - 1].X, abajo, puntos[0].X, abajo);
                relleno.CloseFigure();

                Color rellenoArriba = Color.FromArgb(90, colorLinea);
                Color rellenoAbajo = Color.FromArgb(0, colorLinea);

                using (LinearGradientBrush pincel = new LinearGradientBrush(
                    new Rectangle(izquierda, arriba, derecha - izquierda, abajo - arriba),
                    rellenoArriba,
                    rellenoAbajo,
                    LinearGradientMode.Vertical))
                {
                    grafico.FillPath(pincel, relleno);
                }
            }

            using (Pen lapiz = new Pen(colorLinea, 2.5f))
            {
                lapiz.StartCap = LineCap.Round;
                lapiz.EndCap = LineCap.Round;

                if (puntos.Length > 2)
                {
                    grafico.DrawCurve(lapiz, puntos, 0.45f);
                }
                else if (puntos.Length == 2)
                {
                    grafico.DrawLine(lapiz, puntos[0], puntos[1]);
                }
                else
                {
                    grafico.DrawLine(lapiz, puntos[0].X - 1, puntos[0].Y, puntos[0].X + 1, puntos[0].Y);
                }
            }

            using (SolidBrush punto = new SolidBrush(colorLinea))
            {
                PointF ultimo = puntos[puntos.Length - 1];
                grafico.FillEllipse(punto, ultimo.X - 4, ultimo.Y - 4, 8, 8);
            }

            DibujarEtiquetas(grafico, izquierda, derecha, abajo);
        }

        private void CrearCache()
        {
            LiberarCache();

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            Bitmap nuevo = new Bitmap(Width, Height);

            using (Graphics grafico = Graphics.FromImage(nuevo))
            {
                grafico.Clear(ColorFondoPadre());
                Dibujar(grafico);
            }

            cache = nuevo;
            anchoCache = Width;
            altoCache = Height;
        }

        private Color ColorFondoPadre()
        {
            if (Parent != null && Parent.BackColor != Color.Transparent)
            {
                return Parent.BackColor;
            }

            return TemaActual.Actual.Superficie;
        }

        private void LiberarCache()
        {
            if (cache != null)
            {
                cache.Dispose();
                cache = null;
            }

            anchoCache = 0;
            altoCache = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LiberarCache();
            }

            base.Dispose(disposing);
        }

        private void DibujarEtiquetas(Graphics grafico, int izquierda, int derecha, int abajo)
        {
            if (etiquetas.Count == 0)
            {
                return;
            }

            using (Font fuente = new Font("Segoe UI", 8))
            {
                int posiciones = etiquetas.Count;

                if (posiciones > 6)
                {
                    posiciones = 6;
                }

                for (int indice = 0; indice < posiciones; indice = indice + 1)
                {
                    int origen = indice;

                    if (etiquetas.Count > posiciones)
                    {
                        origen = indice * (etiquetas.Count - 1) / (posiciones - 1);
                    }

                    float x;

                    if (etiquetas.Count == 1)
                    {
                        x = (izquierda + derecha) / 2f;
                    }
                    else
                    {
                        x = izquierda + (float)(derecha - izquierda) * origen / (etiquetas.Count - 1);
                    }

                    string texto = etiquetas[origen];

                    if (texto.Length > 3)
                    {
                        texto = texto.Substring(0, 3);
                    }

                    Size medida = TextRenderer.MeasureText(texto, fuente);
                    int posicionX = (int)x - medida.Width / 2;

                    if (posicionX < 0)
                    {
                        posicionX = 0;
                    }

                    if (posicionX + medida.Width > Width)
                    {
                        posicionX = Width - medida.Width;
                    }

                    TextRenderer.DrawText(
                        grafico,
                        texto,
                        fuente,
                        new Point(posicionX, abajo + 6),
                        colorTexto);
                }
            }
        }
    }
}
