using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormTelegram : Form
    {
        private readonly ServicioBotLocal servicio;
        private readonly Timer temporizador;
        private Label etiquetaEstado;
        private Label etiquetaBot;
        private Label etiquetaError;
        private PictureBox codigoQr;
        private Label instrucciones;
        private Button botonAbrir;
        private Button botonReintentar;
        private Button botonCerrar;
        private string enlaceQr;

        public FormTelegram(ServicioBotLocal servicioBot)
        {
            servicio = servicioBot;
            temporizador = new Timer();
            temporizador.Interval = 750;
            temporizador.Tick += AlActualizarEstado;
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;
            int anchoVentana = 540;

            Text = "FinOptima - Telegram";
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = tema.Fondo;
            ForeColor = tema.Texto;
            Font = new Font("Segoe UI", 10);

            Label titulo = UtilidadesVista.CrearTitulo("Usar FinOptima desde Telegram", 24, 20);
            Controls.Add(titulo);

            etiquetaEstado = new Label();
            etiquetaEstado.AutoSize = true;
            etiquetaEstado.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            etiquetaEstado.Location = new Point(24, titulo.Bottom + 10);
            etiquetaEstado.BackColor = Color.Transparent;
            Controls.Add(etiquetaEstado);

            etiquetaBot = UtilidadesVista.CrearEtiquetaSuave("", 24, etiquetaEstado.Bottom + 4);
            Controls.Add(etiquetaBot);

            codigoQr = new PictureBox();
            codigoQr.Location = new Point((anchoVentana - 240) / 2, etiquetaBot.Bottom + 16);
            codigoQr.Size = new Size(240, 240);
            codigoQr.BackColor = Color.White;
            codigoQr.BorderStyle = BorderStyle.FixedSingle;
            codigoQr.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(codigoQr);

            instrucciones = UtilidadesVista.CrearNota(
                "Escanea el QR con la camara de tu telefono. Se abrira el chat del bot; envia /start para ver los botones.",
                24, codigoQr.Bottom + 14, anchoVentana - 48);
            Controls.Add(instrucciones);

            etiquetaError = UtilidadesVista.CrearNota("", 24, instrucciones.Bottom + 8, anchoVentana - 48);
            etiquetaError.ForeColor = tema.Peligro;
            etiquetaError.Visible = false;
            Controls.Add(etiquetaError);

            int yBotones = Math.Max(instrucciones.Bottom, etiquetaError.Bottom) + 18;
            botonAbrir = UtilidadesVista.CrearBoton("Abrir Telegram", 24, yBotones, 180, 38, TemaActual.Acento);
            botonAbrir.Click += AlAbrirTelegram;
            Controls.Add(botonAbrir);

            botonReintentar = UtilidadesVista.CrearBotonSecundario("Reintentar", 216, yBotones, 130, 38);
            botonReintentar.Click += AlReintentar;
            Controls.Add(botonReintentar);

            botonCerrar = UtilidadesVista.CrearBotonSecundario("Cerrar", 358, yBotones, 130, 38);
            botonCerrar.Click += AlCerrar;
            Controls.Add(botonCerrar);

            CancelButton = botonCerrar;
            ClientSize = new Size(anchoVentana, Math.Max(600, botonCerrar.Bottom + 26));

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
            etiquetaError.TextChanged += delegate(object remitente, EventArgs argumentos)
            {
                AjustarBotones();
            };
            AjustarBotones();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UtilidadesVista.AjustarVentanaAPantalla(this);
            AjustarBotones();
            servicio.IniciarAsync();
            ActualizarEstado();
            temporizador.Start();
        }

        private void AlActualizarEstado(object remitente, EventArgs argumentos)
        {
            ActualizarEstado();
        }

        private void AjustarBotones()
        {
            if (botonAbrir == null || botonReintentar == null || botonCerrar == null)
            {
                return;
            }

            int y = Math.Max(instrucciones.Bottom, etiquetaError.Visible ? etiquetaError.Bottom : instrucciones.Bottom) + 18;
            botonAbrir.Top = y;
            botonReintentar.Top = y;
            botonCerrar.Top = y;
            AutoScrollMinSize = new Size(
                Math.Max(540, botonCerrar.Right + 24),
                Math.Max(600, botonCerrar.Bottom + 26));
        }

        private void ActualizarEstado()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            Tema tema = TemaActual.Actual;
            Exception error = servicio.Error;

            etiquetaEstado.Text = servicio.Estado;
            etiquetaEstado.ForeColor = servicio.EstaActivo ? tema.Exito : (error == null ? tema.TextoSecundario : tema.Peligro);
            etiquetaBot.Text = servicio.NombreUsuario.Length == 0 ? "Conectando con Telegram..." : "@" + servicio.NombreUsuario.TrimStart('@');
            etiquetaError.Text = error == null ? "" : error.Message;
            etiquetaError.Visible = error != null;
            AjustarBotones();

            string enlace = servicio.EnlaceTelegram;
            botonAbrir.Enabled = enlace.Length > 0;

            if (enlace.Length > 0 && enlace != enlaceQr)
            {
                try
                {
                    Bitmap nueva = servicio.CrearQr(240);
                    Bitmap anterior = codigoQr.Image as Bitmap;
                    codigoQr.Image = nueva;
                    enlaceQr = enlace;

                    if (anterior != null)
                    {
                        anterior.Dispose();
                    }
                }
                catch (Exception excepcion)
                {
                    etiquetaError.Text = excepcion.Message;
                    etiquetaError.Visible = true;
                    AjustarBotones();
                }
            }
        }

        private void AlAbrirTelegram(object remitente, EventArgs argumentos)
        {
            string enlace = servicio.EnlaceTelegram;

            if (enlace.Length == 0)
            {
                UtilidadesVista.AvisarError("El bot todavia no esta listo para abrirse.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(enlace)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlReintentar(object remitente, EventArgs argumentos)
        {
            servicio.IniciarAsync();
            ActualizarEstado();
        }

        private void AlCerrar(object remitente, EventArgs argumentos)
        {
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            temporizador.Stop();
            temporizador.Dispose();

            if (codigoQr.Image != null)
            {
                codigoQr.Image.Dispose();
                codigoQr.Image = null;
            }

            base.OnFormClosed(e);
        }
    }
}
