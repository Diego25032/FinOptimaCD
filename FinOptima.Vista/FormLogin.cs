using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormLogin : Form
    {
        private FabricaServicios servicios;
        private TextBox campoCorreo;
        private TextBox campoContrasena;
        private Label etiquetaEstadoCon;
        private Label notaConexion;
        private ServicioBotLocal botLocal;
        private bool principalCerroSesion;

        public FormLogin(FabricaServicios fabrica)
        {
            servicios = fabrica;
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            Text = "FinOptima - Iniciar sesion";
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            UtilidadesVista.AplicarIcono(this);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScroll = true;
            BackColor = tema.Fondo;
            ForeColor = tema.Texto;
            Font = new Font("Segoe UI", 10);

            Font fuenteTitulo = new Font("Segoe UI", 22, FontStyle.Bold);
            Font fuenteSubtitulo = new Font("Segoe UI", 9);
            string textoTitulo = "FINOPTIMA";
            string textoSubtitulo = "Finanzas personales con el metodo de los 7 sobres";

            Color acento = tema.AcentoVisible(TemaActual.Acento);
            Color finCabecera = Tema.Mezclar(tema.Cabecera, acento, 25);

            PanelDegradado cabecera = new PanelDegradado();
            cabecera.Dock = DockStyle.Top;
            cabecera.Inicio = tema.Cabecera;
            cabecera.Fin = finCabecera;
            cabecera.Acento = acento;
            Controls.Add(cabecera);

            Label titulo = new Label();
            titulo.Text = textoTitulo;
            titulo.ForeColor = tema.CabeceraTexto;
            titulo.BackColor = Color.Transparent;
            titulo.Font = fuenteTitulo;
            titulo.Location = new Point(30, 14);
            titulo.AutoSize = true;
            cabecera.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = textoSubtitulo;
            subtitulo.ForeColor = tema.CabeceraSuave;
            subtitulo.BackColor = Color.Transparent;
            subtitulo.Font = fuenteSubtitulo;
            subtitulo.AutoSize = true;
            subtitulo.MaximumSize = new Size(440, 0);
            subtitulo.Location = new Point(33, titulo.Bottom + 6);
            cabecera.Controls.Add(subtitulo);

            int alturaCabecera = subtitulo.Bottom + 18;
            cabecera.Height = alturaCabecera;

            int y = alturaCabecera + 26;
            int altoTexto = TextRenderer.MeasureText("Ag", Font).Height;
            int altoCampo = altoTexto + 10;

            Label rotuloCorreo = UtilidadesVista.CrearEtiqueta("Correo electronico", 33, y);
            Controls.Add(rotuloCorreo);

            campoCorreo = UtilidadesVista.CrearCampo(33, y + altoTexto + 4, 440);
            campoCorreo.Text = "";
            Controls.Add(campoCorreo);

            int yRotuloContrasena = campoCorreo.Top + altoCampo + 18;
            Label rotuloContrasena = UtilidadesVista.CrearEtiqueta("Contrasena", 33, yRotuloContrasena);
            Controls.Add(rotuloContrasena);

            campoContrasena = UtilidadesVista.CrearCampo(33, yRotuloContrasena + altoTexto + 4, 440);
            campoContrasena.UseSystemPasswordChar = true;
            Controls.Add(campoContrasena);

            int yBotones = campoContrasena.Top + altoCampo + 20;
            Button entrar = UtilidadesVista.CrearBoton("Iniciar sesion", 33, yBotones, 210, 40, TemaActual.Acento);
            entrar.Click += AlIniciarSesion;
            Controls.Add(entrar);

            Button registrar = UtilidadesVista.CrearBotonSecundario("Crear cuenta", 263, yBotones, 210, 40);
            registrar.Click += AlCrearCuenta;
            Controls.Add(registrar);

            int yEstado = yBotones + 60;
            etiquetaEstadoCon = new Label();
            etiquetaEstadoCon.Location = new Point(33, yEstado);
            etiquetaEstadoCon.AutoSize = true;
            etiquetaEstadoCon.MaximumSize = new Size(440, 0);
            etiquetaEstadoCon.Text = "Comprobando conexion con PostgreSQL...";
            etiquetaEstadoCon.ForeColor = tema.TextoSecundario;
            etiquetaEstadoCon.BackColor = Color.Transparent;
            Controls.Add(etiquetaEstadoCon);

            notaConexion = UtilidadesVista.CrearNota("Los datos se guardan en PostgreSQL. Revisa Docker o FINOPTIMA_CONEXION si la conexion falla.", 33, 0, 450);
            notaConexion.Font = new Font("Segoe UI", 8);
            Controls.Add(notaConexion);

            AcceptButton = entrar;
            AjustarTamanoEstado();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ProbarConexion();
        }

        private void ProbarConexion()
        {
            try
            {
                servicios.ProbarConexion();
                etiquetaEstadoCon.Text = "Conexion con PostgreSQL: correcta.";
                etiquetaEstadoCon.ForeColor = TemaActual.Actual.Exito;
            }
            catch (Exception error)
            {
                etiquetaEstadoCon.Text = "Conexion: " + error.Message;
                etiquetaEstadoCon.ForeColor = TemaActual.Actual.Peligro;
            }

            AjustarTamanoEstado();
        }

        private void AjustarTamanoEstado()
        {
            notaConexion.Top = etiquetaEstadoCon.Bottom + 10;
            int ancho = Math.Max(520, campoCorreo.Right + 33);
            ClientSize = new Size(ancho, Math.Max(460, notaConexion.Bottom + 26));
            UtilidadesVista.AjustarVentanaAPantalla(this);
        }

        private void AlIniciarSesion(object remitente, EventArgs argumentos)
        {
            try
            {
                ResultadoLogin resultado = servicios.Autenticacion.IniciarSesion(campoCorreo.Text, campoContrasena.Text);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                AbrirPrincipal();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlCrearCuenta(object remitente, EventArgs argumentos)
        {
            using (FormRegistro registro = new FormRegistro(servicios))
            {
                DialogResult resultado = registro.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    campoCorreo.Text = registro.CorreoRegistrado;
                    campoContrasena.Text = "";
                    campoContrasena.Focus();
                }
            }
        }

        private void AbrirPrincipal()
        {
            campoContrasena.Clear();
            Hide();

            botLocal = new ServicioBotLocal(servicios);
            botLocal.IniciarAsync();

            if (servicios.Sesion.EsAdministrador)
            {
                using (FormPrincipalAdministrador principal = new FormPrincipalAdministrador(servicios, botLocal))
                {
                    principal.ShowDialog();
                    principalCerroSesion = principal.CerroSesion;
                }
            }
            else
            {
                using (FormPrincipal principal = new FormPrincipal(servicios, botLocal))
                {
                    principal.ShowDialog();
                    principalCerroSesion = principal.CerroSesion;
                }
            }

            DetenerBotLocal();

            if (servicios.Sesion.HaySesion)
            {
                servicios.Autenticacion.CerrarSesion();
            }

            if (principalCerroSesion)
            {
                campoContrasena.Clear();
                Show();
                campoContrasena.Focus();
            }
            else
            {
                Close();
            }
        }

        private void DetenerBotLocal()
        {
            if (botLocal == null)
            {
                return;
            }

            botLocal.Detener();
            botLocal.Dispose();
            botLocal = null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DetenerBotLocal();
            base.OnFormClosed(e);
        }
    }
}
