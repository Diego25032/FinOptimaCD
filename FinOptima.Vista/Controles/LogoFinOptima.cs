using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Controles
{
    /// <summary>
    /// Logo de FinOptima: un sobre (metodo de los 7 sobres) del que sale una flecha de crecimiento.
    /// Se dibuja con vectores, asi se ve nitido con cualquier escala de Windows y toma el color de acento del usuario.
    /// </summary>
    public class LogoFinOptima : Control
    {
        private Color colorAcento;
        private readonly Timer temporizador;
        private int cuadro;
        private int esperaBrillo;
        private float progresoFlecha;
        private float posicionBrillo;
        private float escalaPulso;

        private const int CuadrosEntrada = 48;
        private const int CuadrosBrillo = 42;
        private const int CuadrosEntreBrillos = 360;

        public LogoFinOptima()
        {
            progresoFlecha = 1f;
            posicionBrillo = -1f;
            escalaPulso = 1f;
            temporizador = new Timer();
            temporizador.Interval = 16;
            temporizador.Tick += new EventHandler(AlAvanzarAnimacion);
            colorAcento = Color.FromArgb(47, 111, 237);
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorAcento
        {
            get { return colorAcento; }
            set
            {
                colorAcento = value;
                Invalidate();
            }
        }

        /// <summary>Reproduce la animacion de entrada: la flecha se dibuja sola y el logo late.</summary>
        public void Animar()
        {
            if (DesignMode)
            {
                return;
            }

            cuadro = 0;
            esperaBrillo = 0;
            progresoFlecha = 0f;
            temporizador.Start();
        }

        protected override void OnHandleCreated(EventArgs argumentos)
        {
            base.OnHandleCreated(argumentos);
            Animar();
        }

        protected override void OnMouseEnter(EventArgs argumentos)
        {
            base.OnMouseEnter(argumentos);

            if (cuadro >= CuadrosEntrada)
            {
                Animar();
            }
        }

        protected override void OnVisibleChanged(EventArgs argumentos)
        {
            base.OnVisibleChanged(argumentos);

            if (!Visible)
            {
                temporizador.Stop();
            }
            else if (IsHandleCreated && !DesignMode)
            {
                temporizador.Start();
            }
        }

        protected override void Dispose(bool liberando)
        {
            if (liberando)
            {
                temporizador.Stop();
                temporizador.Dispose();
            }

            base.Dispose(liberando);
        }

        private void AlAvanzarAnimacion(object remitente, EventArgs argumentos)
        {
            cuadro++;

            if (cuadro <= CuadrosEntrada)
            {
                float t = (float)cuadro / CuadrosEntrada;
                progresoFlecha = SuavizarSalida(Math.Min(1f, t * 1.25f));
                // Pequeno latido al final de la entrada.
                escalaPulso = t > 0.7f ? 1f + 0.06f * (float)Math.Sin((t - 0.7f) / 0.3f * Math.PI) : 1f;
                posicionBrillo = -1f;
            }
            else
            {
                progresoFlecha = 1f;
                escalaPulso = 1f;
                esperaBrillo++;

                if (esperaBrillo <= CuadrosBrillo)
                {
                    posicionBrillo = -0.4f + 1.8f * esperaBrillo / CuadrosBrillo;
                }
                else
                {
                    posicionBrillo = -1f;

                    if (esperaBrillo >= CuadrosEntreBrillos)
                    {
                        esperaBrillo = 0;
                    }
                }
            }

            Invalidate();
        }

        private static float SuavizarSalida(float t)
        {
            float inverso = 1f - t;
            return 1f - inverso * inverso * inverso;
        }

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            base.OnPaint(argumentos);

            int lado = Math.Min(Width, Height);

            if (lado < 8)
            {
                return;
            }

            float tamano = lado / 1.07f * escalaPulso;
            float x = (Width - tamano) / 2f;
            float y = (Height - tamano) / 2f;
            Dibujar(argumentos.Graphics, new RectangleF(x, y, tamano, tamano), colorAcento, progresoFlecha, posicionBrillo);
        }

        /// <summary>
        /// Dibuja el logo dentro del area indicada. El diseno usa una cuadricula de 100 x 100 unidades.
        /// </summary>
        public static void Dibujar(Graphics grafico, RectangleF area, Color acento)
        {
            Dibujar(grafico, area, acento, 1f, -1f);
        }

        /// <summary>
        /// Dibuja el logo con animacion: progreso (0 a 1) de la flecha y posicion (0 a 1) del destello; -1 sin destello.
        /// </summary>
        public static void Dibujar(Graphics grafico, RectangleF area, Color acento, float progreso, float brillo)
        {
            GraphicsState estado = grafico.Save();

            try
            {
                grafico.SmoothingMode = SmoothingMode.AntiAlias;
                grafico.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grafico.TranslateTransform(area.X, area.Y);
                grafico.ScaleTransform(area.Width / 100f, area.Height / 100f);

                Color claro = UtilidadesVista.Mezclar(acento, Color.White, 22);
                Color frente = UtilidadesVista.ColorContraste(acento);

                using (GraphicsPath fondo = RutaRedonda(0, 0, 100, 100, 30))
                using (LinearGradientBrush degradado = new LinearGradientBrush(new PointF(0, 0), new PointF(100, 100), claro, acento))
                {
                    grafico.FillPath(degradado, fondo);

                    if (brillo > -0.5f)
                    {
                        DibujarDestello(grafico, fondo, brillo);
                    }
                }

                // Cuerpo del sobre.
                using (GraphicsPath sobre = RutaRedonda(17, 47, 66, 37, 8))
                using (SolidBrush pincelFrente = new SolidBrush(frente))
                {
                    grafico.FillPath(pincelFrente, sobre);
                }

                // Pliegue del sobre.
                using (Pen pliegue = PlumaRedonda(acento, 5.5f))
                {
                    grafico.DrawLines(pliegue, new PointF[] { new PointF(19, 51), new PointF(50, 70), new PointF(81, 51) });
                }

                // Flecha de crecimiento que sale del sobre.
                PointF[] puntos = new PointF[] { new PointF(27, 42), new PointF(42, 28), new PointF(53, 36), new PointF(68, 21) };
                PointF[] parcial = RecortarLinea(puntos, progreso);

                if (parcial.Length >= 2)
                {
                    using (Pen linea = PlumaRedonda(frente, 7f))
                    {
                        grafico.DrawLines(linea, parcial);
                    }
                }

                if (progreso > 0.75f)
                {
                    float aparicion = Math.Min(1f, (progreso - 0.75f) / 0.25f);
                    int alfa = (int)(255 * aparicion);
                    Color colorPunta = Color.FromArgb(alfa, frente);
                    float centroX = 71f;
                    float centroY = 20f;
                    float escala = 0.5f + 0.5f * aparicion;
                    PointF[] triangulo = new PointF[] { new PointF(58, 15), new PointF(79, 12), new PointF(76, 33) };

                    for (int i = 0; i < triangulo.Length; i++)
                    {
                        triangulo[i] = new PointF(centroX + (triangulo[i].X - centroX) * escala, centroY + (triangulo[i].Y - centroY) * escala);
                    }

                    using (GraphicsPath punta = new GraphicsPath())
                    using (SolidBrush pincelPunta = new SolidBrush(colorPunta))
                    using (Pen bordePunta = PlumaRedonda(colorPunta, 2f))
                    {
                        punta.AddPolygon(triangulo);
                        grafico.FillPath(pincelPunta, punta);
                        grafico.DrawPath(bordePunta, punta);
                    }
                }
            }
            finally
            {
                grafico.Restore(estado);
            }
        }

        /// <summary>
        /// Devuelve el logo como imagen, util para iconos o vistas previas.
        /// </summary>
        public static Bitmap CrearImagen(int lado, Color acento)
        {
            Bitmap imagen = new Bitmap(lado, lado);

            using (Graphics grafico = Graphics.FromImage(imagen))
            {
                grafico.Clear(Color.Transparent);
                Dibujar(grafico, new RectangleF(0, 0, lado, lado), acento);
            }

            return imagen;
        }

        private static void DibujarDestello(Graphics grafico, GraphicsPath fondo, float brillo)
        {
            Region recorte = grafico.Clip;
            grafico.SetClip(fondo, CombineMode.Intersect);
            float centro = -20f + 140f * brillo;

            using (GraphicsPath banda = new GraphicsPath())
            {
                banda.AddPolygon(new PointF[] { new PointF(centro - 14, 0), new PointF(centro + 6, 0), new PointF(centro - 34, 100), new PointF(centro - 54, 100) });

                using (SolidBrush pincel = new SolidBrush(Color.FromArgb(70, Color.White)))
                {
                    grafico.FillPath(pincel, banda);
                }
            }

            grafico.Clip = recorte;
        }

        private static PointF[] RecortarLinea(PointF[] puntos, float progreso)
        {
            if (progreso >= 1f)
            {
                return puntos;
            }

            float total = 0f;

            for (int i = 1; i < puntos.Length; i++)
            {
                total += Distancia(puntos[i - 1], puntos[i]);
            }

            float restante = total * Math.Max(0f, progreso);
            PointF[] temporal = new PointF[puntos.Length];
            temporal[0] = puntos[0];
            int cantidad = 1;

            for (int i = 1; i < puntos.Length && restante > 0f; i++)
            {
                float tramo = Distancia(puntos[i - 1], puntos[i]);

                if (restante >= tramo)
                {
                    temporal[cantidad] = puntos[i];
                    restante -= tramo;
                }
                else
                {
                    float f = restante / tramo;
                    temporal[cantidad] = new PointF(puntos[i - 1].X + (puntos[i].X - puntos[i - 1].X) * f, puntos[i - 1].Y + (puntos[i].Y - puntos[i - 1].Y) * f);
                    restante = 0f;
                }

                cantidad++;
            }

            PointF[] resultado = new PointF[cantidad];
            Array.Copy(temporal, resultado, cantidad);
            return resultado;
        }

        private static float Distancia(PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static Pen PlumaRedonda(Color color, float grosor)
        {
            Pen pluma = new Pen(color, grosor);
            pluma.StartCap = LineCap.Round;
            pluma.EndCap = LineCap.Round;
            pluma.LineJoin = LineJoin.Round;
            return pluma;
        }

        private static GraphicsPath RutaRedonda(float x, float y, float ancho, float alto, float radio)
        {
            GraphicsPath ruta = new GraphicsPath();
            float diametro = radio * 2;
            ruta.AddArc(x, y, diametro, diametro, 180, 90);
            ruta.AddArc(x + ancho - diametro, y, diametro, diametro, 270, 90);
            ruta.AddArc(x + ancho - diametro, y + alto - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(x, y + alto - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}
