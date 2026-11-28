using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloSimuladores : ModuloBase
    {
        private ComboBox campoMeta;
        private NumericUpDown campoAhorroMensual;
        private Label resultadoAhorro;
        private Label etiquetaMeta;
        private Label etiquetaAhorro;
        private ComboBox campoDeuda;
        private NumericUpDown campoPagoMensual;
        private Label resultadoDeuda;
        private Label etiquetaDeuda;
        private Label etiquetaPago;
        private Label nota;
        private GroupBox grupoAhorro;
        private GroupBox grupoDeuda;
        private Button simularAhorro;
        private Button simularDeuda;

        public ModuloSimuladores(FabricaServicios servicios) : base(servicios, "Simuladores")
        {
            ConstruirInterfaz();
            CargarMetas();
            CargarDeudas();
            Distribuir();
        }

        private void ConstruirInterfaz()
        {
            nota = UtilidadesVista.CrearNota(
                "Calcula cuanto tiempo necesitas para cumplir una meta o pagar una deuda.",
                Margen, InicioContenido, 880);
            Controls.Add(nota);

            grupoAhorro = new GroupBox();
            grupoAhorro.Text = "Simulacion de ahorro";
            grupoAhorro.Location = new Point(Margen, 90);
            grupoAhorro.Size = new Size(430, 330);
            grupoAhorro.Font = new Font("Segoe UI", Letra, FontStyle.Bold);
            grupoAhorro.ForeColor = TemaActual.Actual.Texto;
            grupoAhorro.BackColor = Color.Transparent;
            Controls.Add(grupoAhorro);

            etiquetaMeta = UtilidadesVista.CrearEtiqueta("Meta", 20, 30);
            grupoAhorro.Controls.Add(etiquetaMeta);

            campoMeta = UtilidadesVista.CrearLista(20, 56, 380);
            grupoAhorro.Controls.Add(campoMeta);

            etiquetaAhorro = UtilidadesVista.CrearEtiqueta("Ahorro mensual", 20, 98);
            grupoAhorro.Controls.Add(etiquetaAhorro);

            campoAhorroMensual = new NumericUpDown();
            campoAhorroMensual.Location = new Point(20, 124);
            campoAhorroMensual.Width = 180;
            campoAhorroMensual.DecimalPlaces = 2;
            campoAhorroMensual.Maximum = 1000000000;
            campoAhorroMensual.Increment = 10000;
            UtilidadesVista.EstilizarCampo(campoAhorroMensual);
            grupoAhorro.Controls.Add(campoAhorroMensual);

            simularAhorro = UtilidadesVista.CrearBoton("Simular", 220, 122, 180, 32, Acento);
            simularAhorro.Click += AlSimularAhorro;
            grupoAhorro.Controls.Add(simularAhorro);

            resultadoAhorro = new Label();
            resultadoAhorro.Location = new Point(20, 170);
            resultadoAhorro.Size = new Size(380, 145);
            resultadoAhorro.ForeColor = TemaActual.Actual.Texto;
            grupoAhorro.Controls.Add(resultadoAhorro);

            grupoDeuda = new GroupBox();
            grupoDeuda.Text = "Simulacion de pago de deuda";
            grupoDeuda.Location = new Point(Margen + 460, 90);
            grupoDeuda.Size = new Size(430, 330);
            grupoDeuda.Font = new Font("Segoe UI", Letra, FontStyle.Bold);
            grupoDeuda.ForeColor = TemaActual.Actual.Texto;
            grupoDeuda.BackColor = Color.Transparent;
            Controls.Add(grupoDeuda);

            etiquetaDeuda = UtilidadesVista.CrearEtiqueta("Deuda", 20, 30);
            grupoDeuda.Controls.Add(etiquetaDeuda);

            campoDeuda = UtilidadesVista.CrearLista(20, 56, 380);
            grupoDeuda.Controls.Add(campoDeuda);

            etiquetaPago = UtilidadesVista.CrearEtiqueta("Pago mensual", 20, 98);
            grupoDeuda.Controls.Add(etiquetaPago);

            campoPagoMensual = new NumericUpDown();
            campoPagoMensual.Location = new Point(20, 124);
            campoPagoMensual.Width = 180;
            campoPagoMensual.DecimalPlaces = 2;
            campoPagoMensual.Maximum = 1000000000;
            campoPagoMensual.Increment = 10000;
            UtilidadesVista.EstilizarCampo(campoPagoMensual);
            grupoDeuda.Controls.Add(campoPagoMensual);

            simularDeuda = UtilidadesVista.CrearBoton("Simular", 220, 122, 180, 32, Acento);
            simularDeuda.Click += AlSimularDeuda;
            grupoDeuda.Controls.Add(simularDeuda);

            resultadoDeuda = new Label();
            resultadoDeuda.Location = new Point(20, 170);
            resultadoDeuda.Size = new Size(380, 145);
            resultadoDeuda.ForeColor = TemaActual.Actual.Texto;
            grupoDeuda.Controls.Add(resultadoDeuda);
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (grupoAhorro == null || grupoDeuda == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int separacion = Escalar(20);
                int anchoMinimo = Escalar(400);
                int ancho = ClientSize.Width - Margen * 2;
                bool enColumnas = ancho >= anchoMinimo * 2 + separacion;
                int altoGrupo = Math.Min(Escalar(420), Math.Max(Escalar(320), ClientSize.Height - Escalar(120)));

                UtilidadesVista.AjustarEtiqueta(nota, ancho);
                int inicio = nota.Bottom + 14;
                int inicioLogico = inicio - AutoScrollPosition.Y;
                int filas = enColumnas ? 1 : 2;
                int altoContenido = inicioLogico + altoGrupo * filas + separacion * (filas - 1) + Margen;

                if (altoContenido > ClientSize.Height && !VerticalScroll.Visible)
                {
                    ancho = ClientSize.Width - Margen * 2 - SystemInformation.VerticalScrollBarWidth;
                    enColumnas = ancho >= anchoMinimo * 2 + separacion;
                    UtilidadesVista.AjustarEtiqueta(nota, ancho);
                    inicio = nota.Bottom + 14;
                    inicioLogico = inicio - AutoScrollPosition.Y;
                    filas = enColumnas ? 1 : 2;
                    altoContenido = inicioLogico + altoGrupo * filas + separacion * (filas - 1) + Margen;
                }

                if (ancho < anchoMinimo)
                {
                    ancho = anchoMinimo;
                }

                int anchoGrupo = enColumnas ? (ancho - separacion) / 2 : ancho;
                int x = Margen + AutoScrollPosition.X;

                grupoAhorro.SetBounds(x, inicio, anchoGrupo, altoGrupo);
                grupoDeuda.SetBounds(
                    enColumnas ? x + anchoGrupo + separacion : x,
                    enColumnas ? inicio : inicio + altoGrupo + separacion,
                    anchoGrupo,
                    altoGrupo);

                AjustarGrupo(grupoAhorro, campoMeta, campoAhorroMensual, simularAhorro, resultadoAhorro);
                AjustarGrupo(grupoDeuda, campoDeuda, campoPagoMensual, simularDeuda, resultadoDeuda);

                AutoScrollMinSize = new Size(ancho + Margen * 2, altoContenido);
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private static void AjustarGrupo(GroupBox grupo, ComboBox lista, NumericUpDown monto, Button boton, Label resultado)
        {
            int interior = grupo.Width - 40;
            int anchoMonto = Math.Min(180, (interior - 20) / 2);

            lista.SetBounds(20, 56, interior, lista.Height);
            monto.SetBounds(20, 124, anchoMonto, monto.Height);
            boton.SetBounds(20 + anchoMonto + 20, 122, interior - anchoMonto - 20, boton.Height);
            resultado.SetBounds(20, 170, interior, Math.Max(60, grupo.Height - 190));
        }

        private void CargarMetas()
        {
            try
            {
                campoMeta.Items.Clear();

                foreach (MetaResumen meta in Servicios.Metas.Listar())
                {
                    campoMeta.Items.Add(meta);
                }

                campoMeta.DisplayMember = "Nombre";

                if (campoMeta.Items.Count > 0)
                {
                    campoMeta.SelectedIndex = 0;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void CargarDeudas()
        {
            try
            {
                campoDeuda.Items.Clear();

                foreach (DeudaResumen deuda in Servicios.Deudas.Listar())
                {
                    campoDeuda.Items.Add(deuda);
                }

                campoDeuda.DisplayMember = "Acreedor";

                if (campoDeuda.Items.Count > 0)
                {
                    campoDeuda.SelectedIndex = 0;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlSimularAhorro(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoMeta.SelectedItem == null)
                {
                    UtilidadesVista.AvisarError("Selecciona una meta. Si no tienes, crea una primero.");
                    return;
                }

                MetaResumen resumen = (MetaResumen)campoMeta.SelectedItem;

                Meta meta = new Meta();
                meta.Id = resumen.Id;
                meta.Nombre = resumen.Nombre;
                meta.MontoObjetivo = resumen.MontoObjetivo;
                meta.MontoActual = resumen.MontoActual;
                meta.FechaLimite = resumen.FechaLimite;
                meta.Prioridad = resumen.Prioridad;

                ResultadoSimulacionAhorro resultado = Servicios.Simuladores.SimularAhorro(meta, campoAhorroMensual.Value);

                if (!resultado.Posible)
                {
                    resultadoAhorro.Text = resultado.Mensaje;
                    return;
                }

                resultadoAhorro.Text = resultado.Mensaje + Environment.NewLine +
                                       "Falta por ahorrar: " + UtilidadesVista.Moneda(resultado.Faltante) + Environment.NewLine +
                                       "Fecha estimada: " + resultado.FechaEstimada.ToString("dd/MM/yyyy");
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlSimularDeuda(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoDeuda.SelectedItem == null)
                {
                    UtilidadesVista.AvisarError("Selecciona una deuda. Si no tienes, registra una primero.");
                    return;
                }

                DeudaResumen resumen = (DeudaResumen)campoDeuda.SelectedItem;

                Deuda deuda = new Deuda();
                deuda.Id = resumen.Id;
                deuda.Acreedor = resumen.Acreedor;
                deuda.Capital = resumen.Capital;
                deuda.Saldo = resumen.Saldo;
                deuda.TasaInteres = resumen.TasaInteres;
                deuda.PagoMinimo = resumen.PagoMinimo;
                deuda.FechaVencimiento = resumen.FechaVencimiento;

                ResultadoSimulacionDeuda resultado = Servicios.Simuladores.SimularPagoDeuda(deuda, campoPagoMensual.Value);

                if (!resultado.Posible)
                {
                    resultadoDeuda.Text = resultado.Mensaje;
                    return;
                }

                resultadoDeuda.Text = resultado.Mensaje + Environment.NewLine +
                                      "Total pagado: " + UtilidadesVista.Moneda(resultado.TotalPagado) + Environment.NewLine +
                                      "Intereses: " + UtilidadesVista.Moneda(resultado.TotalIntereses) + Environment.NewLine +
                                      "Fecha estimada: " + resultado.FechaEstimada.ToString("dd/MM/yyyy");
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
