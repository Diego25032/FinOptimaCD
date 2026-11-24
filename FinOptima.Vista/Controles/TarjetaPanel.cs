using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista.Controles
{
    public class TarjetaPanel : Panel
    {
        private Color colorFondo;
        private Color colorBorde;
        private Color colorInicio;
        private Color colorFin;
        private int radio;
        private Bitmap fondoCache;
        private int anchoCache;
        private int altoCache;
        private bool resaltada;
        private bool resaltarAlPasar;

        public TarjetaPanel()
        {
            Tema tema = TemaActual.Actual;
            colorFondo = tema.Superficie;
            colorBorde = tema.BordeSuave;
            colorInicio = Color.Empty;
            colorFin = Color.Empty;
            radio = 16;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = colorFondo;
            resaltarAlPasar = true;
        }

        /// <summary>Si es verdadero, el borde se ilumina con el color de acento al pasar el mouse.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ResaltarAlPasar
        {
            get { return resaltarAlPasar; }
            set { resaltarAlPasar = value; }
        }

        protected override void OnMouseEnter(System.EventArgs argumentos)
        {
            base.OnMouseEnter(argumentos);
            ActualizarResaltado();
        }

        protected override void OnMouseLeave(System.EventArgs argumentos)
        {
            base.OnMouseLeave(argumentos);
            ActualizarResaltado();
        }

        protected override void OnControlAdded(ControlEventArgs argumentos)
        {
            base.OnControlAdded(argumentos);
            argumentos.Control.MouseEnter += AlCambiarMouseHijo;
            argumentos.Control.MouseLeave += AlCambiarMouseHijo;
        }

        protected override void OnControlRemoved(ControlEventArgs argumentos)
        {
            base.OnControlRemoved(argumentos);
            argumentos.Control.MouseEnter -= AlCambiarMouseHijo;
            argumentos.Control.MouseLeave -= AlCambiarMouseHijo;
        }

        private void AlCambiarMouseHijo(object remitente, System.EventArgs argumentos)
        {
            ActualizarResaltado();
        }

        private void ActualizarResaltado()
        {
            bool dentro = resaltarAlPasar && IsHandleCreated && ClientRectangle.Contains(PointToClient(Cursor.Position));

            if (dentro != resaltada)
            {
                resaltada = dentro;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorFondo
        {
            get { return colorFondo; }
            set
            {
                colorFondo = value;
                BackColor = value;
                RestablecerFondo();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorBorde
        {
            get { return colorBorde; }
            set
            {
                colorBorde = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorInicio
        {
            get { return colorInicio; }
            set
            {
                colorInicio = value;
                RestablecerFondo();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorFin
        {
            get { return colorFin; }
            set
            {
                colorFin = value;
                RestablecerFondo();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Radio
        {
            get { return radio; }
            set
            {
                radio = value;
                RestablecerFondo();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs argumentos)
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            if (fondoCache == null || anchoCache != Width || altoCache != Height)
            {
                CrearFondo();
            }

            if (fondoCache != null)
            {
                CompositingMode anterior = argumentos.Graphics.CompositingMode;
                argumentos.Graphics.CompositingMode = CompositingMode.SourceCopy;
                argumentos.Graphics.DrawImage(fondoCache, new Rectangle(0, 0, Width, Height), 0, 0, Width, Height, GraphicsUnit.Pixel);
                argumentos.Graphics.CompositingMode = anterior;
            }
        }

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            base.OnPaint(argumentos);

            if (Width <= 1 || Height <= 1)
            {
                return;
            }

            Graphics grafico = argumentos.Graphics;
            grafico.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath ruta = CrearRuta(new Rectangle(0, 0, Width - 1, Height - 1), radio))
            {
                if (resaltada)
                {
                    Color acento = TemaActual.Actual.AcentoVisible(TemaActual.Acento);

                    using (Pen lapiz = new Pen(acento, 2f))
                    {
                        grafico.DrawPath(lapiz, ruta);
                    }
                }
                else
                {
                    using (Pen lapiz = new Pen(colorBorde))
                    {
                        grafico.DrawPath(lapiz, ruta);
                    }
                }
            }
        }

        protected override void OnResize(System.EventArgs argumentos)
        {
            base.OnResize(argumentos);
            RestablecerFondo();
        }

        protected override void OnParentChanged(System.EventArgs argumentos)
        {
            base.OnParentChanged(argumentos);
            RestablecerFondo();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LiberarFondo();
            }

            base.Dispose(disposing);
        }

        private void CrearFondo()
        {
            LiberarFondo();

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            Bitmap nuevo = new Bitmap(Width, Height);

            using (Graphics grafico = Graphics.FromImage(nuevo))
            {
                grafico.SmoothingMode = SmoothingMode.AntiAlias;
                grafico.Clear(ColorFondoPadre());

                using (GraphicsPath ruta = CrearRuta(ClientRectangle, radio))
                {
                    if (colorInicio != Color.Empty && colorFin != Color.Empty)
                    {
                        using (LinearGradientBrush pincel = new LinearGradientBrush(ClientRectangle, colorInicio, colorFin, LinearGradientMode.Horizontal))
                        {
                            grafico.FillPath(pincel, ruta);
                        }
                    }
                    else
                    {
                        using (SolidBrush pincel = new SolidBrush(colorFondo))
                        {
                            grafico.FillPath(pincel, ruta);
                        }
                    }
                }
            }

            fondoCache = nuevo;
            anchoCache = Width;
            altoCache = Height;
        }

        private Color ColorFondoPadre()
        {
            if (Parent != null && Parent.BackColor != Color.Transparent)
            {
                return Parent.BackColor;
            }

            return TemaActual.Actual.Fondo;
        }

        private void RestablecerFondo()
        {
            LiberarFondo();
            Invalidate();
        }

        private void LiberarFondo()
        {
            if (fondoCache != null)
            {
                fondoCache.Dispose();
                fondoCache = null;
            }

            anchoCache = 0;
            altoCache = 0;
        }

        private static GraphicsPath CrearRuta(Rectangle area, int radio)
        {
            GraphicsPath ruta = new GraphicsPath();
            int diametro = radio * 2;

            if (diametro > area.Width)
            {
                diametro = area.Width;
            }

            if (diametro > area.Height)
            {
                diametro = area.Height;
            }

            if (diametro <= 0)
            {
                ruta.AddRectangle(area);
                return ruta;
            }

            ruta.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
            ruta.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
            ruta.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}
