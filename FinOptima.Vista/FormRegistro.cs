using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormRegistro : Form
    {
        private FabricaServicios servicios;
        private TextBox campoNombre;
        private TextBox campoCorreo;
        private TextBox campoContrasena;
        private TextBox campoConfirmacion;
        private string correoRegistrado;

        public string CorreoRegistrado
        {
            get { return correoRegistrado; }
        }

        public FormRegistro(FabricaServicios fabrica)
        {
            servicios = fabrica;
            correoRegistrado = "";
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Text = "FinOptima - Crear cuenta";
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = TemaActual.Actual.Fondo;
            ForeColor = TemaActual.Actual.Texto;
            Font = new Font("Segoe UI", 10);
            AutoScroll = true;

            Label titulo = new Label();
            titulo.Text = "Crear una cuenta";
            titulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titulo.Location = new Point(30, 20);
            titulo.AutoSize = true;
            titulo.ForeColor = TemaActual.Actual.Texto;
            titulo.BackColor = Color.Transparent;
            Controls.Add(titulo);

            Label nota = UtilidadesVista.CrearNota("Las cuentas nuevas se crean como usuarios normales. Un administrador puede cambiar el rol.", 32, titulo.Bottom + 12, 400);
            Controls.Add(nota);

            int yNombre = nota.Bottom + 14;
            Label rotuloNombre = UtilidadesVista.CrearEtiqueta("Nombre completo", 32, yNombre);
            Controls.Add(rotuloNombre);
            campoNombre = UtilidadesVista.CrearCampo(32, rotuloNombre.Bottom + 6, 400);
            Controls.Add(campoNombre);

            int yCorreo = campoNombre.Bottom + 14;
            Label rotuloCorreo = UtilidadesVista.CrearEtiqueta("Correo electronico", 32, yCorreo);
            Controls.Add(rotuloCorreo);
            campoCorreo = UtilidadesVista.CrearCampo(32, rotuloCorreo.Bottom + 6, 400);
            Controls.Add(campoCorreo);

            int yContrasena = campoCorreo.Bottom + 14;
            Label rotuloContrasena = UtilidadesVista.CrearEtiqueta("Contrasena (minimo 8 caracteres y un numero)", 32, yContrasena);
            Controls.Add(rotuloContrasena);
            campoContrasena = UtilidadesVista.CrearCampo(32, rotuloContrasena.Bottom + 6, 400);
            campoContrasena.UseSystemPasswordChar = true;
            Controls.Add(campoContrasena);

            int yConfirmacion = campoContrasena.Bottom + 14;
            Label rotuloConfirmacion = UtilidadesVista.CrearEtiqueta("Confirmar contrasena", 32, yConfirmacion);
            Controls.Add(rotuloConfirmacion);
            campoConfirmacion = UtilidadesVista.CrearCampo(32, rotuloConfirmacion.Bottom + 6, 400);
            campoConfirmacion.UseSystemPasswordChar = true;
            Controls.Add(campoConfirmacion);

            int yBotones = campoConfirmacion.Bottom + 22;
            Button crear = UtilidadesVista.CrearBoton("Crear cuenta", 32, yBotones, 190, 38, TemaActual.Acento);
            crear.Click += AlRegistrar;
            Controls.Add(crear);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 242, yBotones, 190, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = crear;
            CancelButton = cancelar;
            ClientSize = new Size(480, Math.Max(440, cancelar.Bottom + 30));

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
        }

        private void AlRegistrar(object remitente, EventArgs argumentos)
        {
            try
            {
                Resultado resultado = servicios.Autenticacion.Registrar(
                    campoNombre.Text, campoCorreo.Text, campoContrasena.Text, campoConfirmacion.Text);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                correoRegistrado = campoCorreo.Text.Trim().ToLower();
                UtilidadesVista.Informar(resultado.Mensaje);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlCancelar(object remitente, EventArgs argumentos)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
