using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista
{
    /// <summary>
    /// Pantalla de bienvenida corta: logo animado, nombre y barra de carga, con aparicion y salida suaves.
    /// </summary>
    public class FormBienvenida : Form
    {
        private readonly Timer temporizador;
        private readonly LogoFinOptima logo;
        private readonly Panel barraFondo;
        private readonly Panel barraProgreso;
        private int cuadro;
        private const int CuadrosEntrada = 15;
        private const int CuadrosTotales = 110;
        private const int CuadrosSalida = 15;

        public FormBienvenida()
        {
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(420, 300);
            BackColor = tema.Superficie;
            Opacity = 0;
            Text = "FinOptima";
            Icon = Icon.FromHandle(LogoFinOptima.CrearImagen(64, acento).GetHicon());

            logo = new LogoFinOptima();
            logo.ColorAcento = acento;
            logo.Bounds = new Rectangle(160, 48, 100, 100);
            Controls.Add(logo);

            Label nombre = new Label();
            nombre.Text = "FinOptima";
            nombre.Font = new Font("Segoe UI Semibold", 22f);
            nombre.ForeColor = tema.Texto;
            nombre.TextAlign = ContentAlignment.MiddleCenter;
            nombre.Bounds = new Rectangle(0, 160, 420, 44);
            Controls.Add(nombre);

            Label lema = new Label();
            lema.Text = "Tu dinero, en orden con los 7 sobres";
            lema.Font = new Font("Segoe UI", 10f);
            lema.ForeColor = tema.TextoSecundario;
            lema.TextAlign = ContentAlignment.MiddleCenter;
            lema.Bounds = new Rectangle(0, 204, 420, 24);
            Controls.Add(lema);

            barraFondo = new Panel();
            barraFondo.BackColor = tema.SuperficieAlterna;
            barraFondo.Bounds = new Rectangle(110, 250, 200, 4);
            Controls.Add(barraFondo);

            barraProgreso = new Panel();
            barraProgreso.BackColor = acento;
            barraProgreso.Bounds = new Rectangle(0, 0, 0, 4);
            barraFondo.Controls.Add(barraProgreso);

            temporizador = new Timer();
            temporizador.Interval = 16;
            temporizador.Tick += new EventHandler(AlAvanzar);
        }

        protected override void OnShown(EventArgs argumentos)
        {
            base.OnShown(argumentos);
            logo.Animar();
            temporizador.Start();
        }

        protected override void OnClick(EventArgs argumentos)
        {
            base.OnClick(argumentos);
            // Un clic salta la bienvenida.
            if (cuadro < CuadrosTotales - CuadrosSalida)
            {
                cuadro = CuadrosTotales - CuadrosSalida;
            }
        }

        private void AlAvanzar(object remitente, EventArgs argumentos)
        {
            cuadro++;

            if (cuadro <= CuadrosEntrada)
            {
                Opacity = (double)cuadro / CuadrosEntrada;
            }
            else if (cuadro >= CuadrosTotales - CuadrosSalida)
            {
                Opacity = Math.Max(0, (double)(CuadrosTotales - cuadro) / CuadrosSalida);
            }

            double avance = Math.Min(1.0, (double)cuadro / (CuadrosTotales - CuadrosSalida));
            barraProgreso.Width = (int)(barraFondo.Width * (1 - Math.Pow(1 - avance, 2)));

            if (cuadro >= CuadrosTotales)
            {
                temporizador.Stop();
                Close();
            }
        }

        protected override void Dispose(bool liberando)
        {
            if (liberando)
            {
                temporizador.Dispose();
            }

            base.Dispose(liberando);
        }
    }
}
