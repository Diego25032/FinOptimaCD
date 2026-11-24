using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Arma el libro de Excel: hoja de resumen y una tabla con filtros y totales por
    /// cada tipo de dato. Los anchos de columna son fijos para que ningun valor
    /// aparezca como "#####" y el resultado sea igual en cualquier equipo.
    /// </summary>
    internal class ReporteExcel
    {
        private const string FormatoMoneda = "\"$\" #,##0;[Red]-\"$\" #,##0";
        private const string FormatoPorcentaje = "0.0%";
        private const string FormatoFecha = "dd/mm/yyyy";

        private static readonly XLColor ColorMarca = XLColor.FromHtml("#0C261F");
        private static readonly XLColor ColorAcento = XLColor.FromHtml("#17C964");
        private static readonly XLColor ColorSuave = XLColor.FromHtml("#F5F7FA");
        private static readonly XLColor ColorBorde = XLColor.FromHtml("#E2E7ED");
        private static readonly XLColor ColorTextoSecundario = XLColor.FromHtml("#687585");
        private static readonly XLColor ColorPeligro = XLColor.FromHtml("#E5484D");

        private DatosReporte datos;
        private CalculosReporte calculos;

        public ReporteExcel(DatosReporte datosReporte)
        {
            datos = datosReporte;
            calculos = new CalculosReporte(datosReporte, 6);
        }

        public void Generar(string ruta)
        {
            using (XLWorkbook libro = new XLWorkbook())
            {
                libro.Properties.Title = "FinOptima - Reporte financiero";
                libro.Properties.Author = "FinOptima";
                libro.Properties.Subject = "Reporte de " + datos.NombreUsuario;
                libro.Style.Font.FontName = "Calibri";
                libro.Style.Font.FontSize = 11;

                CrearResumen(libro);
                CrearSobres(libro);
                CrearMovimientos(libro);
                CrearMetas(libro);
                CrearDeudas(libro);

                libro.Worksheet(1).SetTabActive();
                libro.SaveAs(ruta);
            }
        }

        // ---------- Resumen ----------

        private void CrearResumen(XLWorkbook libro)
        {
            IXLWorksheet hoja = libro.AddWorksheet("Resumen");
            hoja.SetTabColor(ColorAcento);
            hoja.ShowGridLines = false;

            hoja.Column(1).Width = 3;
            hoja.Column(2).Width = 30;
            hoja.Column(3).Width = 20;
            hoja.Column(4).Width = 4;
            hoja.Column(5).Width = 28;
            hoja.Column(6).Width = 18;
            hoja.Column(7).Width = 12;

            IXLRange banda = hoja.Range(1, 1, 3, 7);
            banda.Style.Fill.BackgroundColor = ColorMarca;

            hoja.Row(1).Height = 12;
            hoja.Row(2).Height = 34;
            hoja.Row(3).Height = 22;

            IXLCell titulo = hoja.Cell(2, 2);
            titulo.Value = "FinOptima · Reporte financiero";
            titulo.Style.Font.FontSize = 20;
            titulo.Style.Font.Bold = true;
            titulo.Style.Font.FontColor = XLColor.White;
            titulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            IXLCell subtitulo = hoja.Cell(3, 2);
            subtitulo.Value = "Preparado para " + datos.NombreUsuario + "  ·  Generado el " + CalculosReporte.FechaLarga(datos.FechaGeneracion) + " a las " + datos.FechaGeneracion.ToString("HH:mm");
            subtitulo.Style.Font.FontColor = XLColor.FromHtml("#A0C4B5");
            subtitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            int fila = 5;
            Subtitulo(hoja, fila, 2, "Indicadores principales");
            fila = fila + 1;

            fila = Indicador(hoja, fila, "Ingresos totales", calculos.Ingresos, FormatoMoneda, false);
            fila = Indicador(hoja, fila, "Gastos totales", calculos.Gastos, FormatoMoneda, false);
            fila = Indicador(hoja, fila, "Disponible (ingresos − gastos)", calculos.Disponible, FormatoMoneda, true);
            fila = Indicador(hoja, fila, "Tasa de ahorro", calculos.TasaAhorro, FormatoPorcentaje, false);
            fila = Indicador(hoja, fila, "Saldo en sobres", calculos.SaldoSobres, FormatoMoneda, false);
            fila = Indicador(hoja, fila, "Ahorro en metas", calculos.AhorroMetas, FormatoMoneda, false);
            fila = Indicador(hoja, fila, "Deudas pendientes", calculos.SaldoDeudas, FormatoMoneda, false);

            int filaCategorias = 5;
            Subtitulo(hoja, filaCategorias, 5, "Gastos por categoría");
            filaCategorias = filaCategorias + 1;

            Encabezado(hoja.Cell(filaCategorias, 5), "Categoría");
            Encabezado(hoja.Cell(filaCategorias, 6), "Total");
            Encabezado(hoja.Cell(filaCategorias, 7), "%");
            hoja.Range(filaCategorias, 6, filaCategorias, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            filaCategorias = filaCategorias + 1;

            if (calculos.GastosPorCategoria.Count == 0)
            {
                IXLCell vacio = hoja.Cell(filaCategorias, 5);
                vacio.Value = "Sin gastos registrados";
                vacio.Style.Font.Italic = true;
                vacio.Style.Font.FontColor = ColorTextoSecundario;
            }

            foreach (TotalCategoria categoria in calculos.GastosPorCategoria)
            {
                hoja.Cell(filaCategorias, 5).Value = categoria.Categoria;
                hoja.Cell(filaCategorias, 6).Value = categoria.Monto;
                hoja.Cell(filaCategorias, 6).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Cell(filaCategorias, 7).Value = categoria.Porcentaje;
                hoja.Cell(filaCategorias, 7).Style.NumberFormat.Format = FormatoPorcentaje;
                hoja.Range(filaCategorias, 5, filaCategorias, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                hoja.Range(filaCategorias, 5, filaCategorias, 7).Style.Border.BottomBorderColor = ColorBorde;
                filaCategorias = filaCategorias + 1;
            }

            if (calculos.GastosPorCategoria.Count > 0)
            {
                hoja.Range(7, 7, filaCategorias - 1, 7).AddConditionalFormat().DataBar(ColorAcento).LowestValue().HighestValue();
            }

            int filaMeses = Math.Max(fila, filaCategorias) + 2;
            Subtitulo(hoja, filaMeses, 2, "Últimos 6 meses");
            filaMeses = filaMeses + 1;

            Encabezado(hoja.Cell(filaMeses, 2), "Mes");
            Encabezado(hoja.Cell(filaMeses, 3), "Ingresos");
            Encabezado(hoja.Cell(filaMeses, 4), "");
            Encabezado(hoja.Cell(filaMeses, 5), "Gastos");
            Encabezado(hoja.Cell(filaMeses, 6), "Balance");
            hoja.Range(filaMeses, 3, filaMeses, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            filaMeses = filaMeses + 1;

            foreach (TotalMes mes in calculos.UltimosMeses)
            {
                hoja.Cell(filaMeses, 2).Value = mes.Etiqueta + " " + mes.Anio;
                hoja.Cell(filaMeses, 3).Value = mes.Ingresos;
                hoja.Cell(filaMeses, 5).Value = mes.Gastos;
                hoja.Cell(filaMeses, 6).Value = mes.Ingresos - mes.Gastos;
                hoja.Cell(filaMeses, 3).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Cell(filaMeses, 5).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Cell(filaMeses, 6).Style.NumberFormat.Format = FormatoMoneda;
                hoja.Range(filaMeses, 2, filaMeses, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                hoja.Range(filaMeses, 2, filaMeses, 6).Style.Border.BottomBorderColor = ColorBorde;
                filaMeses = filaMeses + 1;
            }

            ConfigurarImpresion(hoja, false);
        }

        private int Indicador(IXLWorksheet hoja, int fila, string etiqueta, decimal valor, string formato, bool resaltar)
        {
            IXLCell celdaEtiqueta = hoja.Cell(fila, 2);
            celdaEtiqueta.Value = etiqueta;
            celdaEtiqueta.Style.Font.FontColor = ColorTextoSecundario;

            IXLCell celdaValor = hoja.Cell(fila, 3);
            celdaValor.Value = valor;
            celdaValor.Style.NumberFormat.Format = formato;
            celdaValor.Style.Font.Bold = true;
            celdaValor.Style.Font.FontSize = 12;
            celdaValor.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            IXLRange rango = hoja.Range(fila, 2, fila, 3);
            rango.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.BottomBorderColor = ColorBorde;

            if (resaltar)
            {
                rango.Style.Fill.BackgroundColor = ColorSuave;
            }

            hoja.Row(fila).Height = 22;
            return fila + 1;
        }

        private static void Subtitulo(IXLWorksheet hoja, int fila, int columna, string texto)
        {
            IXLCell celda = hoja.Cell(fila, columna);
            celda.Value = texto;
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontSize = 13;
            celda.Style.Font.FontColor = ColorMarca;
            hoja.Row(fila).Height = 24;
        }

        private static void Encabezado(IXLCell celda, string texto)
        {
            celda.Value = texto;
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = ColorMarca;
        }

        // ---------- Hojas de datos ----------

        private void CrearSobres(XLWorkbook libro)
        {
            string[] columnas = { "Sobre", "Tipo", "Objetivo", "Mínimo", "Máximo", "Saldo", "Participación", "Estado" };
            double[] anchos = { 24, 16, 12, 12, 12, 18, 15, 12 };
            IXLWorksheet hoja = CrearHoja(libro, "Sobres", columnas, anchos);

            decimal totalPositivo = 0;

            foreach (Sobre sobre in datos.Sobres)
            {
                if (sobre.Activo && sobre.Saldo > 0)
                {
                    totalPositivo = totalPositivo + sobre.Saldo;
                }
            }

            int fila = 2;

            foreach (Sobre sobre in datos.Sobres)
            {
                decimal participacion = 0;

                if (sobre.Activo && sobre.Saldo > 0 && totalPositivo > 0)
                {
                    participacion = sobre.Saldo / totalPositivo;
                }

                hoja.Cell(fila, 1).Value = sobre.Nombre;
                hoja.Cell(fila, 2).Value = CalculosReporte.NombreTipoSobre(sobre.Tipo);
                hoja.Cell(fila, 3).Value = sobre.PorcentajeObjetivo / 100;
                hoja.Cell(fila, 4).Value = sobre.PorcentajeMinimo / 100;
                hoja.Cell(fila, 5).Value = sobre.PorcentajeMaximo / 100;
                hoja.Cell(fila, 6).Value = sobre.Saldo;
                hoja.Cell(fila, 7).Value = participacion;
                hoja.Cell(fila, 8).Value = sobre.Activo ? "Activo" : "Inactivo";
                fila = fila + 1;
            }

            if (fila == 2)
            {
                SinRegistros(hoja, columnas.Length, "Todavía no hay sobres registrados.");
                return;
            }

            FormatoColumna(hoja, 3, fila - 1, FormatoPorcentaje);
            FormatoColumna(hoja, 4, fila - 1, FormatoPorcentaje);
            FormatoColumna(hoja, 5, fila - 1, FormatoPorcentaje);
            FormatoColumna(hoja, 6, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 7, fila - 1, FormatoPorcentaje);
            hoja.Range(2, 7, fila - 1, 7).AddConditionalFormat().DataBar(ColorAcento).LowestValue().HighestValue();

            IXLTable tabla = CrearTabla(hoja, "TablaSobres", fila - 1, columnas.Length);
            tabla.Field("Sobre").TotalsRowLabel = "Total";
            tabla.Field("Saldo").TotalsRowFunction = XLTotalsRowFunction.Sum;
            hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoMoneda;
        }

        private void CrearMovimientos(XLWorkbook libro)
        {
            string[] columnas = { "Fecha", "Tipo", "Categoría", "Sobre", "Descripción", "Ingreso", "Gasto" };
            double[] anchos = { 13, 11, 18, 18, 48, 16, 16 };
            IXLWorksheet hoja = CrearHoja(libro, "Movimientos", columnas, anchos);

            int fila = 2;

            foreach (Movimiento movimiento in calculos.MovimientosOrdenados)
            {
                bool esIngreso = movimiento.Tipo == TipoMovimiento.Ingreso;

                hoja.Cell(fila, 1).Value = movimiento.Fecha;
                hoja.Cell(fila, 2).Value = CalculosReporte.TipoTexto(movimiento.Tipo);
                hoja.Cell(fila, 3).Value = movimiento.Categoria;
                hoja.Cell(fila, 4).Value = calculos.NombreSobre(movimiento.SobreId);
                hoja.Cell(fila, 5).Value = movimiento.Descripcion;

                if (esIngreso)
                {
                    hoja.Cell(fila, 6).Value = movimiento.Monto;
                }
                else
                {
                    hoja.Cell(fila, 7).Value = movimiento.Monto;
                }

                fila = fila + 1;
            }

            if (fila == 2)
            {
                SinRegistros(hoja, columnas.Length, "Aún no hay movimientos registrados.");
                return;
            }

            FormatoColumna(hoja, 1, fila - 1, FormatoFecha);
            FormatoColumna(hoja, 6, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 7, fila - 1, FormatoMoneda);
            hoja.Range(2, 6, fila - 1, 6).Style.Font.FontColor = XLColor.FromHtml("#0A823E");
            hoja.Range(2, 7, fila - 1, 7).Style.Font.FontColor = ColorPeligro;

            IXLTable tabla = CrearTabla(hoja, "TablaMovimientos", fila - 1, columnas.Length);
            tabla.Field("Fecha").TotalsRowLabel = "Total";
            tabla.Field("Ingreso").TotalsRowFunction = XLTotalsRowFunction.Sum;
            tabla.Field("Gasto").TotalsRowFunction = XLTotalsRowFunction.Sum;
            hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoMoneda;
            hoja.Cell(fila, 7).Style.NumberFormat.Format = FormatoMoneda;
        }

        private void CrearMetas(XLWorkbook libro)
        {
            string[] columnas = { "Meta", "Prioridad", "Objetivo", "Ahorrado", "Falta", "Progreso", "Fecha límite", "Días restantes" };
            double[] anchos = { 28, 12, 16, 16, 16, 13, 14, 15 };
            IXLWorksheet hoja = CrearHoja(libro, "Metas", columnas, anchos);

            int fila = 2;

            foreach (Meta meta in datos.Metas)
            {
                decimal progreso = 0;

                if (meta.MontoObjetivo > 0)
                {
                    progreso = meta.MontoActual / meta.MontoObjetivo;
                }

                decimal falta = meta.MontoObjetivo - meta.MontoActual;

                if (falta < 0)
                {
                    falta = 0;
                }

                int dias = (int)(meta.FechaLimite.Date - datos.FechaGeneracion.Date).TotalDays;

                hoja.Cell(fila, 1).Value = meta.Nombre;
                hoja.Cell(fila, 2).Value = CalculosReporte.NombrePrioridad(meta.Prioridad);
                hoja.Cell(fila, 3).Value = meta.MontoObjetivo;
                hoja.Cell(fila, 4).Value = meta.MontoActual;
                hoja.Cell(fila, 5).Value = falta;
                hoja.Cell(fila, 6).Value = progreso > 1 ? 1 : progreso;
                hoja.Cell(fila, 7).Value = meta.FechaLimite;
                hoja.Cell(fila, 8).Value = dias;
                fila = fila + 1;
            }

            if (fila == 2)
            {
                SinRegistros(hoja, columnas.Length, "Aún no hay metas registradas.");
                return;
            }

            FormatoColumna(hoja, 3, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 4, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 5, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 6, fila - 1, FormatoPorcentaje);
            FormatoColumna(hoja, 7, fila - 1, FormatoFecha);
            FormatoColumna(hoja, 8, fila - 1, "0;[Red]-0");
            hoja.Range(2, 6, fila - 1, 6).AddConditionalFormat().DataBar(ColorAcento).Minimum(XLCFContentType.Number, 0).Maximum(XLCFContentType.Number, 1);

            IXLTable tabla = CrearTabla(hoja, "TablaMetas", fila - 1, columnas.Length);
            tabla.Field("Meta").TotalsRowLabel = "Total";
            tabla.Field("Objetivo").TotalsRowFunction = XLTotalsRowFunction.Sum;
            tabla.Field("Ahorrado").TotalsRowFunction = XLTotalsRowFunction.Sum;
            tabla.Field("Falta").TotalsRowFunction = XLTotalsRowFunction.Sum;
            hoja.Cell(fila, 3).Style.NumberFormat.Format = FormatoMoneda;
            hoja.Cell(fila, 4).Style.NumberFormat.Format = FormatoMoneda;
            hoja.Cell(fila, 5).Style.NumberFormat.Format = FormatoMoneda;
        }

        private void CrearDeudas(XLWorkbook libro)
        {
            string[] columnas = { "Acreedor", "Capital inicial", "Saldo pendiente", "Pagado", "Tasa", "Cuota mínima", "Vencimiento" };
            double[] anchos = { 30, 17, 17, 12, 10, 16, 14 };
            IXLWorksheet hoja = CrearHoja(libro, "Deudas", columnas, anchos);

            int fila = 2;

            foreach (Deuda deuda in datos.Deudas)
            {
                decimal pagado = 0;

                if (deuda.Capital > 0)
                {
                    pagado = (deuda.Capital - deuda.Saldo) / deuda.Capital;
                }

                if (pagado < 0)
                {
                    pagado = 0;
                }

                hoja.Cell(fila, 1).Value = deuda.Acreedor;
                hoja.Cell(fila, 2).Value = deuda.Capital;
                hoja.Cell(fila, 3).Value = deuda.Saldo;
                hoja.Cell(fila, 4).Value = pagado;
                hoja.Cell(fila, 5).Value = deuda.TasaInteres / 100;
                hoja.Cell(fila, 6).Value = deuda.PagoMinimo;
                hoja.Cell(fila, 7).Value = deuda.FechaVencimiento;
                fila = fila + 1;
            }

            if (fila == 2)
            {
                SinRegistros(hoja, columnas.Length, "No hay deudas registradas.");
                return;
            }

            FormatoColumna(hoja, 2, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 3, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 4, fila - 1, FormatoPorcentaje);
            FormatoColumna(hoja, 5, fila - 1, "0.00%");
            FormatoColumna(hoja, 6, fila - 1, FormatoMoneda);
            FormatoColumna(hoja, 7, fila - 1, FormatoFecha);
            hoja.Range(2, 4, fila - 1, 4).AddConditionalFormat().DataBar(ColorAcento).Minimum(XLCFContentType.Number, 0).Maximum(XLCFContentType.Number, 1);

            IXLTable tabla = CrearTabla(hoja, "TablaDeudas", fila - 1, columnas.Length);
            tabla.Field("Acreedor").TotalsRowLabel = "Total";
            tabla.Field("Capital inicial").TotalsRowFunction = XLTotalsRowFunction.Sum;
            tabla.Field("Saldo pendiente").TotalsRowFunction = XLTotalsRowFunction.Sum;
            tabla.Field("Cuota mínima").TotalsRowFunction = XLTotalsRowFunction.Sum;
            hoja.Cell(fila, 2).Style.NumberFormat.Format = FormatoMoneda;
            hoja.Cell(fila, 3).Style.NumberFormat.Format = FormatoMoneda;
            hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoMoneda;
        }

        // ---------- Utilidades ----------

        private static IXLWorksheet CrearHoja(XLWorkbook libro, string nombre, string[] columnas, double[] anchos)
        {
            IXLWorksheet hoja = libro.AddWorksheet(nombre);
            hoja.SetTabColor(ColorMarca);

            for (int indice = 0; indice < columnas.Length; indice = indice + 1)
            {
                hoja.Cell(1, indice + 1).Value = columnas[indice];
                hoja.Column(indice + 1).Width = anchos[indice];
            }

            IXLRange encabezado = hoja.Range(1, 1, 1, columnas.Length);
            encabezado.Style.Font.Bold = true;
            encabezado.Style.Font.FontColor = XLColor.White;
            encabezado.Style.Fill.BackgroundColor = ColorMarca;
            encabezado.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            hoja.Row(1).Height = 24;
            hoja.SheetView.FreezeRows(1);

            ConfigurarImpresion(hoja, true);
            return hoja;
        }

        private static IXLTable CrearTabla(IXLWorksheet hoja, string nombre, int ultimaFila, int columnas)
        {
            IXLTable tabla = hoja.Range(1, 1, ultimaFila, columnas).CreateTable(nombre);
            tabla.Theme = XLTableTheme.TableStyleLight1;
            tabla.ShowRowStripes = true;
            tabla.ShowAutoFilter = true;
            tabla.ShowTotalsRow = true;

            IXLRangeRow totales = tabla.TotalsRow();
            totales.Style.Font.Bold = true;
            totales.Style.Fill.BackgroundColor = ColorSuave;
            totales.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            totales.Style.Border.TopBorderColor = ColorMarca;

            IXLRangeRow encabezado = tabla.HeadersRow();
            encabezado.Style.Font.Bold = true;
            encabezado.Style.Font.FontColor = XLColor.White;
            encabezado.Style.Fill.BackgroundColor = ColorMarca;
            return tabla;
        }

        private static void FormatoColumna(IXLWorksheet hoja, int columna, int ultimaFila, string formato)
        {
            hoja.Range(2, columna, ultimaFila, columna).Style.NumberFormat.Format = formato;
        }

        private static void SinRegistros(IXLWorksheet hoja, int columnas, string mensaje)
        {
            IXLRange rango = hoja.Range(2, 1, 2, columnas);
            rango.Merge();
            rango.Value = mensaje;
            rango.Style.Font.Italic = true;
            rango.Style.Font.FontColor = ColorTextoSecundario;
            rango.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Row(2).Height = 26;
        }

        private static void ConfigurarImpresion(IXLWorksheet hoja, bool horizontal)
        {
            hoja.PageSetup.PageOrientation = horizontal ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
            hoja.PageSetup.PaperSize = XLPaperSize.A4Paper;
            hoja.PageSetup.FitToPages(1, 0);
            hoja.PageSetup.Margins.Left = 0.4;
            hoja.PageSetup.Margins.Right = 0.4;
            hoja.PageSetup.Footer.Center.AddText("FinOptima · Página &P de &N");

            if (horizontal)
            {
                hoja.PageSetup.SetRowsToRepeatAtTop(1, 1);
            }
        }
    }
}
