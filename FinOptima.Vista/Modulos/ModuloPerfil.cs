using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloPerfil : ModuloBase
    {
        private Usuario usuario;
        private Label nota;
        private Label etiquetaNombre;
        private Label correo;
        private Label rol;
        private Label etiquetaColor;
        private Label etiquetaFuente;
        private TextBox campoNombre;
        private TextBox campoColor;
        private NumericUpDown campoFuente;
        private CheckBox alertaDeudas;
        private CheckBox alertaMetas;
        private CheckBox alertaPresupuesto;
        private CheckBox alertaSobres;
        private PictureBox foto;
        private Panel muestraColor;
        private Panel marcoColor;
        private GroupBox grupoAlertas;
        private Button cargarFoto;
        private Button quitarFoto;
        private Button guardar;
        private Button contrasena;
        private byte[] fotoSeleccionada;
        private bool fotoCambiada;

        public event EventHandler PerfilCambiado;

        public ModuloPerfil(FabricaServicios servicios) : base(servicios, "Mi perfil")
        {
            fotoSeleccionada = null;
            fotoCambiada = false;

            try
            {
                usuario = Servicios.Perfil.ObtenerActual();
            }
            catch (Exception error)
            {
                MostrarError(error);
                return;
            }

            ConstruirInterfaz();
            CargarDatos();
            Distribuir();
        }

        private void ConstruirInterfaz()
        {
            nota = UtilidadesVista.CrearNota("Actualiza tus datos, la apariencia de la aplicacion y tus alertas.", Margen, InicioContenido, 880);
            Controls.Add(nota);

            int filaNombre = 90;
            etiquetaNombre = UtilidadesVista.CrearEtiqueta("Nombre completo", Margen, filaNombre);
            Controls.Add(etiquetaNombre);
            campoNombre = UtilidadesVista.CrearCampo(Margen, FilaCampo(filaNombre), 300);
            Controls.Add(campoNombre);

            int filaCuenta = FilaCampo(filaNombre) + 34;
            correo = UtilidadesVista.CrearEtiqueta("Correo: " + usuario.Correo, Margen, filaCuenta);
            correo.ForeColor = TemaActual.Actual.TextoSecundario;
            Controls.Add(correo);

            rol = UtilidadesVista.CrearEtiqueta("Rol: " + NombreRol(), Margen, filaCuenta + 26);
            rol.ForeColor = TemaActual.Actual.TextoSecundario;
            Controls.Add(rol);

            int filaColor = filaCuenta + 54;
            etiquetaColor = UtilidadesVista.CrearEtiqueta("Color de acento (#RRGGBB)", Margen, filaColor);
            Controls.Add(etiquetaColor);
            campoColor = UtilidadesVista.CrearCampo(Margen, FilaCampo(filaColor), 140);
            campoColor.TextChanged += AlCambiarColor;
            Controls.Add(campoColor);

            marcoColor = new Panel();
            marcoColor.Location = new Point(Margen + 155, FilaCampo(filaColor));
            marcoColor.Size = new Size(42, 26);
            marcoColor.BackColor = TemaActual.Actual.Borde;
            Controls.Add(marcoColor);

            muestraColor = new Panel();
            muestraColor.Location = new Point(1, 1);
            muestraColor.Size = new Size(40, 24);
            marcoColor.Controls.Add(muestraColor);

            int filaFuente = FilaCampo(filaColor) + 38;
            etiquetaFuente = UtilidadesVista.CrearEtiqueta("Tamano de letra (8 a 14)", Margen, filaFuente);
            Controls.Add(etiquetaFuente);
            campoFuente = new NumericUpDown();
            campoFuente.Location = new Point(Margen, FilaCampo(filaFuente));
            campoFuente.Width = 80;
            campoFuente.Minimum = 8;
            campoFuente.Maximum = 14;
            UtilidadesVista.AplicarEstiloCampo(campoFuente);
            Controls.Add(campoFuente);

            int filaAlertas = FilaCampo(filaFuente) + 38;
            grupoAlertas = new GroupBox();
            grupoAlertas.Text = "Alertas";
            grupoAlertas.Location = new Point(Margen, filaAlertas);
            grupoAlertas.Size = new Size(310, 150);
            grupoAlertas.ForeColor = TemaActual.Actual.Texto;
            grupoAlertas.BackColor = Color.Transparent;
            Controls.Add(grupoAlertas);

            alertaDeudas = CrearAlerta("Deudas por vencer", 28, grupoAlertas);
            alertaMetas = CrearAlerta("Metas cerca de su fecha", 56, grupoAlertas);
            alertaPresupuesto = CrearAlerta("Presupuestos excedidos", 84, grupoAlertas);
            alertaSobres = CrearAlerta("Sobres sin saldo suficiente", 112, grupoAlertas);

            foto = new PictureBox();
            foto.Location = new Point(Margen + 350, 90);
            foto.Size = new Size(140, 140);
            foto.BorderStyle = BorderStyle.FixedSingle;
            foto.SizeMode = PictureBoxSizeMode.Zoom;
            foto.BackColor = TemaActual.Actual.Superficie;
            Controls.Add(foto);

            cargarFoto = BotonSecundario("Cargar foto", Margen + 350, 242, 140);
            cargarFoto.Click += AlCargarFoto;
            Controls.Add(cargarFoto);

            quitarFoto = BotonPeligro("Quitar foto", Margen + 350, 282, 140);
            quitarFoto.Click += AlQuitarFoto;
            Controls.Add(quitarFoto);

            int filaBotones = filaAlertas + 175;
            guardar = Boton("Guardar cambios", Margen, filaBotones, 180);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            contrasena = BotonSecundario("Cambiar contrasena", Margen + 200, filaBotones, 190);
            contrasena.Click += AlCambiarContrasena;
            Controls.Add(contrasena);
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (campoNombre == null || guardar == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int ancho = ClientSize.Width - Margen * 2;
                int alto = DistribuirEnAncho(ancho);

                if (alto > ClientSize.Height && !VerticalScroll.Visible)
                {
                    ancho = ClientSize.Width - Margen * 2 - SystemInformation.VerticalScrollBarWidth;
                    alto = DistribuirEnAncho(ancho);
                }

                AutoScrollMinSize = new Size(
                    Math.Max(ClientSize.Width, ancho + Margen * 2),
                    Math.Max(ClientSize.Height, alto));
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private int DistribuirEnAncho(int ancho)
        {
            int separacion = Escalar(24);
            ancho = Math.Max(Escalar(320), ancho);
            int x = Margen + AutoScrollPosition.X;
            bool fotoALaLado = ancho >= Escalar(760);
            int anchoIzquierdo = fotoALaLado
                ? Math.Min(Escalar(520), ancho - Escalar(200) - separacion)
                : ancho;

            UtilidadesVista.AjustarEtiqueta(nota, ancho);

            int y = nota.Bottom + 14;
            etiquetaNombre.Location = new Point(x, y);
            campoNombre.SetBounds(x, etiquetaNombre.Bottom + 4, Math.Min(Escalar(420), anchoIzquierdo), campoNombre.Height);

            correo.Location = new Point(x, campoNombre.Bottom + 10);
            UtilidadesVista.AjustarEtiqueta(correo, anchoIzquierdo);
            rol.Location = new Point(x, correo.Bottom + 4);
            UtilidadesVista.AjustarEtiqueta(rol, anchoIzquierdo);

            etiquetaColor.Location = new Point(x, rol.Bottom + 14);
            int yColor = etiquetaColor.Bottom + 4;
            int anchoColor = Math.Min(Escalar(140), Math.Max(Escalar(90), anchoIzquierdo - Escalar(90)));
            campoColor.SetBounds(x, yColor, anchoColor, campoColor.Height);

            marcoColor.SetBounds(x + campoColor.Right + 16, yColor, Escalar(42), Escalar(26));
            muestraColor.SetBounds(1, 1, marcoColor.Width - 2, marcoColor.Height - 2);

            etiquetaFuente.Location = new Point(x, marcoColor.Bottom + 14);
            campoFuente.SetBounds(x, etiquetaFuente.Bottom + 4, Escalar(80), campoFuente.Height);

            grupoAlertas.SetBounds(x, campoFuente.Bottom + 16, anchoIzquierdo, Escalar(150));

            int xFoto = fotoALaLado ? x + anchoIzquierdo + separacion : x;
            int yFoto = fotoALaLado ? etiquetaNombre.Top : grupoAlertas.Bottom + 24;
            int ladoFoto = Escalar(140);

            foto.SetBounds(xFoto, yFoto, ladoFoto, ladoFoto);
            cargarFoto.SetBounds(xFoto, foto.Bottom + 12, ladoFoto, cargarFoto.Height);
            quitarFoto.SetBounds(xFoto, cargarFoto.Bottom + 10, ladoFoto, quitarFoto.Height);

            int yBotones = Math.Max(grupoAlertas.Bottom, quitarFoto.Bottom) + 24;
            bool botonesEnLinea = ancho >= Escalar(400);
            int anchoGuardar = botonesEnLinea
                ? Math.Min(Escalar(180), Math.Max(Escalar(120), (ancho - separacion) / 2))
                : Math.Min(Escalar(180), ancho);
            int anchoContrasena = botonesEnLinea
                ? Math.Min(Escalar(190), ancho - anchoGuardar - separacion)
                : Math.Min(Escalar(190), ancho);

            guardar.SetBounds(x, yBotones, anchoGuardar, guardar.Height);
            contrasena.SetBounds(
                botonesEnLinea ? x + anchoGuardar + separacion : x,
                botonesEnLinea ? yBotones : guardar.Bottom + 10,
                anchoContrasena,
                contrasena.Height);

            return contrasena.Bottom - AutoScrollPosition.Y + Margen;
        }

        private CheckBox CrearAlerta(string texto, int y, GroupBox grupo)
        {
            CheckBox alerta = new CheckBox();
            alerta.Text = texto;
            alerta.Location = new Point(16, y);
            alerta.AutoSize = true;
            alerta.ForeColor = TemaActual.Actual.Texto;
            alerta.BackColor = Color.Transparent;
            grupo.Controls.Add(alerta);
            return alerta;
        }

        private void CargarDatos()
        {
            campoNombre.Text = usuario.NombreCompleto;
            campoColor.Text = usuario.ColorAcento;
            campoFuente.Value = usuario.TamanoFuente;
            alertaDeudas.Checked = usuario.AlertasDeudas;
            alertaMetas.Checked = usuario.AlertasMetas;
            alertaPresupuesto.Checked = usuario.AlertasPresupuesto;
            alertaSobres.Checked = usuario.AlertasSobres;

            MostrarFoto(usuario.Foto);
        }

        private void AlCambiarColor(object remitente, EventArgs argumentos)
        {
            muestraColor.BackColor = UtilidadesVista.ColorDesdeTexto(campoColor.Text);
        }

        private void AlCargarFoto(object remitente, EventArgs argumentos)
        {
            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Seleccionar foto";
            dialogo.Filter = "Imagenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                byte[] datos = File.ReadAllBytes(dialogo.FileName);

                if (datos.Length > 2097152)
                {
                    UtilidadesVista.AvisarError("La foto no puede superar 2 MB.");
                    return;
                }

                fotoSeleccionada = datos;
                fotoCambiada = true;
                MostrarFoto(datos);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlQuitarFoto(object remitente, EventArgs argumentos)
        {
            fotoSeleccionada = null;
            fotoCambiada = true;

            if (foto.Image != null)
            {
                foto.Image.Dispose();
                foto.Image = null;
            }
        }

        private void MostrarFoto(byte[] datos)
        {
            if (foto.Image != null)
            {
                foto.Image.Dispose();
                foto.Image = null;
            }

            if (datos == null || datos.Length == 0)
            {
                return;
            }

            using (MemoryStream memoria = new MemoryStream(datos))
            {
                using (Image original = Image.FromStream(memoria))
                {
                    foto.Image = new Bitmap(original);
                }
            }
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Resultado resultado = Servicios.Perfil.GuardarPerfil(
                    campoNombre.Text,
                    campoColor.Text.Trim(),
                    Convert.ToInt32(campoFuente.Value),
                    alertaDeudas.Checked,
                    alertaMetas.Checked,
                    alertaPresupuesto.Checked,
                    alertaSobres.Checked);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                if (fotoCambiada)
                {
                    Resultado resultadoFoto = Servicios.Perfil.GuardarFoto(fotoSeleccionada);

                    if (!resultadoFoto.Exito)
                    {
                        UtilidadesVista.AvisarError(resultadoFoto.Mensaje);
                        return;
                    }
                }

                usuario = Servicios.Perfil.ObtenerActual();
                MostrarResultado(resultado);
                AvisarCambio();
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlCambiarContrasena(object remitente, EventArgs argumentos)
        {
            using (FormContrasena formulario = new FormContrasena(Servicios))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AvisarCambio()
        {
            if (PerfilCambiado != null)
            {
                PerfilCambiado(this, EventArgs.Empty);
            }
        }

        private string NombreRol()
        {
            if (usuario.Rol == RolUsuario.Administrador)
            {
                return "Administrador";
            }

            return "Usuario";
        }
    }
}
