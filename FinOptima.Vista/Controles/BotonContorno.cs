using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Controles
{
    public class BotonContorno : Button
    {
        private Color colorBorde;

        public BotonContorno()
        {
            colorBorde = TemaActual.Actual.Borde;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            ResizeRedraw = true;
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

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            UtilidadesVista.Redondear(this, UtilidadesVista.RadioBoton);
        }

        protected override void OnPaint(PaintEventArgs argumentos)
        {
            base.OnPaint(argumentos);

            int ancho = Width - 3;
            int alto = Height - 3;

            if (ancho <= 2 || alto <= 2)
            {
                return;
            }

            Graphics grafico = argumentos.Graphics;
            grafico.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath ruta = UtilidadesVista.CrearRutaRedonda(new Rectangle(1, 1, ancho, alto), UtilidadesVista.RadioBoton))
            {
                using (Pen lapiz = new Pen(colorBorde, 1.4f))
                {
                    grafico.DrawPath(lapiz, ruta);
                }
            }
        }
    }
}
