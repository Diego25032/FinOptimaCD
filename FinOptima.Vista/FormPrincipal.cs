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
    public class FormPrincipal : Form
    {
        private FabricaServicios servicios;
        private Panel panelContenido;
        private Panel cabecera;
        private FlowLayoutPanel navegacion;
        private Label etiquetaUsuario;
        private ComboBox selectorTema;
        private Button botonMas;
        private Button botonRegistrar;
        private Panel barraMas;
        private FlowLayoutPanel botonesMas;
        private bool masAbierto;
        private NotifyIcon iconoNotificacion;
        private bool cerroSesion;
        private bool cargandoTema;
        private string claveModuloActual;

        public bool CerroSesion
        {
            get { return cerroSesion; }
        }

        public FormPrincipal(FabricaServicios fabrica)
        {
            servicios = fabrica;
            cerroSesion = false;

            if (!servicios.Sesion.HaySesion)
            {
                throw new InvalidOperationException("No hay una sesion activa.");
            }

            if (servicios.Sesion.EsAdministrador)
            {
                throw new InvalidOperationException("Los administradores usan el panel de administracion.");
            }

            claveModuloActual = "inicio";
            masAbierto = false;
            cargandoTema = false;
            AplicarTemaActual();
            ConstruirInterfaz();
            MostrarModuloPorClave(claveModuloActual);
            PrepararNotificaciones();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            Text = "FinOptima - " + servicios.Sesion.Usuario.NombreCompleto;
            UtilidadesVista.AplicarIcono(this);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1240, 720);
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
            navegacion.Padding = new Padding(2, 19, 0, 0);

            Panel marca = CrearMarcaVisual(tema, acento);
            Panel acciones = CrearAccionesUsuario(tema, acento);

            AgregarBotonNav("Panel", "inicio");
            AgregarBotonNav("Sobres", "sobres");
            AgregarBotonNav("Registros", "movimientos");
            AgregarBotonNav("Metas", "metas");
            AgregarBotonNav("Deudas", "deudas");
            botonMas = AgregarBotonNav(TextoBotonMas(), "mas");

            barraMas = CrearBarraMas(tema, acento);

            cabecera.Controls.Add(navegacion);
            cabecera.Controls.Add(marca);
            cabecera.Controls.Add(acciones);

            // El orden importa: WinForms acomoda primero el ultimo control agregado.
            // Asi la cabecera queda arriba, la barra de "Mas" justo debajo y el contenido al final.
            Controls.Add(panelContenido);
            Controls.Add(barraMas);
            Controls.Add(cabecera);
        }

        private Panel CrearMarcaVisual(Tema tema, Color acento)
        {
            Color acentoVisible = tema.AcentoVisible(acento);

            Panel marca = new Panel();
            marca.Dock = DockStyle.Left;
            marca.Width = 216;
            marca.BackColor = tema.Cabecera;

            LogoFinOptima logo = new LogoFinOptima();
            logo.Location = new Point(18, 18);
            logo.Size = new Size(38, 38);
            logo.BackColor = tema.Cabecera;
            logo.ColorAcento = acentoVisible;
            marca.Controls.Add(logo);

            Label titulo = new Label();
            titulo.Text = "FinOptima";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            titulo.ForeColor = tema.CabeceraTexto;
            titulo.BackColor = Color.Transparent;
            titulo.Location = new Point(66, 14);
            marca.Controls.Add(titulo);

            Label nota = new Label();
            nota.Text = "Metodo de los 7 sobres";
            nota.AutoSize = true;
            nota.Font = new Font("Segoe UI", 7.5f);
            nota.ForeColor = tema.CabeceraSuave;
            nota.BackColor = Color.Transparent;
            nota.Location = new Point(68, 42);
            marca.Controls.Add(nota);

            return marca;
        }

        private Panel CrearAccionesUsuario(Tema tema, Color acento)
        {
            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Right;
            acciones.FlowDirection = FlowDirection.RightToLeft;
            acciones.WrapContents = false;
            acciones.BackColor = tema.Cabecera;
            acciones.Padding = new Padding(0, 19, 12, 0);

            BotonContorno salir = new BotonContorno();
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

            selectorTema = UtilidadesVista.CrearLista(0, 0, 164);
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
            etiquetaUsuario.Text = servicios.Sesion.Usuario.NombreCompleto;
            etiquetaUsuario.ForeColor = tema.CabeceraTexto;
            etiquetaUsuario.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            etiquetaUsuario.AutoSize = false;
            etiquetaUsuario.AutoEllipsis = true;
            etiquetaUsuario.TextAlign = ContentAlignment.MiddleRight;
            etiquetaUsuario.Width = AnchoNombreCabecera(etiquetaUsuario.Text);
            etiquetaUsuario.Height = 38;
            etiquetaUsuario.Margin = new Padding(14, 0, 0, 0);

            botonRegistrar = UtilidadesVista.CrearBoton("+  Registrar", 0, 0, 124, 38, acento);
            botonRegistrar.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            botonRegistrar.Margin = new Padding(14, 0, 0, 0);
            botonRegistrar.Click += AlRegistrarMovimiento;

            acciones.Controls.Add(botonRegistrar);
            acciones.Controls.Add(etiquetaUsuario);
            acciones.Controls.Add(selectorTema);
            acciones.Controls.Add(salir);

            int ancho = 12 + 78 + 8 + 164 + 8 + etiquetaUsuario.Width + 14 + 124 + 14;
            acciones.Width = ancho;
            return acciones;
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

            if (clave == "mas")
            {
                boton.Click += AlAbrirMenuMas;
            }
            else
            {
                boton.Click += AlElegirModulo;
            }

            navegacion.Controls.Add(boton);
            return boton;
        }

        /// <summary>
        /// Barra que se despliega debajo de la navegacion al tocar "Mas". Empuja el contenido
        /// hacia abajo en lugar de abrir un menu flotante aparte, y queda abierta mientras se
        /// usa uno de sus modulos.
        /// </summary>
        private Panel CrearBarraMas(Tema tema, Color acento)
        {
            Panel barra = new Panel();
            barra.Dock = DockStyle.Top;
            barra.Height = 54;
            barra.BackColor = ColorBarraMas(tema);
            barra.Padding = new Padding(216 + 2, 0, 12, 0);
            barra.Paint += AlPintarBarraMas;

            botonesMas = new FlowLayoutPanel();
            botonesMas.Dock = DockStyle.Fill;
            botonesMas.FlowDirection = FlowDirection.LeftToRight;
            botonesMas.WrapContents = false;
            botonesMas.AutoScroll = false;
            botonesMas.BackColor = barra.BackColor;
            botonesMas.Padding = new Padding(0, 9, 0, 0);
            barra.Controls.Add(botonesMas);

            AgregarBotonMas("Análisis", "analisis");
            AgregarBotonMas("Simuladores", "simuladores");
            AgregarBotonMas("Presupuestos", "presupuestos");
            AgregarBotonMas("Alertas", "alertas");
            AgregarBotonMas("Reportes", "reportes");
            AgregarBotonMas("Respaldos", "respaldos");
            AgregarBotonMas("Mi perfil", "perfil");
            AgregarBotonMas("Actividad", "actividad");

            barra.Visible = BarraMasVisible();
            return barra;
        }

        private void AgregarBotonMas(string texto, string clave)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Tag = clave;
            boton.AutoSize = false;
            boton.Height = 34;
            boton.Width = AnchoBotonNav(texto) - 4;
            boton.Margin = new Padding(3, 0, 3, 0);
            UtilidadesVista.EstilizarBotonSubNav(boton, clave == claveModuloActual, TemaActual.Acento, ColorBarraMas(TemaActual.Actual));
            boton.Click += AlElegirModuloMas;
            botonesMas.Controls.Add(boton);
        }

        private static Color ColorBarraMas(Tema tema)
        {
            return Tema.Mezclar(tema.Cabecera, tema.Fondo, 55);
        }

        private bool BarraMasVisible()
        {
            return masAbierto || !EsModuloPrincipal(claveModuloActual);
        }

        private string TextoBotonMas()
        {
            return BarraMasVisible() ? "Más  ▴" : "Más  ▾";
        }

        private void AlAbrirMenuMas(object remitente, EventArgs argumentos)
        {
            if (barraMas == null)
            {
                return;
            }

            // Si se esta viendo un modulo de "Mas", la barra se mantiene para no perder la ubicacion.
            if (!EsModuloPrincipal(claveModuloActual))
            {
                masAbierto = true;
            }
            else
            {
                masAbierto = !masAbierto;
            }

            ActualizarNavegacion();
        }

        private void AlElegirModuloMas(object remitente, EventArgs argumentos)
        {
            Button boton = remitente as Button;

            if (boton == null)
            {
                return;
            }

            claveModuloActual = (string)boton.Tag;
            masAbierto = true;
            ActualizarNavegacion();
            MostrarModuloPorClave(claveModuloActual);
        }

        private void AlPintarBarraMas(object remitente, PaintEventArgs argumentos)
        {
            Tema tema = TemaActual.Actual;
            Control barra = (Control)remitente;

            using (SolidBrush pincel = new SolidBrush(tema.Borde))
            {
                argumentos.Graphics.FillRectangle(pincel, 0, barra.ClientSize.Height - 1, barra.ClientSize.Width, 1);
            }
        }

        private void AlElegirModulo(object remitente, EventArgs argumentos)
        {
            Button boton = (Button)remitente;
            claveModuloActual = (string)boton.Tag;
            masAbierto = false;
            ActualizarNavegacion();
            MostrarModuloPorClave(claveModuloActual);
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

            if (botonMas != null)
            {
                botonMas.Text = TextoBotonMas();
                botonMas.Width = AnchoBotonNav(botonMas.Text);
            }

            if (botonesMas != null)
            {
                foreach (Control control in botonesMas.Controls)
                {
                    Button boton = control as Button;

                    if (boton != null)
                    {
                        UtilidadesVista.EstilizarBotonSubNav(boton, (string)boton.Tag == claveModuloActual, TemaActual.Acento, ColorBarraMas(TemaActual.Actual));
                    }
                }
            }

            if (barraMas != null)
            {
                barraMas.Visible = BarraMasVisible();
            }
        }

        private bool EsActivo(string clave)
        {
            if (clave == "mas")
            {
                return !EsModuloPrincipal(claveModuloActual);
            }

            return clave == claveModuloActual;
        }

        private bool EsModuloPrincipal(string clave)
        {
            string[] principales = new string[] { "inicio", "sobres", "movimientos", "metas", "deudas" };

            foreach (string principal in principales)
            {
                if (principal == clave)
                {
                    return true;
                }
            }

            return false;
        }

        private void MostrarModuloPorClave(string clave)
        {
            try
            {
                if (clave == "inicio")
                {
                    MostrarModulo(new ModuloDashboard(servicios));
                }
                else if (clave == "sobres")
                {
                    MostrarModulo(new ModuloSobres(servicios));
                }
                else if (clave == "movimientos")
                {
                    MostrarModulo(new ModuloMovimientos(servicios));
                }
                else if (clave == "metas")
                {
                    MostrarModulo(new ModuloMetas(servicios));
                }
                else if (clave == "deudas")
                {
                    MostrarModulo(new ModuloDeudas(servicios));
                }
                else if (clave == "analisis")
                {
                    MostrarModulo(new ModuloAnalisis(servicios));
                }
                else if (clave == "simuladores")
                {
                    MostrarModulo(new ModuloSimuladores(servicios));
                }
                else if (clave == "presupuestos")
                {
                    MostrarModulo(new ModuloPresupuestos(servicios));
                }
                else if (clave == "alertas")
                {
                    MostrarModulo(new ModuloAlertas(servicios));
                }
                else if (clave == "reportes")
                {
                    MostrarModulo(new ModuloReportes(servicios));
                }
                else if (clave == "respaldos")
                {
                    MostrarModulo(new ModuloRespaldos(servicios));
                }
                else if (clave == "perfil")
                {
                    ModuloPerfil perfil = new ModuloPerfil(servicios);
                    perfil.PerfilCambiado += AlCambiarPerfil;
                    MostrarModulo(perfil);
                }
                else if (clave == "actividad")
                {
                    MostrarModulo(new ModuloActividad(servicios));
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void MostrarModulo(UserControl modulo)
        {
            panelContenido.SuspendLayout();
            panelContenido.Controls.Clear();
            modulo.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(modulo);
            panelContenido.ResumeLayout();
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

        private int AnchoBotonNav(string texto)
        {
            int ancho;

            using (Font fuente = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                ancho = TextRenderer.MeasureText(texto, fuente).Width + 26;
            }

            if (ancho < 70)
            {
                ancho = 70;
            }

            return ancho;
        }

        private int AnchoNombreCabecera(string texto)
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

            if (ancho > 180)
            {
                ancho = 180;
            }

            return ancho;
        }

        private void AlRegistrarMovimiento(object remitente, EventArgs argumentos)
        {
            try
            {
                using (FormMovimiento formulario = new FormMovimiento(servicios, null))
                {
                    if (formulario.ShowDialog(this) == DialogResult.OK)
                    {
                        claveModuloActual = "movimientos";
                        masAbierto = false;
                        ActualizarNavegacion();
                        MostrarModuloPorClave(claveModuloActual);
                    }
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
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

        private void PrepararNotificaciones()
        {
            iconoNotificacion = new NotifyIcon();
            Icon iconoBandeja = UtilidadesVista.IconoAplicacion();

            if (iconoBandeja == null)
            {
                iconoBandeja = SystemIcons.Information;
            }

            iconoNotificacion.Icon = iconoBandeja;
            iconoNotificacion.Text = "FinOptima";
            iconoNotificacion.Visible = true;

            try
            {
                ResumenDashboard resumen = servicios.Dashboard.Obtener();

                if (resumen.Alertas.Count > 0)
                {
                    Alerta primera = resumen.Alertas[0];
                    string mensaje = resumen.Alertas.Count + " alerta(s). " + primera.Titulo + ": " + primera.Mensaje;
                    iconoNotificacion.ShowBalloonTip(5000, "FinOptima", mensaje, ToolTipIcon.Warning);
                }
            }
            catch
            {
                iconoNotificacion.Visible = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (iconoNotificacion != null)
            {
                iconoNotificacion.Visible = false;
                iconoNotificacion.Dispose();
            }

            base.OnFormClosed(e);
        }
    }
}
