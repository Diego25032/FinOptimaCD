using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Controles
{
    public class GraficoDona : Control
    {
        private List<SegmentoDona> segmentos;
        private Color colorPista;
        private Color colorTexto;
        private string textoVacio;
        private Bitmap cache;
        private int anchoCache;
        private int altoCache;

        public GraficoDona()
        {
            Tema tema = TemaActual.Actual;
            segmentos = new List<SegmentoDona>();
            colorPista = tema.SuperficieAlterna;
            colorTexto = tema.TextoSecundario;
            textoVacio = "Sin gastos registrados este mes";
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorPista
        {
            get { return colorPista; }
            set
            {
                colorPista = value;
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

        public void EstablecerDatos(List<SegmentoDona> nuevos)
        {
            segmentos = new List<SegmentoDona>();

            if (nuevos != null)
            {
                foreach (SegmentoDona segmento in nuevos)
                {
                    segmentos.Add(segmento);
                }
            }

            LiberarCache();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            if (Width < 80 || Height < 80)
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
            grafico.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            int grosor = 24;
            int anchoAnillo = (Width * 42) / 100;
            int diametro = Height - 32;

            if (diametro > anchoAnillo - 16)
            {
                diametro = anchoAnillo - 16;
            }

            if (diametro < 60)
            {
                diametro = 60;
            }

            Rectangle area = new Rectangle(14, (Height - diametro) / 2, diametro, diametro);

            using (Pen pista = new Pen(colorPista, grosor))
            {
                grafico.DrawEllipse(pista, Ajustar(area, grosor));
            }

            decimal total = 0;

            foreach (SegmentoDona segmento in segmentos)
            {
                total = total + segmento.Monto;
            }

            if (segmentos.Count == 0 || total <= 0)
            {
                DibujarTextoVacio(grafico, area);
                return;
            }

            float inicio = -90;

            foreach (SegmentoDona segmento in segmentos)
            {
                float barrido = (float)(360.0 * (double)(segmento.Monto / total));

                if (barrido <= 0)
                {
                    continue;
                }

                float visible = barrido - 1.5f;

                if (visible < 0.8f)
                {
                    visible = 0.8f;
                }

                using (Pen lapiz = new Pen(segmento.Color, grosor))
                {
                    grafico.DrawArc(lapiz, Ajustar(area, grosor), inicio, visible);
                }

                inicio = inicio + barrido;
            }

            DibujarCentro(grafico, area, total);
            DibujarLeyenda(grafico, area);
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

        private void DibujarTextoVacio(Graphics grafico, Rectangle area)
        {
            int anchoTexto = (int)(area.Width * 0.72);

            if (anchoTexto < 90)
            {
                anchoTexto = 90;
            }

            using (Font fuente = new Font("Segoe UI", 8.5f))
            {
                Rectangle zona = new Rectangle(
                    area.Left + (area.Width - anchoTexto) / 2,
                    area.Top + area.Height / 2 - 30,
                    anchoTexto,
                    60);

                using (SolidBrush pincel = new SolidBrush(colorTexto))
                {
                    using (StringFormat formato = new StringFormat())
                    {
                        formato.Alignment = StringAlignment.Center;
                        formato.LineAlignment = StringAlignment.Center;
                        grafico.DrawString(textoVacio, fuente, pincel, zona, formato);
                    }
                }
            }
        }

        private void DibujarCentro(Graphics grafico, Rectangle area, decimal total)
        {
            using (Font titulo = new Font("Segoe UI", 8))
            {
                using (Font valor = new Font("Segoe UI", 10.5f))
                {
                    int anchoTexto = (int)(area.Width * 0.66);
                    Rectangle zonaTitulo = new Rectangle(
                        area.Left + (area.Width - anchoTexto) / 2,
                        area.Top + area.Height / 2 - 20,
                        anchoTexto,
                        18);

                    Rectangle zonaValor = new Rectangle(
                        area.Left + (area.Width - anchoTexto) / 2,
                        area.Top + area.Height / 2 - 2,
                        anchoTexto,
                        22);

                    DibujarTexto(grafico, "Gastos", titulo, zonaTitulo, colorTexto, StringAlignment.Center);
                    DibujarTexto(grafico, UtilidadesVista.MonedaCorta(total), valor, zonaValor, TemaActual.Actual.Texto, StringAlignment.Center);
                }
            }
        }

        private void DibujarLeyenda(Graphics grafico, Rectangle area)
        {
            int xLeyenda = area.Right + 22;
            int anchoLeyenda = Width - xLeyenda - 12;

            if (anchoLeyenda < 120)
            {
                return;
            }

            int visibles = segmentos.Count;

            if (visibles > 5)
            {
                visibles = 5;
            }

            int altoFila = 32;
            int altoTotal = visibles * altoFila;
            int y = (Height - altoTotal) / 2;

            using (Font fuente = new Font("Segoe UI", 8.5f))
            {
                using (Font porcentaje = new Font("Segoe UI", 8.5f))
                {
                    for (int indice = 0; indice < visibles; indice = indice + 1)
                    {
                        SegmentoDona segmento = segmentos[indice];
                        int yFila = y + indice * altoFila;

                        using (SolidBrush punto = new SolidBrush(segmento.Color))
                        {
                            grafico.FillEllipse(punto, xLeyenda, yFila + 10, 9, 9);
                        }

                        string nombre = segmento.Nombre;

                        if (nombre.Length > 18)
                        {
                            nombre = nombre.Substring(0, 17) + "...";
                        }

                        Rectangle zonaNombre = new Rectangle(xLeyenda + 16, yFila + 2, anchoLeyenda - 66, 24);
                        DibujarTexto(grafico, nombre, fuente, zonaNombre, colorTexto, StringAlignment.Near);

                        string textoPorcentaje = UtilidadesVista.Porcentaje(Math.Round(segmento.Porcentaje, 0));
                        Size medida = TextRenderer.MeasureText(textoPorcentaje, porcentaje);
                        Rectangle zonaPorcentaje = new Rectangle(
                            xLeyenda + anchoLeyenda - medida.Width,
                            yFila + 2,
                            medida.Width,
                            24);

                        DibujarTexto(grafico, textoPorcentaje, porcentaje, zonaPorcentaje, TemaActual.Actual.Texto, StringAlignment.Far);
                    }

                    if (segmentos.Count > visibles)
                    {
                        int yExtra = y + visibles * altoFila + 2;
                        string textoExtra = "+" + (segmentos.Count - visibles) + " categorias mas";
                        DibujarTexto(
                            grafico,
                            textoExtra,
                            fuente,
                            new Rectangle(xLeyenda + 16, yExtra, anchoLeyenda - 32, 24),
                            colorTexto,
                            StringAlignment.Near);
                    }
                }
            }
        }

        private static void DibujarTexto(Graphics grafico, string texto, Font fuente, Rectangle zona, Color color, StringAlignment alineacion)
        {
            using (SolidBrush pincel = new SolidBrush(color))
            {
                using (StringFormat formato = new StringFormat())
                {
                    formato.Alignment = alineacion;
                    formato.LineAlignment = StringAlignment.Center;
                    formato.Trimming = StringTrimming.EllipsisCharacter;
                    formato.FormatFlags = StringFormatFlags.NoWrap;
                    grafico.DrawString(texto, fuente, pincel, zona, formato);
                }
            }
        }

        private static Rectangle Ajustar(Rectangle area, int grosor)
        {
            int mitad = grosor / 2;
            return new Rectangle(area.Left + mitad, area.Top + mitad, area.Width - grosor, area.Height - grosor);
        }
    }
}
