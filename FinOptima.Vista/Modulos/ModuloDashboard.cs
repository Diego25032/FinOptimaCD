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
    public class ModuloDashboard : ModuloBase
    {
        private TarjetaPanel tarjetaCuenta;
        private Label etiquetaCuentaRotulo;
        private Label etiquetaCuentaSaldo;
        private Label etiquetaCuentaNota;
        private Label etiquetaMes;
        private Button botonActualizarCuenta;

        private TarjetaPanel tarjetaSaldo;
        private TarjetaPanel tarjetaFlujo;
        private TarjetaPanel tarjetaGastos;
        private MedidorSemicirculo medidorSaldo;
        private MedidorSemicirculo medidorFlujo;
        private MedidorSemicirculo medidorGastos;
        private Label valorSaldo;
        private Label valorFlujo;
        private Label valorGastos;

        private TarjetaPanel tarjetaTendencia;
        private GraficoLinea graficoTendencia;
        private TarjetaPanel tarjetaEstructura;
        private GraficoDona graficoEstructura;

        private TarjetaPanel tarjetaSobres;
        private DataGridView grillaSobres;
        private Label etiquetaTotalSobres;
        private TarjetaPanel tarjetaAlertas;
        private DataGridView grillaAlertas;
        private Label etiquetaSinAlertas;

        private Label rotuloTendencia;
        private Label notaTendencia;
        private Label rotuloEstructura;
        private Label notaEstructura;
        private Label rotuloSobres;
        private Label rotuloAlertas;
        private Label notaAlertas;
        private Label[] rotulosMedidor = new Label[3];
        private Label[] notasMedidor = new Label[3];

        public ModuloDashboard(FabricaServicios servicios) : base(servicios, "Panel general")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            tarjetaCuenta = new TarjetaPanel();
            tarjetaCuenta.ColorFondo = tema.Superficie;
            tarjetaCuenta.ColorInicio = tema.Superficie;
            tarjetaCuenta.ColorFin = Tema.Mezclar(tema.Superficie, Acento, 34);
            tarjetaCuenta.ColorBorde = Tema.Mezclar(tema.Superficie, Acento, 42);

            etiquetaCuentaRotulo = CrearRotulo("CUENTA PRINCIPAL", 24, 18, 8.5f, true);
            tarjetaCuenta.Controls.Add(etiquetaCuentaRotulo);

            etiquetaCuentaSaldo = new Label();
            etiquetaCuentaSaldo.Text = "";
            etiquetaCuentaSaldo.Location = new Point(24, 40);
            etiquetaCuentaSaldo.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            etiquetaCuentaSaldo.Size = new Size(460, etiquetaCuentaSaldo.PreferredHeight);
            etiquetaCuentaSaldo.ForeColor = tema.Texto;
            etiquetaCuentaSaldo.BackColor = Color.Transparent;
            etiquetaCuentaSaldo.AutoEllipsis = true;
            tarjetaCuenta.Controls.Add(etiquetaCuentaSaldo);

            etiquetaCuentaNota = new Label();
            etiquetaCuentaNota.Text = "";
            etiquetaCuentaNota.Location = new Point(26, 80);
            etiquetaCuentaNota.Font = new Font("Segoe UI", 9);
            etiquetaCuentaNota.AutoEllipsis = true;
            etiquetaCuentaNota.Size = new Size(620, etiquetaCuentaNota.PreferredHeight * 2);
            etiquetaCuentaNota.ForeColor = tema.TextoSecundario;
            etiquetaCuentaNota.BackColor = Color.Transparent;
            tarjetaCuenta.Controls.Add(etiquetaCuentaNota);

            etiquetaMes = new Label();
            etiquetaMes.Text = "";
            etiquetaMes.AutoSize = false;
            etiquetaMes.TextAlign = ContentAlignment.MiddleCenter;
            etiquetaMes.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            etiquetaMes.ForeColor = tema.Texto;
            etiquetaMes.BackColor = Tema.Mezclar(tema.Superficie, Acento, 20);
            etiquetaMes.Size = new Size(Escalar(140), Math.Max(Escalar(28), etiquetaMes.PreferredHeight + 10));
            UtilidadesVista.Redondear(etiquetaMes, etiquetaMes.Height / 2);
            tarjetaCuenta.Controls.Add(etiquetaMes);

            botonActualizarCuenta = UtilidadesVista.CrearBotonSecundario("Actualizar", 0, 0, Escalar(110), Escalar(32));
            botonActualizarCuenta.Click += AlActualizar;
            tarjetaCuenta.Controls.Add(botonActualizarCuenta);

            Controls.Add(tarjetaCuenta);

            tarjetaSaldo = CrearTarjetaMedidor(0, "SALDO", "En sobres sobre ingresos", tema.Exito, out valorSaldo, out medidorSaldo);
            tarjetaFlujo = CrearTarjetaMedidor(1, "FLUJO DE CAJA", "Disponible sobre ingresos", tema.Exito, out valorFlujo, out medidorFlujo);
            tarjetaGastos = CrearTarjetaMedidor(2, "GASTOS", "Gasto sobre ingresos", tema.Peligro, out valorGastos, out medidorGastos);
            Controls.Add(tarjetaSaldo);
            Controls.Add(tarjetaFlujo);
            Controls.Add(tarjetaGastos);

            tarjetaTendencia = new TarjetaPanel();
            tarjetaTendencia.ColorFondo = tema.Superficie;
            rotuloTendencia = CrearRotulo("Tendencia de saldo", 22, 18, 12, true);
            rotuloTendencia.ForeColor = tema.Texto;
            tarjetaTendencia.Controls.Add(rotuloTendencia);
            notaTendencia = CrearRotulo("Ahorro acumulado de los últimos 6 meses", 22, rotuloTendencia.Bottom + 2, 8.5f, false);
            tarjetaTendencia.Controls.Add(notaTendencia);

            graficoTendencia = new GraficoLinea();
            graficoTendencia.ColorLinea = Acento;
            graficoTendencia.ColorGrilla = tema.BordeSuave;
            graficoTendencia.ColorTexto = tema.TextoSecundario;
            tarjetaTendencia.Controls.Add(graficoTendencia);
            Controls.Add(tarjetaTendencia);

            tarjetaEstructura = new TarjetaPanel();
            tarjetaEstructura.ColorFondo = tema.Superficie;
            rotuloEstructura = CrearRotulo("Estructura de los gastos", 22, 18, 12, true);
            rotuloEstructura.ForeColor = tema.Texto;
            tarjetaEstructura.Controls.Add(rotuloEstructura);
            notaEstructura = CrearRotulo("Reparto por categoría del mes", 22, rotuloEstructura.Bottom + 2, 8.5f, false);
            tarjetaEstructura.Controls.Add(notaEstructura);

            graficoEstructura = new GraficoDona();
            graficoEstructura.ColorPista = tema.SuperficieAlterna;
            graficoEstructura.ColorTexto = tema.TextoSecundario;
            graficoEstructura.TextoVacio = "Sin gastos registrados este mes";
            tarjetaEstructura.Controls.Add(graficoEstructura);
            Controls.Add(tarjetaEstructura);

            tarjetaSobres = new TarjetaPanel();
            tarjetaSobres.ColorFondo = tema.Superficie;
            rotuloSobres = CrearRotulo("Distribución de sobres", 22, 18, 12, true);
            rotuloSobres.ForeColor = tema.Texto;
            tarjetaSobres.Controls.Add(rotuloSobres);

            etiquetaTotalSobres = CrearRotulo("", 22, rotuloSobres.Bottom + 2, 8.5f, false);
            tarjetaSobres.Controls.Add(etiquetaTotalSobres);

            grillaSobres = new GrillaDatos();
            UtilidadesVista.PrepararGrilla(grillaSobres);
            grillaSobres.MinimumSize = new Size(0, 0);
            grillaSobres.Columns.Add("Nombre", "Sobre");
            grillaSobres.Columns.Add("Objetivo", "Objetivo %");
            grillaSobres.Columns.Add("Saldo", "Saldo");
            grillaSobres.Columns.Add("Sugerido", "Sugerido");
            grillaSobres.Columns.Add("Estado", "Estado");
            tarjetaSobres.Controls.Add(grillaSobres);
            Controls.Add(tarjetaSobres);

            tarjetaAlertas = new TarjetaPanel();
            tarjetaAlertas.ColorFondo = tema.Superficie;
            rotuloAlertas = CrearRotulo("Alertas activas", 22, 18, 12, true);
            rotuloAlertas.ForeColor = tema.Texto;
            tarjetaAlertas.Controls.Add(rotuloAlertas);
            notaAlertas = CrearRotulo("Presupuestos, deudas, metas y sobres", 22, rotuloAlertas.Bottom + 2, 8.5f, false);
            notaAlertas.AutoEllipsis = true;
            tarjetaAlertas.Controls.Add(notaAlertas);

            grillaAlertas = new GrillaDatos();
            UtilidadesVista.PrepararGrilla(grillaAlertas);
            grillaAlertas.MinimumSize = new Size(0, 0);
            grillaAlertas.Columns.Add("Nivel", "Nivel");
            grillaAlertas.Columns.Add("Titulo", "Alerta");
            grillaAlertas.Columns.Add("Mensaje", "Detalle");
            grillaAlertas.Columns[0].FillWeight = 22;
            grillaAlertas.Columns[1].FillWeight = 38;
            grillaAlertas.Columns[2].FillWeight = 70;
            grillaAlertas.ShowCellToolTips = true;
            tarjetaAlertas.Controls.Add(grillaAlertas);

            etiquetaSinAlertas = new Label();
            etiquetaSinAlertas.Text = "No hay alertas activas. Todo va en orden.";
            etiquetaSinAlertas.TextAlign = ContentAlignment.MiddleCenter;
            etiquetaSinAlertas.Font = new Font("Segoe UI", 10);
            etiquetaSinAlertas.ForeColor = tema.TextoSecundario;
            etiquetaSinAlertas.BackColor = Color.Transparent;
            etiquetaSinAlertas.Visible = false;
            tarjetaAlertas.Controls.Add(etiquetaSinAlertas);
            etiquetaSinAlertas.BringToFront();
            Controls.Add(tarjetaAlertas);
        }

        private TarjetaPanel CrearTarjetaMedidor(int indice, string titulo, string nota, Color color, out Label valor, out MedidorSemicirculo medidor)
        {
            Tema tema = TemaActual.Actual;

            TarjetaPanel tarjeta = new TarjetaPanel();
            tarjeta.ColorFondo = tema.Superficie;

            Label rotulo = CrearRotulo(titulo, 20, 16, 8.5f, true);
            tarjeta.Controls.Add(rotulo);
            Label detalle = CrearRotulo(nota, 20, rotulo.Bottom + 2, 8, false);
            detalle.AutoEllipsis = true;
            tarjeta.Controls.Add(detalle);
            rotulosMedidor[indice] = rotulo;
            notasMedidor[indice] = detalle;

            medidor = new MedidorSemicirculo();
            medidor.ColorValor = color;
            medidor.ColorPista = tema.SuperficieAlterna;
            tarjeta.Controls.Add(medidor);

            valor = new Label();
            valor.Text = "";
            valor.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            valor.ForeColor = tema.Texto;
            valor.BackColor = Color.Transparent;
            valor.AutoEllipsis = true;
            valor.TextAlign = ContentAlignment.MiddleLeft;
            tarjeta.Controls.Add(valor);

            return tarjeta;
        }

        private Label CrearRotulo(string texto, int x, int y, float tamano, bool negrita)
        {
            Label rotulo = new Label();
            rotulo.Text = texto;
            rotulo.Location = new Point(x, y);
            rotulo.AutoSize = true;
            rotulo.Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular);
            rotulo.ForeColor = TemaActual.Actual.TextoSecundario;
            rotulo.BackColor = Color.Transparent;
            return rotulo;
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        /// <summary>
        /// Acomoda el panel en tres filas que llenan la ventana:
        ///   1. Cuenta principal + tres medidores.
        ///   2. Tendencia de saldo + estructura de gastos.
        ///   3. Distribucion de sobres + alertas.
        /// Las alturas salen del espacio disponible y de la fuente real (escala de Windows),
        /// asi que en pantallas grandes todo cabe sin scroll y en pantallas pequenas aparece
        /// un scroll vertical sin cortar el contenido de las tarjetas.
        /// </summary>
        private void Distribuir()
        {
            if (tarjetaCuenta == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int espacio = Escalar(16);
                int inicio = InicioContenido;
                int inicioLogico = inicio - AutoScrollPosition.Y;

                // ---- Alturas ----
                int altoValorMedidor = valorSaldo.PreferredHeight + 4;
                int cabeceraMedidor = notasMedidor[0].Bottom + 6;
                int altoFila1 = cabeceraMedidor + Escalar(78) + altoValorMedidor + Escalar(14);

                int altoCuentaNecesario = Escalar(18) + etiquetaCuentaRotulo.Height + 6 + etiquetaCuentaSaldo.PreferredHeight
                    + etiquetaCuentaNota.PreferredHeight * 2 + 10 + botonActualizarCuenta.Height + Escalar(16);

                if (altoCuentaNecesario > altoFila1)
                {
                    altoFila1 = altoCuentaNecesario;
                }

                int minimoFila2 = Escalar(250);
                int minimoFila3 = Escalar(220);
                int disponible = ClientSize.Height - inicioLogico - altoFila1 - espacio * 3;
                int altoFila2 = (disponible * 54) / 100;
                int altoFila3 = disponible - altoFila2;

                if (altoFila2 < minimoFila2)
                {
                    altoFila2 = minimoFila2;
                }

                if (altoFila3 < minimoFila3)
                {
                    altoFila3 = minimoFila3;
                }

                int altoContenido = inicioLogico + altoFila1 + espacio + altoFila2 + espacio + altoFila3 + espacio;

                // ---- Ancho (descontando la barra de scroll si va a aparecer) ----
                int anchoCliente = ClientSize.Width;

                if (altoContenido > ClientSize.Height && !VerticalScroll.Visible)
                {
                    anchoCliente = anchoCliente - SystemInformation.VerticalScrollBarWidth;
                }

                int ancho = anchoCliente - Margen * 2;
                int anchoMinimo = Escalar(980);

                if (ancho < anchoMinimo)
                {
                    ancho = anchoMinimo;
                }

                int x = Margen + AutoScrollPosition.X;

                // ---- Fila 1: cuenta + medidores ----
                int anchoCuenta = (ancho * 32) / 100;

                if (anchoCuenta < Escalar(330))
                {
                    anchoCuenta = Escalar(330);
                }

                int anchoMedidor = (ancho - anchoCuenta - espacio * 3) / 3;
                int y = inicio;

                tarjetaCuenta.SetBounds(x, y, anchoCuenta, altoFila1);
                DistribuirCuenta(anchoCuenta, altoFila1);

                TarjetaPanel[] tarjetas = { tarjetaSaldo, tarjetaFlujo, tarjetaGastos };
                MedidorSemicirculo[] medidores = { medidorSaldo, medidorFlujo, medidorGastos };
                Label[] valores = { valorSaldo, valorFlujo, valorGastos };

                for (int indice = 0; indice < 3; indice = indice + 1)
                {
                    int xTarjeta = x + anchoCuenta + espacio + (anchoMedidor + espacio) * indice;
                    tarjetas[indice].SetBounds(xTarjeta, y, anchoMedidor, altoFila1);

                    int anchoInterior = anchoMedidor - 36;
                    int yValor = altoFila1 - Escalar(12) - altoValorMedidor;
                    notasMedidor[indice].AutoSize = false;
                    notasMedidor[indice].SetBounds(20, rotulosMedidor[indice].Bottom + 2, anchoInterior, notasMedidor[indice].PreferredHeight);
                    medidores[indice].SetBounds(18, cabeceraMedidor, anchoInterior, yValor - cabeceraMedidor);
                    valores[indice].SetBounds(20, yValor, anchoInterior, altoValorMedidor);
                }

                // ---- Fila 2: tendencia + estructura ----
                y = y + altoFila1 + espacio;
                int anchoTendencia = (ancho * 60) / 100;
                int anchoEstructura = ancho - anchoTendencia - espacio;

                if (anchoEstructura < Escalar(360))
                {
                    anchoEstructura = Escalar(360);
                    anchoTendencia = ancho - anchoEstructura - espacio;
                }

                tarjetaTendencia.SetBounds(x, y, anchoTendencia, altoFila2);
                tarjetaEstructura.SetBounds(x + anchoTendencia + espacio, y, anchoEstructura, altoFila2);

                int topTendencia = notaTendencia.Bottom + 10;
                int topEstructura = notaEstructura.Bottom + 4;
                graficoTendencia.SetBounds(16, topTendencia, anchoTendencia - 32, altoFila2 - topTendencia - 14);
                graficoEstructura.SetBounds(6, topEstructura, anchoEstructura - 12, altoFila2 - topEstructura - 8);

                // ---- Fila 3: sobres + alertas ----
                y = y + altoFila2 + espacio;
                int anchoSobres = (ancho * 58) / 100;
                int anchoAlertas = ancho - anchoSobres - espacio;

                tarjetaSobres.SetBounds(x, y, anchoSobres, altoFila3);
                tarjetaAlertas.SetBounds(x + anchoSobres + espacio, y, anchoAlertas, altoFila3);

                int topSobres = etiquetaTotalSobres.Bottom + 10;
                int topAlertas = notaAlertas.Bottom + 10;
                notaAlertas.AutoSize = false;
                notaAlertas.SetBounds(22, rotuloAlertas.Bottom + 2, anchoAlertas - 44, notaAlertas.PreferredHeight);
                grillaSobres.SetBounds(16, topSobres, anchoSobres - 32, altoFila3 - topSobres - 16);
                grillaAlertas.SetBounds(16, topAlertas, anchoAlertas - 32, altoFila3 - topAlertas - 16);
                etiquetaSinAlertas.SetBounds(16, topAlertas, anchoAlertas - 32, altoFila3 - topAlertas - 16);

                AutoScrollMinSize = new Size(ancho + Margen * 2, altoContenido);
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private void DistribuirCuenta(int ancho, int alto)
        {
            int interior = ancho - 48;

            etiquetaCuentaRotulo.Location = new Point(24, Escalar(18));
            etiquetaMes.Location = new Point(ancho - 20 - etiquetaMes.Width, Escalar(14));

            int ySaldo = etiquetaCuentaRotulo.Bottom + 6;
            etiquetaCuentaSaldo.SetBounds(22, ySaldo, interior, etiquetaCuentaSaldo.PreferredHeight);

            int yNota = etiquetaCuentaSaldo.Bottom;
            etiquetaCuentaNota.SetBounds(26, yNota, interior, etiquetaCuentaNota.PreferredHeight * 2 + 2);

            botonActualizarCuenta.Location = new Point(24, alto - Escalar(16) - botonActualizarCuenta.Height);
        }

        private int AnchoSeguro(int valor, int minimo)
        {
            if (valor < minimo)
            {
                return minimo;
            }

            return valor;
        }

        private void CargarDatos()
        {
            try
            {
                ResumenDashboard resumen = Servicios.Dashboard.Obtener();
                etiquetaMes.Text = resumen.NombreMes + " " + resumen.Anio;
                ContadorMoneda.Animar(etiquetaCuentaSaldo, resumen.SaldoSobres);
                etiquetaCuentaNota.Text = "Saldo en sobres\nDisponible del mes: " + UtilidadesVista.Moneda(resumen.DisponibleMes);

                medidorSaldo.Valor = CalcularRatio(resumen.SaldoSobres, resumen.IngresosMes);
                medidorFlujo.Valor = CalcularRatio(resumen.DisponibleMes, resumen.IngresosMes);
                medidorGastos.Valor = CalcularRatio(resumen.GastosMes, resumen.IngresosMes);

                ContadorMoneda.Animar(valorSaldo, resumen.SaldoSobres);
                ContadorMoneda.Animar(valorFlujo, resumen.DisponibleMes);
                ContadorMoneda.Animar(valorGastos, resumen.GastosMes);

                medidorFlujo.ColorValor = resumen.DisponibleMes >= 0 ? TemaActual.Actual.Exito : TemaActual.Actual.Peligro;

                CargarSobres(resumen);
                CargarAlertas(resumen);
                CargarTendencia();
                CargarEstructura(resumen);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private double CalcularRatio(decimal parte, decimal total)
        {
            if (total <= 0)
            {
                return 0;
            }

            double ratio = (double)(parte / total);

            if (ratio < 0)
            {
                return 0;
            }

            if (ratio > 1)
            {
                return 1;
            }

            return ratio;
        }

        private void CargarSobres(ResumenDashboard resumen)
        {
            grillaSobres.Rows.Clear();
            decimal total = 0;

            foreach (SobreResumen sobre in resumen.Sobres)
            {
                total = total + sobre.Saldo;
                grillaSobres.Rows.Add(
                    sobre.Nombre,
                    UtilidadesVista.Porcentaje(sobre.PorcentajeObjetivo),
                    UtilidadesVista.Moneda(sobre.Saldo),
                    UtilidadesVista.Moneda(sobre.MontoSugerido),
                    sobre.Estado);
            }

            etiquetaTotalSobres.Text = "Total disponible: " + UtilidadesVista.Moneda(total);
        }

        private void CargarAlertas(ResumenDashboard resumen)
        {
            grillaAlertas.Rows.Clear();

            foreach (Alerta alerta in resumen.Alertas)
            {
                int fila = grillaAlertas.Rows.Add(alerta.Nivel, alerta.Titulo, alerta.Mensaje);
                grillaAlertas.Rows[fila].Cells[2].ToolTipText = alerta.Mensaje;
            }

            bool hayAlertas = resumen.Alertas.Count > 0;
            grillaAlertas.Visible = hayAlertas;
            etiquetaSinAlertas.Visible = !hayAlertas;
        }

        private void CargarTendencia()
        {
            List<ResumenMensual> meses = Servicios.Analisis.ListarUltimosMeses(6);
            List<decimal> valores = new List<decimal>();
            List<string> etiquetas = new List<string>();
            decimal acumulado = 0;
            bool hayMovimientos = false;

            for (int indice = meses.Count - 1; indice >= 0; indice = indice - 1)
            {
                ResumenMensual mes = meses[indice];
                acumulado = acumulado + mes.Ahorro;
                valores.Add(acumulado);
                etiquetas.Add(mes.NombreMes);

                if (mes.Ingresos != 0 || mes.Gastos != 0)
                {
                    hayMovimientos = true;
                }
            }

            if (!hayMovimientos)
            {
                graficoTendencia.EstablecerDatos(new List<decimal>(), new List<string>());
                return;
            }

            graficoTendencia.EstablecerDatos(valores, etiquetas);
        }

        private void CargarEstructura(ResumenDashboard resumen)
        {
            ResumenMensual mensual = Servicios.Analisis.ObtenerResumen(resumen.Anio, resumen.Mes);
            Color[] colores = new Color[]
            {
                Color.FromArgb(23, 201, 100),
                Color.FromArgb(0, 145, 255),
                Color.FromArgb(245, 165, 36),
                Color.FromArgb(147, 97, 255),
                Color.FromArgb(243, 18, 96),
                Color.FromArgb(0, 184, 169)
            };

            List<SegmentoDona> segmentos = new List<SegmentoDona>();

            for (int indice = 0; indice < mensual.GastosPorCategoria.Count; indice = indice + 1)
            {
                DetalleCategoria detalle = mensual.GastosPorCategoria[indice];
                segmentos.Add(new SegmentoDona(detalle.Categoria, detalle.Monto, detalle.Porcentaje, colores[indice % colores.Length]));
            }

            graficoEstructura.EstablecerDatos(segmentos);
        }

        private void AlActualizar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }
    }
}
