using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloBase : UserControl
    {
        protected FabricaServicios Servicios;
        protected Color Acento;
        protected float Letra;
        protected int Margen;
        protected bool Recalculando;

        private Panel lineaTitulo;
        private Timer temporizadorScroll;
        private int pasoMuesca;
        private int scrollPendiente;
        private bool scrollProgramado;

        public ModuloBase(FabricaServicios servicios, string titulo)
        {
            Servicios = servicios;
            Margen = 24;
            Acento = TemaActual.Actual.AcentoVisible(UtilidadesVista.AcentoDeUsuario(servicios.Sesion.Usuario.ColorAcento));
            Letra = UtilidadesVista.TamanoFuenteSeguro(servicios.Sesion.Usuario.TamanoFuente);

            Dock = DockStyle.Fill;
            AutoScroll = true;
            DoubleBuffered = true;
            BackColor = TemaActual.Actual.Fondo;
            Font = new Font("Segoe UI", Letra);

            // Los modulos empiezan su contenido en y=52; el titulo no crece mas para no invadirlo.
            float tamanoTitulo = Letra + 8;

            if (tamanoTitulo > 18)
            {
                tamanoTitulo = 18;
            }

            Label rotulo = new Label();
            rotulo.Text = titulo;
            rotulo.Location = new Point(Margen, 8);
            rotulo.AutoSize = true;
            rotulo.Font = new Font("Segoe UI", tamanoTitulo, FontStyle.Bold);
            rotulo.ForeColor = TemaActual.Actual.Texto;
            rotulo.BackColor = Color.Transparent;
            Controls.Add(rotulo);

            lineaTitulo = new Panel();
            lineaTitulo.Location = new Point(Margen, rotulo.Bottom + 5);
            lineaTitulo.Size = new Size(52, 3);
            lineaTitulo.BackColor = Acento;
            Controls.Add(lineaTitulo);
        }

        /// <summary>
        /// Primera coordenada Y libre debajo del titulo del modulo. Se calcula con la fuente
        /// real para que nada quede tapado cuando Windows usa escalado de 125% o 150%.
        /// </summary>
        protected int InicioContenido
        {
            get
            {
                // lineaTitulo.Bottom ya incluye el desplazamiento del scroll, asi que el valor
                // devuelto sirve directamente para SetBounds.
                int inicio = lineaTitulo.Bottom + 14;
                int minimo = 52 + AutoScrollPosition.Y;

                if (inicio < minimo)
                {
                    inicio = minimo;
                }

                return inicio;
            }
        }

        /// <summary>
        /// Factor de escala de la pantalla (1 = 100%, 1.5 = 150%).
        /// </summary>
        protected float Escala
        {
            get
            {
                float escala = DeviceDpi / 96f;

                if (escala < 1)
                {
                    escala = 1;
                }

                return escala;
            }
        }

        protected int Escalar(int valor)
        {
            return (int)Math.Round(valor * Escala);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parametros = base.CreateParams;
                parametros.ExStyle = parametros.ExStyle | 0x02000000;
                return parametros;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs argumentos)
        {
            if (!VerticalScroll.Visible || argumentos.Delta == 0)
            {
                base.OnMouseWheel(argumentos);
                return;
            }

            int muescas = Math.Abs(argumentos.Delta) / 120;

            if (muescas == 0)
            {
                muescas = 1;
            }

            if (pasoMuesca == 0 && !scrollProgramado)
            {
                int anterior = -AutoScrollPosition.Y;
                base.OnMouseWheel(argumentos);
                int actual = -AutoScrollPosition.Y;
                int recorrido = actual - anterior;
                int maximo = AutoScrollMinSize.Height - ClientSize.Height;

                if (recorrido != 0 && actual != 0 && actual != maximo)
                {
                    pasoMuesca = Math.Abs(recorrido) / muescas;
                }

                ProgramarScroll();
                return;
            }

            HandledMouseEventArgs manejado = argumentos as HandledMouseEventArgs;

            if (manejado != null)
            {
                manejado.Handled = true;
            }

            int sentido = 1;

            if (argumentos.Delta > 0)
            {
                sentido = -1;
            }

            scrollPendiente = scrollPendiente + sentido * muescas * pasoMuesca;
            ProgramarScroll();
        }

        private void ProgramarScroll()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            scrollProgramado = true;

            if (temporizadorScroll == null)
            {
                temporizadorScroll = new Timer();
                temporizadorScroll.Interval = 35;
                temporizadorScroll.Tick += AlAplicarScroll;
            }

            if (!temporizadorScroll.Enabled)
            {
                temporizadorScroll.Start();
            }
        }

        private void AlAplicarScroll(object remitente, EventArgs argumentos)
        {
            temporizadorScroll.Stop();
            scrollProgramado = false;

            int pendiente = scrollPendiente;
            scrollPendiente = 0;

            if (pendiente == 0 || IsDisposed)
            {
                return;
            }

            int actual = -AutoScrollPosition.Y;
            int maximo = AutoScrollMinSize.Height - ClientSize.Height;

            if (maximo < 0)
            {
                maximo = 0;
            }

            int destino = actual + pendiente;

            if (destino < 0)
            {
                destino = 0;
            }

            if (destino > maximo)
            {
                destino = maximo;
            }

            if (destino == actual)
            {
                return;
            }

            AutoScrollPosition = new Point(0, destino);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && temporizadorScroll != null)
            {
                temporizadorScroll.Stop();
                temporizadorScroll.Dispose();
                temporizadorScroll = null;
            }

            base.Dispose(disposing);
        }

        protected void MostrarResultado(Resultado resultado)
        {
            if (resultado.Exito)
            {
                UtilidadesVista.Informar(resultado.Mensaje);
            }
            else
            {
                UtilidadesVista.AvisarError(resultado.Mensaje);
            }
        }

        protected void MostrarError(Exception error)
        {
            UtilidadesVista.AvisarError(error.Message);
        }

        protected Button Boton(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBoton(texto, x, y, ancho, 32, Acento);
        }

        protected Button BotonSecundario(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBotonSecundario(texto, x, y, ancho, 32);
        }

        protected Button BotonPeligro(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBotonPeligro(texto, x, y, ancho, 32);
        }

        protected int FilaCampo(int yEtiqueta)
        {
            return yEtiqueta + TextRenderer.MeasureText("Ag", Font).Height + 4;
        }
    }
}
