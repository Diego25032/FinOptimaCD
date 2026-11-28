using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Modulos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormPrincipalAdministrador : Form
    {
        private FabricaServicios servicios;
        private ServicioBotLocal botLocal;
        private Panel cabecera;
        private Panel panelContenido;
        private Panel panelMarca;
        private FlowLayoutPanel panelAcciones;
        private Label tituloMarca;
        private Label notaMarca;
        private FlowLayoutPanel navegacion;
        private Label etiquetaUsuario;
        private ComboBox selectorTema;
        private bool cerroSesion;
        private bool cargandoTema;
        private string claveModuloActual;

        public bool CerroSesion
        {
            get { return cerroSesion; }
        }

        public FormPrincipalAdministrador(FabricaServicios fabrica, ServicioBotLocal servicioBot)
        {
            servicios = fabrica;
            botLocal = servicioBot;
            cerroSesion = false;

            if (!servicios.Sesion.HaySesion)
            {
                throw new InvalidOperationException("No hay una sesion activa.");
            }

            if (!servicios.Sesion.EsAdministrador)
            {
                throw new InvalidOperationException("Esta ventana es solo para administradores.");
            }

            claveModuloActual = "administracion";
            cargandoTema = false;
            AplicarTemaActual();
            ConstruirInterfaz();
            MostrarModuloPorClave(claveModuloActual);
        }

        private void ConstruirInterfaz()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            Text = "FinOptima - Administracion - " + servicios.Sesion.Usuario.NombreCompleto;
            UtilidadesVista.AplicarIcono(this);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Size maximo = UtilidadesVista.TamanoMaximoVentana(this);
            MinimumSize = new Size(Math.Min(1240, maximo.Width), Math.Min(720, maximo.Height));
            BackColor = tema.Fondo;
            Font = new Font("Segoe UI", UtilidadesVista.TamanoFuenteSeguro(servicios.Sesion.Usuario.TamanoFuente));

            cabecera = new Panel();
            cabecera.Dock = DockStyle.Top;
            cabecera.Height = 76;
            cabecera.BackColor = tema.Cabecera;
            cabecera.Padding = new Padding(0, 0, 0, 1);
            cabecera.Paint += AlPintarCabecera;

            panelContenido = new Panel();
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.BackColor = tema.Fondo;

            navegacion = new FlowLayoutPanel();
            navegacion.Dock = DockStyle.Fill;
            navegacion.BackColor = tema.Cabecera;
            navegacion.FlowDirection = FlowDirection.LeftToRight;
            navegacion.WrapContents = false;
            navegacion.AutoScroll = true;
            navegacion.Padding = new Padding(2, 19, 0, 0);

            panelMarca = CrearMarcaVisual(tema, acento);
            panelAcciones = CrearAccionesUsuario(tema);

            AgregarBotonNav("Administracion", "administracion");
            AgregarBotonNav("Mi perfil", "perfil");
            AgregarBotonNav("Telegram", "telegram");

            cabecera.Controls.Add(navegacion);
            cabecera.Controls.Add(panelMarca);
            cabecera.Controls.Add(panelAcciones);
            cabecera.ClientSizeChanged += AjustarCabecera;

            Controls.Add(panelContenido);
            Controls.Add(cabecera);
            AjustarCabecera(cabecera, EventArgs.Empty);
        }

        private Panel CrearMarcaVisual(Tema tema, Color acento)
        {
            Color acentoVisible = tema.AcentoVisible(acento);

            Panel marca = new Panel();
            marca.Dock = DockStyle.Left;
            marca.Width = 216;
            marca.Tag = 216;
            marca.BackColor = tema.Cabecera;

            LogoFinOptima logo = new LogoFinOptima();
            logo.Location = new Point(18, 18);
            logo.Size = new Size(38, 38);
            logo.BackColor = tema.Cabecera;
            logo.ColorAcento = acentoVisible;
            marca.Controls.Add(logo);

            tituloMarca = new Label();
            tituloMarca.Text = "FinOptima";
            tituloMarca.AutoSize = true;
            tituloMarca.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            tituloMarca.ForeColor = tema.CabeceraTexto;
            tituloMarca.BackColor = Color.Transparent;
            tituloMarca.Location = new Point(66, 14);
            marca.Controls.Add(tituloMarca);

            notaMarca = new Label();
            notaMarca.Text = "Administracion de cuentas";
            notaMarca.AutoSize = true;
            notaMarca.Font = new Font("Segoe UI", 7.5f);
            notaMarca.ForeColor = tema.CabeceraSuave;
            notaMarca.BackColor = Color.Transparent;
            notaMarca.Location = new Point(68, tituloMarca.Bottom + 2);
            marca.Controls.Add(notaMarca);

            return marca;
        }

        private FlowLayoutPanel CrearAccionesUsuario(Tema tema)
        {
            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Right;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = true;
            acciones.AutoScroll = true;
            acciones.BackColor = tema.Cabecera;
            acciones.Padding = new Padding(0, 19, 12, 0);

            BotonContorno salir = new BotonContorno();
            salir.Name = "Salir";
            salir.Text = "Salir";
            salir.Size = new Size(78, 38);
            salir.MinimumSize = new Size(0, 0);
            salir.BackColor = tema.Cabecera;
            salir.ForeColor = tema.Peligro;
            salir.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            salir.ColorBorde = Tema.Mezclar(tema.Cabecera, tema.Peligro, 70);
            salir.FlatAppearance.MouseOverBackColor = Tema.Mezclar(tema.Cabecera, tema.Peligro, 18);
            salir.FlatAppearance.MouseDownBackColor = Tema.Mezclar(tema.Cabecera, tema.Peligro, 30);
            salir.Margin = new Padding(8, 0, 0, 0);
            salir.Click += AlCerrarSesion;

            selectorTema = UtilidadesVista.CrearLista(0, 0, 184);
            selectorTema.Name = "SelectorTema";
            selectorTema.Font = new Font("Segoe UI", 9);
            selectorTema.DisplayMember = "Etiqueta";
            selectorTema.Margin = new Padding(8, 4, 0, 0);

            cargandoTema = true;

            foreach (Tema disponible in CatalogoTemas.Listar())
            {
                selectorTema.Items.Add(disponible);
            }

            selectorTema.SelectedIndex = IndiceDelTema(servicios.Sesion.Usuario.Tema);
            cargandoTema = false;
            selectorTema.SelectedIndexChanged += AlCambiarTema;

            etiquetaUsuario = new Label();
            etiquetaUsuario.Name = "Usuario";
            etiquetaUsuario.Text = servicios.Sesion.Usuario.NombreCompleto;
            etiquetaUsuario.ForeColor = tema.CabeceraTexto;
            etiquetaUsuario.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            etiquetaUsuario.AutoSize = false;
            etiquetaUsuario.AutoEllipsis = true;
            etiquetaUsuario.TextAlign = ContentAlignment.MiddleRight;
            etiquetaUsuario.Width = AnchoDelNombre(etiquetaUsuario.Text);
            etiquetaUsuario.Height = 38;
            etiquetaUsuario.Margin = new Padding(14, 0, 0, 0);

            acciones.Controls.Add(selectorTema);
            acciones.Controls.Add(etiquetaUsuario);
            acciones.Controls.Add(salir);

            int ancho = 12 + 78 + 8 + 184 + 8 + etiquetaUsuario.Width + 12;
            acciones.Width = ancho;
            return acciones;
        }

        private void AjustarCabecera(object remitente, EventArgs argumentos)
        {
            if (panelMarca == null || panelAcciones == null || etiquetaUsuario == null)
            {
                return;
            }

            etiquetaUsuario.Visible = true;
            tituloMarca.Visible = true;
            notaMarca.Visible = true;
            panelMarca.Width = EscalarCabecera(216);
            ActualizarAnchoAcciones();
            int anchoAcciones = panelAcciones.Width;
            int minimoNavegacion = EscalarCabecera(70);

            if (cabecera.ClientSize.Width < panelMarca.Width + anchoAcciones + minimoNavegacion)
            {
                tituloMarca.Visible = false;
                notaMarca.Visible = false;
                panelMarca.Width = EscalarCabecera(64);
            }

            if (cabecera.ClientSize.Width >= panelMarca.Width + anchoAcciones + minimoNavegacion)
            {
                panelAcciones.WrapContents = false;
                panelAcciones.AutoScroll = false;
                panelAcciones.Width = anchoAcciones;
                cabecera.Height = EscalarCabecera(76);
            }
            else
            {
                panelAcciones.WrapContents = true;
                panelAcciones.AutoScroll = true;
                int anchoDisponible = Math.Max(
                    EscalarCabecera(280),
                    cabecera.ClientSize.Width - panelMarca.Width - minimoNavegacion);
                panelAcciones.Width = Math.Min(anchoAcciones, anchoDisponible);
                cabecera.Height = EscalarCabecera(118);
            }

            navegacion.Padding = new Padding(
                2,
                Math.Max(0, (cabecera.ClientSize.Height - EscalarCabecera(38)) / 2),
                0,
                0);
        }

        private void ActualizarAnchoAcciones()
        {
            int ancho = panelAcciones.Padding.Horizontal;

            foreach (Control control in panelAcciones.Controls)
            {
                if (control.Visible)
                {
                    ancho = ancho + control.Width + control.Margin.Horizontal;
                }
            }

            panelAcciones.Width = ancho;
        }

        private int EscalarCabecera(int valor)
        {
            return (int)Math.Round(valor * DeviceDpi / 96f);
        }

        private Button AgregarBotonNav(string texto, string clave)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Tag = clave;
            boton.AutoSize = false;
            boton.Height = 38;
            boton.Width = AnchoBotonNav(texto);
            boton.Margin = new Padding(3, 0, 3, 0);
            UtilidadesVista.EstilizarBotonNav(boton, EsActivo(clave), TemaActual.Acento);
            boton.Click += AlElegirModulo;

            navegacion.Controls.Add(boton);
            return boton;
        }

        private void AlElegirModulo(object remitente, EventArgs argumentos)
        {
            Button boton = (Button)remitente;

            if ((string)boton.Tag == "telegram")
            {
                AbrirTelegram();
                return;
            }

            claveModuloActual = (string)boton.Tag;
            ActualizarNavegacion();
            MostrarModuloPorClave(claveModuloActual);
        }

        private void AbrirTelegram()
        {
            if (botLocal == null)
            {
                UtilidadesVista.AvisarError("El bot local no esta disponible.");
                return;
            }

            using (FormTelegram telegram = new FormTelegram(botLocal))
            {
                telegram.ShowDialog(this);
            }
        }

        private void ActualizarNavegacion()
        {
            foreach (Control control in navegacion.Controls)
            {
                Button boton = control as Button;

                if (boton == null)
                {
                    continue;
                }

                UtilidadesVista.EstilizarBotonNav(boton, EsActivo((string)boton.Tag), TemaActual.Acento);
            }
        }

        private bool EsActivo(string clave)
        {
            return clave == claveModuloActual;
        }

        private void MostrarModuloPorClave(string clave)
        {
            try
            {
                if (clave == "administracion")
                {
                    MostrarFormulario(new FormAdministracion(servicios));
                }
                else if (clave == "perfil")
                {
                    ModuloPerfil perfil = new ModuloPerfil(servicios);
                    perfil.PerfilCambiado += AlCambiarPerfil;
                    MostrarModulo(perfil);
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void MostrarFormulario(Form formulario)
        {
            panelContenido.SuspendLayout();
            LimpiarContenido();
            formulario.MinimumSize = Size.Empty;
            formulario.TopLevel = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(formulario);
            formulario.Show();
            panelContenido.ResumeLayout();
        }

        private void MostrarModulo(UserControl modulo)
        {
            panelContenido.SuspendLayout();
            LimpiarContenido();
            modulo.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(modulo);
            panelContenido.ResumeLayout();
        }

        private void LimpiarContenido()
        {
            while (panelContenido.Controls.Count > 0)
            {
                Control control = panelContenido.Controls[0];
                panelContenido.Controls.RemoveAt(0);
                control.Dispose();
            }
        }

        private void AlCambiarPerfil(object remitente, EventArgs argumentos)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(ReconstruirInterfaz));
            }
        }

        private void AlCambiarTema(object remitente, EventArgs argumentos)
        {
            if (cargandoTema)
            {
                return;
            }

            Tema seleccionado = selectorTema.SelectedItem as Tema;

            if (seleccionado == null)
            {
                return;
            }

            try
            {
                Resultado resultado = servicios.Perfil.GuardarTema(seleccionado.Visual);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    RestablecerSelector();
                    return;
                }

                TemaActual.Cambiar(seleccionado);
                TemaActual.Acento = UtilidadesVista.AcentoDeUsuario(servicios.Sesion.Usuario.ColorAcento);

                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(ReconstruirInterfaz));
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
                RestablecerSelector();
            }
        }

        private void RestablecerSelector()
        {
            if (selectorTema == null)
            {
                return;
            }

            cargandoTema = true;
            selectorTema.SelectedIndex = IndiceDelTema(servicios.Sesion.Usuario.Tema);
            cargandoTema = false;
        }

        private int IndiceDelTema(TemaVisual visual)
        {
            for (int i = 0; i < selectorTema.Items.Count; i++)
            {
                Tema tema = (Tema)selectorTema.Items[i];

                if (tema.Visual == visual)
                {
                    return i;
                }
            }

            return 0;
        }

        private void AplicarTemaActual()
        {
            TemaActual.Cambiar(servicios.Sesion.Usuario.Tema);
            TemaActual.Acento = UtilidadesVista.AcentoDeUsuario(servicios.Sesion.Usuario.ColorAcento);
        }

        private void ReconstruirInterfaz()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();
            LimpiarControles();
            AplicarTemaActual();
            ConstruirInterfaz();
            MostrarModuloPorClave(claveModuloActual);
            ResumeLayout(true);
        }

        private void LimpiarControles()
        {
            while (Controls.Count > 0)
            {
                Control control = Controls[0];
                Controls.RemoveAt(0);
                control.Dispose();
            }
        }

        private int AnchoDelNombre(string texto)
        {
            int ancho;

            using (Font fuente = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                ancho = TextRenderer.MeasureText(texto, fuente).Width + 8;
            }

            if (ancho < 120)
            {
                ancho = 120;
            }

            if (ancho > 200)
            {
                ancho = 200;
            }

            return ancho;
        }

        private int AnchoBotonNav(string texto)
        {
            int ancho;

            using (Font fuente = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                ancho = TextRenderer.MeasureText(texto, fuente).Width + 26;
            }

            if (ancho < 90)
            {
                ancho = 90;
            }

            return ancho;
        }

        private void AlPintarCabecera(object remitente, PaintEventArgs argumentos)
        {
            Tema tema = TemaActual.Actual;

            using (SolidBrush pincel = new SolidBrush(tema.Borde))
            {
                argumentos.Graphics.FillRectangle(pincel, 0, cabecera.ClientSize.Height - 1, cabecera.ClientSize.Width, 1);
            }
        }

        private void AlCerrarSesion(object remitente, EventArgs argumentos)
        {
            if (!UtilidadesVista.Confirmar("Deseas cerrar la sesion?"))
            {
                return;
            }

            cerroSesion = true;
            servicios.Autenticacion.CerrarSesion();
            Close();
        }
    }
}
