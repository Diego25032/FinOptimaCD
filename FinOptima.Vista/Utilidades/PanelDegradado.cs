using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FinOptima.Vista.Utilidades
{
    public class PanelDegradado : Panel
    {
        private Color inicio;
        private Color fin;
        private Color acento;

        public PanelDegradado()
        {
            inicio = Color.Empty;
            fin = Color.Empty;
            acento = Color.Empty;
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Inicio
        {
            get { return inicio; }
            set { inicio = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Fin
        {
            get { return fin; }
            set { fin = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Acento
        {
            get { return acento; }
            set { acento = value; }
        }

        protected override void OnPaintBackground(PaintEventArgs argumentos)
        {
            Rectangle area = ClientRectangle;

            if (area.Width <= 0 || area.Height <= 0 || inicio == Color.Empty || fin == Color.Empty)
            {
                base.OnPaintBackground(argumentos);
                return;
            }

            using (LinearGradientBrush pincel = new LinearGradientBrush(area, inicio, fin, LinearGradientMode.Horizontal))
            {
                argumentos.Graphics.FillRectangle(pincel, area);
            }

            if (acento != Color.Empty)
            {
                using (SolidBrush linea = new SolidBrush(acento))
                {
                    argumentos.Graphics.FillRectangle(linea, 0, area.Height - 3, area.Width, 3);
                }
            }
        }
    }
}
