using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public class TemaOscuro : Tema
    {
        public override string Nombre
        {
            get { return "Oscuro"; }
        }

        public override TemaVisual Visual
        {
            get { return TemaVisual.Oscuro; }
        }

        public override Color Fondo
        {
            get { return Color.FromArgb(11, 15, 14); }
        }

        public override Color Superficie
        {
            get { return Color.FromArgb(21, 26, 24); }
        }

        public override Color SuperficieAlterna
        {
            get { return Color.FromArgb(28, 35, 32); }
        }

        public override Color Texto
        {
            get { return Color.FromArgb(243, 246, 244); }
        }

        public override Color TextoSecundario
        {
            get { return Color.FromArgb(152, 163, 157); }
        }

        public override Color Borde
        {
            get { return Color.FromArgb(48, 59, 54); }
        }

        public override Color Cabecera
        {
            get { return Color.FromArgb(10, 14, 13); }
        }

        public override Color CabeceraTexto
        {
            get { return Color.FromArgb(245, 248, 246); }
        }

        public override Color CabeceraSuave
        {
            get { return Color.FromArgb(147, 160, 153); }
        }

        public override Color Menu
        {
            get { return Color.FromArgb(14, 19, 17); }
        }

        public override Color MenuTexto
        {
            get { return Color.FromArgb(201, 210, 205); }
        }

        public override Color Peligro
        {
            get { return Color.FromArgb(243, 18, 96); }
        }

        public override Color Exito
        {
            get { return Color.FromArgb(23, 201, 100); }
        }

        public override Color AcentoVisible(Color acento)
        {
            if (Brillo(acento) < 110)
            {
                return Mezclar(acento, Color.White, 35);
            }

            return acento;
        }

        public override Color AcentoSuave(Color acento)
        {
            return Mezclar(Superficie, acento, 16);
        }
    }
}
