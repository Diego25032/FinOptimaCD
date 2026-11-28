using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloAnalisis : ModuloBase
    {
        private Label nota;
        private Label etiquetaMes;
        private Label etiquetaAnio;
        private ComboBox campoMes;
        private NumericUpDown campoAnio;
        private Button consultar;
        private Panel tarjetaIngresos;
        private Panel tarjetaGastos;
        private Panel tarjetaAhorro;
        private Panel tarjetaPorcentaje;
        private DataGridView grillaGastos;
        private DataGridView grillaIngresos;
        private DataGridView grillaMeses;
        private Label tituloGastos;
        private Label tituloIngresos;
        private Label tituloMeses;

        public ModuloAnalisis(FabricaServicios servicios) : base(servicios, "Analisis mensual")
        {
            ConstruirInterfaz();
            CargarMeses();
            Consultar();
            CargarUltimosMeses();
            Distribuir();
        }

        private void ConstruirInterfaz()
        {
            nota = UtilidadesVista.CrearNota(
                "Compara ingresos, gastos y ahorro por categoria en cada mes.",
                Margen, InicioContenido, 880);
            Controls.Add(nota);

            int fila = FilaCampo(85);

            etiquetaMes = UtilidadesVista.CrearEtiqueta("Mes", Margen, 85);
            Controls.Add(etiquetaMes);
            campoMes = UtilidadesVista.CrearLista(Margen, fila, 180);
            Controls.Add(campoMes);

            etiquetaAnio = UtilidadesVista.CrearEtiqueta("Anio", Margen + 200, 85);
            Controls.Add(etiquetaAnio);
            campoAnio = new NumericUpDown();
            campoAnio.Location = new Point(Margen + 200, fila);
            campoAnio.Width = 100;
            campoAnio.Minimum = 2000;
            campoAnio.Maximum = 2100;
            campoAnio.Value = DateTime.Today.Year;
            UtilidadesVista.EstilizarCampo(campoAnio);
            Controls.Add(campoAnio);

            consultar = Boton("Consultar", Margen + 330, fila - 4, 130);
            consultar.Click += AlConsultar;
            Controls.Add(consultar);

            int posicionTarjetas = fila + 48;
            int posicionTitulos = posicionTarjetas + 100;
            int posicionGrillas = posicionTitulos + 38;

            tarjetaIngresos = UtilidadesVista.CrearTarjeta("Ingresos del mes", Margen, posicionTarjetas, 200, 78, Acento);
            Controls.Add(tarjetaIngresos);

            tarjetaGastos = UtilidadesVista.CrearTarjeta("Gastos del mes", Margen + 215, posicionTarjetas, 200, 78, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaGastos);

            tarjetaAhorro = UtilidadesVista.CrearTarjeta("Ahorro del mes", Margen + 430, posicionTarjetas, 200, 78, TemaActual.Actual.Exito);
            Controls.Add(tarjetaAhorro);

            tarjetaPorcentaje = UtilidadesVista.CrearTarjeta("Porcentaje de ahorro", Margen + 645, posicionTarjetas, 200, 78, Acento);
            Controls.Add(tarjetaPorcentaje);

            tituloGastos = new Label();
            tituloGastos.Text = "Gastos por categoria";
            tituloGastos.Location = new Point(Margen, posicionTitulos);
            tituloGastos.AutoSize = true;
            tituloGastos.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloGastos.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloGastos);

            grillaGastos = CrearGrilla(Margen, posicionGrillas, 410);
            grillaGastos.Columns.Add("Categoria", "Categoria");
            grillaGastos.Columns.Add("Monto", "Monto");
            grillaGastos.Columns.Add("Porcentaje", "Porcentaje");
            Controls.Add(grillaGastos);

            tituloIngresos = new Label();
            tituloIngresos.Text = "Ingresos por categoria";
            tituloIngresos.Location = new Point(Margen + 430, posicionTitulos);
            tituloIngresos.AutoSize = true;
            tituloIngresos.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloIngresos.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloIngresos);

            grillaIngresos = CrearGrilla(Margen + 430, posicionGrillas, 410);
            grillaIngresos.Columns.Add("Categoria", "Categoria");
            grillaIngresos.Columns.Add("Monto", "Monto");
            grillaIngresos.Columns.Add("Porcentaje", "Porcentaje");
            Controls.Add(grillaIngresos);

            int posicionTituloMeses = posicionGrillas + 214;

            tituloMeses = new Label();
            tituloMeses.Text = "Ultimos seis meses";
            tituloMeses.Location = new Point(Margen, posicionTituloMeses);
            tituloMeses.AutoSize = true;
            tituloMeses.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloMeses.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloMeses);

            grillaMeses = new GrillaDatos();
            grillaMeses.Location = new Point(Margen, posicionTituloMeses + 34);
            grillaMeses.Size = new Size(840, 170);
            grillaMeses.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaMeses);
            grillaMeses.Columns.Add("Mes", "Mes");
            grillaMeses.Columns.Add("Ingresos", "Ingresos");
            grillaMeses.Columns.Add("Gastos", "Gastos");
            grillaMeses.Columns.Add("Ahorro", "Ahorro");
            grillaMeses.Columns.Add("Porcentaje", "Porcentaje de ahorro");
            Controls.Add(grillaMeses);
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (tarjetaIngresos == null || grillaMeses == null || Recalculando)
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
            int separacion = Escalar(16);
            ancho = Math.Max(Escalar(320), ancho);
            int x = Margen + AutoScrollPosition.X;

            UtilidadesVista.AjustarEtiqueta(nota, ancho);

            int yEtiqueta = nota.Bottom + Escalar(12);
            int yCampo = FilaCampo(yEtiqueta);
            etiquetaMes.Location = new Point(x, yEtiqueta);
            campoMes.SetBounds(x, yCampo, Escalar(180), campoMes.Height);
            etiquetaAnio.Location = new Point(x + Escalar(200), yEtiqueta);
            campoAnio.SetBounds(x + Escalar(200), yCampo, Escalar(100), campoAnio.Height);
            consultar.SetBounds(x + Escalar(330), yCampo - 4, Escalar(130), consultar.Height);

            int yTarjetas = Math.Max(Math.Max(campoMes.Bottom, campoAnio.Bottom), consultar.Bottom) + Escalar(18);
            int columnas = ancho >= Escalar(850) ? 4 : (ancho >= Escalar(430) ? 2 : 1);
            int anchoTarjeta = (ancho - separacion * (columnas - 1)) / columnas;
            int altoTarjeta = Escalar(78);
            Panel[] tarjetas = { tarjetaIngresos, tarjetaGastos, tarjetaAhorro, tarjetaPorcentaje };

            for (int indice = 0; indice < tarjetas.Length; indice = indice + 1)
            {
                int columna = indice % columnas;
                int fila = indice / columnas;
                UtilidadesVista.AjustarTarjeta(
                    tarjetas[indice],
                    anchoTarjeta,
                    altoTarjeta);
                tarjetas[indice].Location = new Point(
                    x + columna * (anchoTarjeta + separacion),
                    yTarjetas + fila * (altoTarjeta + separacion));
            }

            int filasTarjetas = (tarjetas.Length + columnas - 1) / columnas;
            int yTitulos = yTarjetas + filasTarjetas * altoTarjeta + (filasTarjetas - 1) * separacion + Escalar(22);
            bool enColumnas = ancho >= Escalar(840);
            int anchoGrilla = enColumnas ? (ancho - separacion) / 2 : ancho;
            int altoPareja = Escalar(200);

            if (enColumnas)
            {
                int deseado = ClientSize.Height - (yTitulos + Escalar(34)) - Escalar(240);
                altoPareja = Math.Min(Escalar(420), Math.Max(altoPareja, deseado));
            }

            tituloGastos.Location = new Point(x, yTitulos);
            grillaGastos.SetBounds(x, yTitulos + Escalar(34), anchoGrilla, altoPareja);

            if (enColumnas)
            {
                tituloIngresos.Location = new Point(x + anchoGrilla + separacion, yTitulos);
                grillaIngresos.SetBounds(x + anchoGrilla + separacion, yTitulos + Escalar(34), anchoGrilla, altoPareja);
            }
            else
            {
                int yIngresos = grillaGastos.Bottom + Escalar(20);
                tituloIngresos.Location = new Point(x, yIngresos);
                grillaIngresos.SetBounds(x, yIngresos + Escalar(34), anchoGrilla, altoPareja);
            }

            int yMeses = grillaIngresos.Bottom + Escalar(20);
            tituloMeses.Location = new Point(x, yMeses);
            grillaMeses.SetBounds(x, yMeses + Escalar(34), ancho, Escalar(170));

            return grillaMeses.Bottom - AutoScrollPosition.Y + Margen;
        }

        private DataGridView CrearGrilla(int x, int y, int ancho)
        {
            DataGridView grilla = new GrillaDatos();
            grilla.Location = new Point(x, y);
            grilla.Size = new Size(ancho, 200);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            UtilidadesVista.PrepararGrilla(grilla);
            return grilla;
        }

        private void CargarMeses()
        {
            for (int mes = 1; mes <= 12; mes = mes + 1)
            {
                DateTime fecha = new DateTime(2000, mes, 1);
                string nombre = fecha.ToString("MMMM");
                nombre = char.ToUpper(nombre[0]) + nombre.Substring(1);
                campoMes.Items.Add(nombre);
            }

            campoMes.SelectedIndex = DateTime.Today.Month - 1;
        }

        private void AlConsultar(object remitente, EventArgs argumentos)
        {
            Consultar();
        }

        private void Consultar()
        {
            try
            {
                int mes = campoMes.SelectedIndex + 1;
                int anio = Convert.ToInt32(campoAnio.Value);
                ResumenMensual resumen = Servicios.Analisis.ObtenerResumen(anio, mes);

                UtilidadesVista.PonerValor(tarjetaIngresos, UtilidadesVista.Moneda(resumen.Ingresos));
                UtilidadesVista.PonerValor(tarjetaGastos, UtilidadesVista.Moneda(resumen.Gastos));
                UtilidadesVista.PonerValor(tarjetaAhorro, UtilidadesVista.Moneda(resumen.Ahorro));
                UtilidadesVista.PonerValor(tarjetaPorcentaje, UtilidadesVista.Porcentaje(resumen.PorcentajeAhorro));

                grillaGastos.Rows.Clear();

                foreach (DetalleCategoria detalle in resumen.GastosPorCategoria)
                {
                    grillaGastos.Rows.Add(
                        detalle.Categoria,
                        UtilidadesVista.Moneda(detalle.Monto),
                        UtilidadesVista.Porcentaje(detalle.Porcentaje));
                }

                grillaIngresos.Rows.Clear();

                foreach (DetalleCategoria detalle in resumen.IngresosPorCategoria)
                {
                    grillaIngresos.Rows.Add(
                        detalle.Categoria,
                        UtilidadesVista.Moneda(detalle.Monto),
                        UtilidadesVista.Porcentaje(detalle.Porcentaje));
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void CargarUltimosMeses()
        {
            try
            {
                List<ResumenMensual> resumenes = Servicios.Analisis.ListarUltimosMeses(6);
                grillaMeses.Rows.Clear();

                foreach (ResumenMensual resumen in resumenes)
                {
                    grillaMeses.Rows.Add(
                        resumen.NombreMes + " " + resumen.Anio.ToString(),
                        UtilidadesVista.Moneda(resumen.Ingresos),
                        UtilidadesVista.Moneda(resumen.Gastos),
                        UtilidadesVista.Moneda(resumen.Ahorro),
                        UtilidadesVista.Porcentaje(resumen.PorcentajeAhorro));
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
