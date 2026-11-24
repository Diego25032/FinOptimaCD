using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Consultas;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloMovimientos : ModuloBase
    {
        private ComboBox campoMes;
        private NumericUpDown campoAnio;
        private ComboBox campoTipo;
        private TextBox campoCategoria;
        private TextBox campoTexto;
        private DataGridView grilla;
        private Label etiquetaResumen;
        private TarjetaPanel tarjetaFiltros;
        private TarjetaPanel tarjetaRegistros;
        private Button botonFiltrar;
        private Button botonLimpiar;
        private Button botonNuevo;
        private Button botonEditar;
        private Button botonEliminar;
        private Button botonActualizar;

        public ModuloMovimientos(FabricaServicios servicios) : base(servicios, "Registros")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            tarjetaFiltros = new TarjetaPanel();
            tarjetaFiltros.ColorFondo = tema.Superficie;

            Label tituloFiltros = new Label();
            tituloFiltros.Text = "Mi filtro";
            tituloFiltros.Location = new Point(18, 18);
            tituloFiltros.AutoSize = true;
            tituloFiltros.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            tituloFiltros.ForeColor = tema.Texto;
            tituloFiltros.BackColor = Color.Transparent;
            tarjetaFiltros.Controls.Add(tituloFiltros);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearEtiquetaSuave("Buscar", 18, 58));
            campoTexto = UtilidadesVista.CrearCampo(18, 78, 244);
            tarjetaFiltros.Controls.Add(campoTexto);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearEtiquetaSuave("Mes", 18, 116));
            campoMes = UtilidadesVista.CrearLista(18, 136, 244);
            campoMes.Items.Add("Todos");

            for (int numero = 1; numero <= 12; numero = numero + 1)
            {
                campoMes.Items.Add(CalendarioFinanciero.NombreMes(numero));
            }

            campoMes.SelectedIndex = 0;
            tarjetaFiltros.Controls.Add(campoMes);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearEtiquetaSuave("Anio", 18, 174));
            campoAnio = new NumericUpDown();
            campoAnio.Location = new Point(18, 194);
            campoAnio.Width = 120;
            campoAnio.Minimum = 2000;
            campoAnio.Maximum = 2100;
            campoAnio.Value = DateTime.Today.Year;
            campoAnio.Enabled = false;
            UtilidadesVista.EstilizarCampo(campoAnio);
            tarjetaFiltros.Controls.Add(campoAnio);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearEtiquetaSuave("Tipo", 18, 232));
            campoTipo = UtilidadesVista.CrearLista(18, 252, 244);
            campoTipo.Items.Add("Todos");
            campoTipo.Items.Add("Ingreso");
            campoTipo.Items.Add("Gasto");
            campoTipo.SelectedIndex = 0;
            tarjetaFiltros.Controls.Add(campoTipo);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearEtiquetaSuave("Categoria", 18, 290));
            campoCategoria = UtilidadesVista.CrearCampo(18, 310, 244);
            tarjetaFiltros.Controls.Add(campoCategoria);

            botonFiltrar = UtilidadesVista.CrearBoton("Aplicar filtro", 18, 362, 244, 36, Acento);
            botonFiltrar.Click += AlFiltrar;
            tarjetaFiltros.Controls.Add(botonFiltrar);

            botonLimpiar = UtilidadesVista.CrearBotonSecundario("Restablecer filtro", 18, 406, 244, 36);
            botonLimpiar.Click += AlLimpiar;
            tarjetaFiltros.Controls.Add(botonLimpiar);

            tarjetaFiltros.Controls.Add(UtilidadesVista.CrearNota("Los filtros se aplican al instante.", 18, 452, 244));
            Controls.Add(tarjetaFiltros);

            tarjetaRegistros = new TarjetaPanel();
            tarjetaRegistros.ColorFondo = tema.Superficie;

            Label tituloRegistros = new Label();
            tituloRegistros.Text = "Movimientos";
            tituloRegistros.Location = new Point(20, 16);
            tituloRegistros.AutoSize = true;
            tituloRegistros.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            tituloRegistros.ForeColor = tema.Texto;
            tituloRegistros.BackColor = Color.Transparent;
            tarjetaRegistros.Controls.Add(tituloRegistros);

            etiquetaResumen = new Label();
            etiquetaResumen.Text = "";
            etiquetaResumen.Location = new Point(20, 44);
            etiquetaResumen.AutoSize = false;
            etiquetaResumen.Height = 20;
            etiquetaResumen.AutoEllipsis = true;
            etiquetaResumen.Font = new Font("Segoe UI", 9);
            etiquetaResumen.ForeColor = tema.TextoSecundario;
            etiquetaResumen.BackColor = Color.Transparent;
            tarjetaRegistros.Controls.Add(etiquetaResumen);

            botonNuevo = UtilidadesVista.CrearBoton("+  Nuevo", 0, 0, 120, 32, Acento);
            botonNuevo.Click += AlNuevo;
            tarjetaRegistros.Controls.Add(botonNuevo);

            botonEditar = UtilidadesVista.CrearBotonSecundario("Editar", 0, 0, 100, 32);
            botonEditar.Click += AlEditar;
            tarjetaRegistros.Controls.Add(botonEditar);

            botonEliminar = UtilidadesVista.CrearBotonPeligro("Eliminar", 0, 0, 100, 32);
            botonEliminar.Click += AlEliminar;
            tarjetaRegistros.Controls.Add(botonEliminar);

            botonActualizar = UtilidadesVista.CrearBotonSecundario("Actualizar", 0, 0, 110, 32);
            botonActualizar.Click += AlActualizar;
            tarjetaRegistros.Controls.Add(botonActualizar);

            grilla = new GrillaDatos();
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.MinimumSize = new Size(0, 0);
            grilla.Columns.Add("Fecha", "Fecha");
            grilla.Columns.Add("Tipo", "Tipo");
            grilla.Columns.Add("Categoria", "Categoria");
            grilla.Columns.Add("Descripcion", "Descripcion");
            grilla.Columns.Add("Monto", "Monto");
            grilla.Columns.Add("Sobre", "Sobre");
            tarjetaRegistros.Controls.Add(grilla);
            Controls.Add(tarjetaRegistros);

            campoMes.SelectedIndexChanged += AlCambiarMes;
            campoAnio.ValueChanged += AlCambiarFiltro;
            campoTipo.SelectedIndexChanged += AlCambiarFiltro;
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (tarjetaFiltros == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int ancho = ClientSize.Width - Margen * 2;

                if (ancho < 1040)
                {
                    ancho = 1040;
                }

                int alto = ClientSize.Height - 58 - Margen;

                if (alto < 520)
                {
                    alto = 520;
                }

                int altoContenido = 58 + alto + 8;

                if (altoContenido > ClientSize.Height && !VerticalScroll.Visible)
                {
                    ancho = ancho - SystemInformation.VerticalScrollBarWidth;

                    if (ancho < 1040)
                    {
                        ancho = 1040;
                    }
                }

                tarjetaFiltros.SetBounds(Margen, 58, 280, alto);
                tarjetaRegistros.SetBounds(Margen + 296, 58, ancho - 296, alto);

                int anchoRegistros = tarjetaRegistros.Width;
                botonNuevo.Location = new Point(anchoRegistros - 20 - 120, 14);
                botonActualizar.Location = new Point(botonNuevo.Left - 8 - 110, 14);
                botonEliminar.Location = new Point(botonActualizar.Left - 8 - 100, 14);
                botonEditar.Location = new Point(botonEliminar.Left - 8 - 100, 14);

                int anchoResumen = botonEditar.Left - 40;

                if (anchoResumen < 180)
                {
                    anchoResumen = 180;
                }

                etiquetaResumen.Width = anchoResumen;
                grilla.SetBounds(16, 84, anchoRegistros - 32, alto - 100);

                AutoScrollMinSize = new Size(ancho + Margen * 2, altoContenido);
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private void CargarDatos()
        {
            try
            {
                List<Sobre> sobres = Servicios.Sobres.Listar();
                Dictionary<int, string> nombresSobre = new Dictionary<int, string>();

                foreach (Sobre sobre in sobres)
                {
                    nombresSobre[sobre.Id] = sobre.Nombre;
                }

                List<Movimiento> movimientos = Servicios.Movimientos.Listar(LeerFiltro());
                grilla.Rows.Clear();
                decimal ingresos = 0;
                decimal gastos = 0;

                foreach (Movimiento movimiento in movimientos)
                {
                    string nombreSobre = "Sin sobre";

                    if (movimiento.SobreId > 0 && nombresSobre.ContainsKey(movimiento.SobreId))
                    {
                        nombreSobre = nombresSobre[movimiento.SobreId];
                    }

                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        ingresos = ingresos + movimiento.Monto;
                    }
                    else
                    {
                        gastos = gastos + movimiento.Monto;
                    }

                    int indice = grilla.Rows.Add(
                        movimiento.Fecha.ToString("dd/MM/yyyy"),
                        UtilidadesVista.NombreTipoMovimiento((int)movimiento.Tipo),
                        movimiento.Categoria,
                        movimiento.Descripcion,
                        UtilidadesVista.Moneda(movimiento.Monto),
                        nombreSobre);

                    grilla.Rows[indice].Tag = movimiento;
                }

                etiquetaResumen.Text = movimientos.Count + " registros   ·   Ingresos " + UtilidadesVista.MonedaCorta(ingresos) + "   ·   Gastos " + UtilidadesVista.MonedaCorta(gastos);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private FiltroMovimientos LeerFiltro()
        {
            FiltroMovimientos filtro = new FiltroMovimientos();

            if (campoMes.SelectedIndex > 0)
            {
                filtro.Mes = campoMes.SelectedIndex;
                filtro.Anio = (int)campoAnio.Value;
            }

            if (campoTipo.SelectedIndex == 1)
            {
                filtro.Tipo = TipoMovimiento.Ingreso;
            }

            if (campoTipo.SelectedIndex == 2)
            {
                filtro.Tipo = TipoMovimiento.Gasto;
            }

            filtro.Categoria = campoCategoria.Text.Trim();
            filtro.Texto = campoTexto.Text.Trim();
            return filtro;
        }

        private void AlCambiarMes(object remitente, EventArgs argumentos)
        {
            campoAnio.Enabled = campoMes.SelectedIndex > 0;
            CargarDatos();
        }

        private void AlCambiarFiltro(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }

        private void AlFiltrar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }

        private void AlLimpiar(object remitente, EventArgs argumentos)
        {
            campoMes.SelectedIndex = 0;
            campoAnio.Value = DateTime.Today.Year;
            campoTipo.SelectedIndex = 0;
            campoCategoria.Text = "";
            campoTexto.Text = "";
            CargarDatos();
        }

        private void AlNuevo(object remitente, EventArgs argumentos)
        {
            using (FormMovimiento formulario = new FormMovimiento(Servicios, null))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            Movimiento movimiento = ObtenerSeleccionado();

            if (movimiento == null)
            {
                UtilidadesVista.AvisarError("Selecciona un movimiento.");
                return;
            }

            using (FormMovimiento formulario = new FormMovimiento(Servicios, movimiento))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEliminar(object remitente, EventArgs argumentos)
        {
            Movimiento movimiento = ObtenerSeleccionado();

            if (movimiento == null)
            {
                UtilidadesVista.AvisarError("Selecciona un movimiento.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar este movimiento? El saldo del sobre se corrige automaticamente."))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Movimientos.Eliminar(movimiento.Id));
                CargarDatos();
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlActualizar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }

        private Movimiento ObtenerSeleccionado()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (Movimiento)grilla.CurrentRow.Tag;
        }
    }
}
