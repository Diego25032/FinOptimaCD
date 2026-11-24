using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista.Controles
{
    public class MedidorSemicirculo : Control
    {
        private double valor;
        private double valorObjetivo;
        private double valorInicial;
        private int cuadroAnimacion;
        private readonly Timer temporizador;
        private const int CuadrosAnimacion = 40;
        private Color colorValor;
        private Color colorPista;
        private Bitmap cache;
        private int anchoCache;
        private int altoCache;

        public MedidorSemicirculo()
        {
            Tema tema = TemaActual.Actual;
            valor = 0;
            temporizador = new Timer();
            temporizador.Interval = 16;
            temporizador.Tick += new EventHandler(AlAvanzarAnimacion);
            colorValor = tema.Exito;
            colorPista = tema.SuperficieAlterna;
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Valor
        {
            get { return valorObjetivo; }
            set
            {
                double nuevo = value;

                if (nuevo < 0)
                {
                    nuevo = 0;
                }

                if (nuevo > 1)
                {
                    nuevo = 1;
                }

                valorObjetivo = nuevo;

                if (DesignMode)
                {
                    valor = nuevo;
                    LiberarCache();
                    Invalidate();
                    return;
                }

                // El arco se llena poco a poco hasta el nuevo valor.
                valorInicial = valor;
                cuadroAnimacion = 0;
                temporizador.Start();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorValor
        {
            get { return colorValor; }
            set
            {
                colorValor = value;
                LiberarCache();
                Invalidate();
            }
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

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            if (Width < 40 || Height < 30)
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

            int grosor = 14;
            int margen = 12;
            int diametro = Width - margen * 2;
            int maximoPorAlto = (Height - 16) * 2;

            if (diametro > maximoPorAlto)
            {
                diametro = maximoPorAlto;
            }

            if (diametro < 40)
            {
                return;
            }

            Rectangle area = new Rectangle((Width - diametro) / 2, Height - 8 - diametro / 2, diametro, diametro);

            using (Pen pista = new Pen(colorPista, grosor))
            {
                pista.StartCap = LineCap.Round;
                pista.EndCap = LineCap.Round;
                grafico.DrawArc(pista, area, 180, 180);
            }

            using (SolidBrush marca = new SolidBrush(TemaActual.Actual.TextoSecundario))
            {
                int centroX = area.Left + diametro / 2;
                int centroY = area.Top + diametro / 2;
                int radioMarca = diametro / 2 + grosor / 2 - 2;
                grafico.FillEllipse(marca, centroX - radioMarca - 3, centroY - 3, 6, 6);
                grafico.FillEllipse(marca, centroX + radioMarca - 3, centroY - 3, 6, 6);
            }

            if (valor > 0)
            {
                using (Pen progreso = new Pen(colorValor, grosor))
                {
                    progreso.StartCap = LineCap.Round;
                    progreso.EndCap = LineCap.Round;
                    grafico.DrawArc(progreso, area, 180, (float)(180 * valor));
                }
            }

            double angulo = (180 + 180 * valor) * Math.PI / 180;
            int centroAgujaX = area.Left + diametro / 2;
            int centroAgujaY = area.Top + diametro / 2;
            int radioInterno = 12;
            int radioExterno = diametro / 2 - grosor - 6;

            if (radioExterno < radioInterno + 8)
            {
                radioExterno = radioInterno + 8;
            }

            PointF desde = new PointF(
                (float)(centroAgujaX + Math.Cos(angulo) * radioInterno),
                (float)(centroAgujaY + Math.Sin(angulo) * radioInterno));

            PointF hasta = new PointF(
                (float)(centroAgujaX + Math.Cos(angulo) * radioExterno),
                (float)(centroAgujaY + Math.Sin(angulo) * radioExterno));

            using (Pen aguja = new Pen(colorValor, 2.5f))
            {
                aguja.StartCap = LineCap.Round;
                aguja.EndCap = LineCap.Round;
                grafico.DrawLine(aguja, desde, hasta);
            }

            using (SolidBrush punto = new SolidBrush(colorValor))
            {
                grafico.FillEllipse(punto, centroAgujaX - 5, centroAgujaY - 5, 10, 10);
            }
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

        private void AlAvanzarAnimacion(object remitente, EventArgs argumentos)
        {
            cuadroAnimacion++;
            double t = (double)cuadroAnimacion / CuadrosAnimacion;

            if (t >= 1)
            {
                t = 1;
                temporizador.Stop();
            }

            double inverso = 1 - t;
            double suave = 1 - inverso * inverso * inverso;
            valor = valorInicial + (valorObjetivo - valorInicial) * suave;
            LiberarCache();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                temporizador.Stop();
                temporizador.Dispose();
            }

            if (disposing)
            {
                LiberarCache();
            }

            base.Dispose(disposing);
        }
    }
}
