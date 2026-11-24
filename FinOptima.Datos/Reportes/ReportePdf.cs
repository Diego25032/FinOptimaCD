using System;
using System.Collections.Generic;
using System.Globalization;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Arma el PDF del reporte financiero: portada con indicadores, sobres, graficos,
    /// metas, deudas y movimientos recientes.
    /// </summary>
    internal class ReportePdf
    {
        private const int MaximoMovimientos = 40;
        private const double AltoFila = 22;

        private DatosReporte datos;
        private CalculosReporte calculos;
        private LienzoPdf lienzo;

        public ReportePdf(DatosReporte datosReporte)
        {
            datos = datosReporte;
            calculos = new CalculosReporte(datosReporte, 6);
        }

        public void Generar(string ruta)
        {
            ResolvedorFuentes.Registrar();

            PdfDocument documento = new PdfDocument();
            documento.Info.Title = "FinOptima - Reporte financiero";
            documento.Info.Author = "FinOptima";
            documento.Info.Subject = "Reporte financiero de " + datos.NombreUsuario;
            documento.Info.Creator = "FinOptima";

            string fecha = CalculosReporte.FechaCorta(datos.FechaGeneracion);
            lienzo = new LienzoPdf(documento, datos.NombreUsuario, fecha);

            DibujarPortada();
            DibujarSobres();
            DibujarGraficos();
            DibujarMetas();
            DibujarDeudas();
            DibujarMovimientos();

            lienzo.Guardar(ruta);
        }

        // ---------- Portada ----------

        private void DibujarPortada()
        {
            double ancho = lienzo.Ancho;
            double x = LienzoPdf.MargenX;

            lienzo.Rectangulo(0, 0, ancho, 152, PaletaReporte.Marca);
            lienzo.Dibujo.DrawEllipse(new XSolidBrush(PaletaReporte.MarcaClara), ancho - 170, -150, 300, 300);

            LogoPdf.Dibujar(lienzo.Dibujo, x, 34, 34, PaletaReporte.Acento, PaletaReporte.Marca);
            lienzo.Texto("FinOptima", Fuentes.Negrita(22), PaletaReporte.TextoSobreMarca, x + 46, 30);
            lienzo.Texto("Reporte financiero personal  ·  Método de los 7 sobres", Fuentes.Normal(9), PaletaReporte.TextoSuaveSobreMarca, x + 47, 58);

            double anchoDerecha = 220;
            double xDerecha = ancho - LienzoPdf.MargenX - anchoDerecha;
            lienzo.TextoDerecha("PREPARADO PARA", Fuentes.Negrita(7), PaletaReporte.TextoSuaveSobreMarca, xDerecha, 32, anchoDerecha);
            lienzo.TextoDerecha(datos.NombreUsuario, Fuentes.Negrita(13), PaletaReporte.TextoSobreMarca, xDerecha, 43, anchoDerecha);
            lienzo.TextoDerecha("Generado el " + CalculosReporte.FechaLarga(datos.FechaGeneracion) + "  ·  " + datos.FechaGeneracion.ToString("HH:mm"),
                Fuentes.Normal(8), PaletaReporte.TextoSuaveSobreMarca, xDerecha, 62, anchoDerecha);

            double yTarjetas = 108;
            double espacio = 10;
            double anchoTarjeta = (lienzo.AnchoUtil - espacio * 3) / 4;

            XColor colorDisponible = calculos.Disponible >= 0 ? PaletaReporte.Info : PaletaReporte.Peligro;

            Indicador(x, yTarjetas, anchoTarjeta, "INGRESOS", CalculosReporte.Moneda(calculos.Ingresos), "Total registrado", PaletaReporte.Acento);
            Indicador(x + (anchoTarjeta + espacio), yTarjetas, anchoTarjeta, "GASTOS", CalculosReporte.Moneda(calculos.Gastos), "Total registrado", PaletaReporte.Peligro);
            Indicador(x + (anchoTarjeta + espacio) * 2, yTarjetas, anchoTarjeta, "DISPONIBLE", CalculosReporte.Moneda(calculos.Disponible), "Ingresos menos gastos", colorDisponible);
            Indicador(x + (anchoTarjeta + espacio) * 3, yTarjetas, anchoTarjeta, "TASA DE AHORRO", CalculosReporte.Porcentaje(calculos.TasaAhorro), "Del total de ingresos", PaletaReporte.Aviso);

            double yResumen = yTarjetas + 78 + 12;
            lienzo.RectanguloRedondeado(x, yResumen, lienzo.AnchoUtil, 40, 8, PaletaReporte.FondoSuave);

            double anchoDato = lienzo.AnchoUtil / 3;
            DatoSecundario(x, yResumen, anchoDato, "Saldo en sobres", CalculosReporte.Moneda(calculos.SaldoSobres));
            DatoSecundario(x + anchoDato, yResumen, anchoDato, "Ahorro en metas", CalculosReporte.Moneda(calculos.AhorroMetas) + " de " + CalculosReporte.Moneda(calculos.ObjetivoMetas));
            DatoSecundario(x + anchoDato * 2, yResumen, anchoDato, "Deudas pendientes", CalculosReporte.Moneda(calculos.SaldoDeudas));
            lienzo.Linea(x + anchoDato, yResumen + 10, x + anchoDato, yResumen + 30, PaletaReporte.Borde, 0.8);
            lienzo.Linea(x + anchoDato * 2, yResumen + 10, x + anchoDato * 2, yResumen + 30, PaletaReporte.Borde, 0.8);

            lienzo.Y = yResumen + 40 + 26;
        }

        private void Indicador(double x, double y, double ancho, string titulo, string valor, string nota, XColor color)
        {
            lienzo.Tarjeta(x, y, ancho, 78);
            lienzo.RectanguloRedondeado(x + 14, y + 14, 22, 4, 2, color);
            lienzo.TextoAjustado(titulo, Fuentes.Negrita(7), PaletaReporte.TextoSecundario, x + 14, y + 24, ancho - 28);
            lienzo.TextoQueQuepa(valor, true, 14, 9, PaletaReporte.Texto, x + 14, y + 36, ancho - 28);
            lienzo.TextoAjustado(nota, Fuentes.Normal(7), PaletaReporte.TextoSecundario, x + 14, y + 58, ancho - 28);
        }

        private void DatoSecundario(double x, double y, double ancho, string titulo, string valor)
        {
            lienzo.TextoAjustado(titulo, Fuentes.Normal(7.5), PaletaReporte.TextoSecundario, x + 16, y + 8, ancho - 32);
            lienzo.TextoQueQuepa(valor, true, 10, 7.5, PaletaReporte.Texto, x + 16, y + 20, ancho - 32);
        }

        // ---------- Secciones ----------

        private void TituloSeccion(string titulo, string detalle, double altoMinimo)
        {
            lienzo.AsegurarEspacio(28 + altoMinimo);

            double x = LienzoPdf.MargenX;
            lienzo.RectanguloRedondeado(x, lienzo.Y + 2, 4, 14, 2, PaletaReporte.Acento);
            lienzo.Texto(titulo, Fuentes.Negrita(12.5), PaletaReporte.Texto, x + 12, lienzo.Y);

            if (detalle.Length > 0)
            {
                lienzo.TextoDerecha(detalle, Fuentes.Normal(8), PaletaReporte.TextoSecundario, x, lienzo.Y + 4, lienzo.AnchoUtil);
            }

            lienzo.Y = lienzo.Y + 26;
        }

        private void EstadoVacio(string mensaje)
        {
            double x = LienzoPdf.MargenX;
            lienzo.RectanguloRedondeado(x, lienzo.Y, lienzo.AnchoUtil, 36, 8, PaletaReporte.FondoSuave);
            lienzo.TextoCentrado(mensaje, Fuentes.Normal(8.5), PaletaReporte.TextoSecundario, x, lienzo.Y, lienzo.AnchoUtil, 36);
            lienzo.Y = lienzo.Y + 36 + 26;
        }

        private void EncabezadoTabla(string[] titulos, double[] anchos, int[] alineaciones)
        {
            double x = LienzoPdf.MargenX;
            XFont fuente = Fuentes.Negrita(7);
            lienzo.RectanguloRedondeado(x, lienzo.Y, lienzo.AnchoUtil, 20, 6, PaletaReporte.FondoSuave);

            double cursor = x;

            for (int indice = 0; indice < titulos.Length; indice = indice + 1)
            {
                CeldaTexto(titulos[indice].ToUpper(CultureInfo.CurrentCulture), fuente, PaletaReporte.TextoSecundario, cursor, lienzo.Y + 6.5, anchos[indice], alineaciones[indice]);
                cursor = cursor + anchos[indice];
            }

            lienzo.Y = lienzo.Y + 24;
        }

        private void CeldaTexto(string texto, XFont fuente, XColor color, double x, double y, double ancho, int alineacion)
        {
            double interior = ancho - 16;

            if (alineacion == 1)
            {
                lienzo.TextoDerecha(texto, fuente, color, x + 8, y, interior);
            }
            else if (alineacion == 2)
            {
                lienzo.TextoCentrado(texto, fuente, color, x + 8, y, interior, fuente.GetHeight());
            }
            else
            {
                lienzo.TextoAjustado(texto, fuente, color, x + 8, y, interior);
            }
        }

        private bool NuevaFila(string[] titulos, double[] anchos, int[] alineaciones, double alto)
        {
            bool saltoPagina = lienzo.AsegurarEspacio(alto);

            if (saltoPagina)
            {
                EncabezadoTabla(titulos, anchos, alineaciones);
            }

            return saltoPagina;
        }

        private void SeparadorFila(double alto)
        {
            double x = LienzoPdf.MargenX;
            lienzo.Linea(x + 6, lienzo.Y + alto, x + lienzo.AnchoUtil - 6, lienzo.Y + alto, PaletaReporte.Borde, 0.5);
            lienzo.Y = lienzo.Y + alto;
        }

        // ---------- Sobres ----------

        private void DibujarSobres()
        {
            int activos = 0;

            foreach (Sobre sobre in datos.Sobres)
            {
                if (sobre.Activo)
                {
                    activos = activos + 1;
                }
            }

            TituloSeccion("Distribución de sobres", activos + " sobres activos", 80);

            if (activos == 0)
            {
                EstadoVacio("Todavía no hay sobres activos.");
                return;
            }

            string[] titulos = { "Sobre", "Objetivo", "Rango", "Saldo", "Participación del saldo" };
            double[] anchos = { 135, 60, 80, 95, lienzo.AnchoUtil - 370 };
            int[] alineaciones = { 0, 1, 2, 1, 0 };
            EncabezadoTabla(titulos, anchos, alineaciones);

            decimal totalPositivo = 0;

            foreach (Sobre sobre in datos.Sobres)
            {
                if (sobre.Activo && sobre.Saldo > 0)
                {
                    totalPositivo = totalPositivo + sobre.Saldo;
                }
            }

            int posicion = 0;
            XFont normal = Fuentes.Normal(8.5);
            XFont negrita = Fuentes.Negrita(8.5);

            foreach (Sobre sobre in datos.Sobres)
            {
                if (!sobre.Activo)
                {
                    continue;
                }

                NuevaFila(titulos, anchos, alineaciones, AltoFila);

                XColor color = PaletaReporte.ColorSerie(posicion);
                double x = LienzoPdf.MargenX;
                double yTexto = lienzo.Y + 6.5;

                lienzo.Circulo(x + 9, lienzo.Y + 8, 7, color);
                lienzo.TextoAjustado(sobre.Nombre, negrita, PaletaReporte.Texto, x + 22, yTexto, anchos[0] - 28);
                x = x + anchos[0];

                CeldaTexto(sobre.PorcentajeObjetivo.ToString("0.#") + "%", normal, PaletaReporte.Texto, x, yTexto, anchos[1], 1);
                x = x + anchos[1];

                CeldaTexto(sobre.PorcentajeMinimo.ToString("0.#") + "% – " + sobre.PorcentajeMaximo.ToString("0.#") + "%", normal, PaletaReporte.TextoSecundario, x, yTexto, anchos[2], 2);
                x = x + anchos[2];

                XColor colorSaldo = sobre.Saldo < 0 ? PaletaReporte.Peligro : PaletaReporte.Texto;
                CeldaTexto(CalculosReporte.Moneda(sobre.Saldo), negrita, colorSaldo, x, yTexto, anchos[3], 1);
                x = x + anchos[3];

                decimal participacion = 0;

                if (totalPositivo > 0 && sobre.Saldo > 0)
                {
                    participacion = sobre.Saldo / totalPositivo;
                }

                double anchoBarra = anchos[4] - 60;
                lienzo.Barra(x + 8, lienzo.Y + 8, anchoBarra, 6, participacion, color);
                lienzo.TextoDerecha(CalculosReporte.Porcentaje(participacion), normal, PaletaReporte.TextoSecundario, x + anchoBarra + 10, yTexto, 42);

                SeparadorFila(AltoFila);
                posicion = posicion + 1;
            }

            NuevaFila(titulos, anchos, alineaciones, AltoFila);
            double xTotal = LienzoPdf.MargenX;
            lienzo.TextoAjustado("Total en sobres", negrita, PaletaReporte.Texto, xTotal + 22, lienzo.Y + 7, 200);
            CeldaTexto(CalculosReporte.Moneda(calculos.SaldoSobres), Fuentes.Negrita(9), PaletaReporte.Texto, xTotal + anchos[0] + anchos[1] + anchos[2], lienzo.Y + 7, anchos[3], 1);
            lienzo.Y = lienzo.Y + AltoFila + 24;
        }

        // ---------- Graficos ----------

        private void DibujarGraficos()
        {
            double alto = 206;
            TituloSeccion("Análisis de gastos", "", alto);

            double espacio = 14;
            double ancho = (lienzo.AnchoUtil - espacio) / 2;
            double x = LienzoPdf.MargenX;
            double y = lienzo.Y;

            lienzo.Tarjeta(x, y, ancho, alto);
            lienzo.Tarjeta(x + ancho + espacio, y, ancho, alto);

            DibujarDona(x, y, ancho, alto);
            DibujarBarrasMensuales(x + ancho + espacio, y, ancho, alto);

            lienzo.Y = y + alto + 26;
        }

        private void DibujarDona(double x, double y, double ancho, double alto)
        {
            lienzo.Texto("Gastos por categoría", Fuentes.Negrita(10), PaletaReporte.Texto, x + 16, y + 14);
            lienzo.Texto("Reparto del total de gastos", Fuentes.Normal(7.5), PaletaReporte.TextoSecundario, x + 16, y + 29);

            double diametro = 112;
            double xDona = x + 16;
            double yDona = y + 58;

            if (calculos.GastosPorCategoria.Count == 0 || calculos.Gastos <= 0)
            {
                lienzo.Circulo(xDona, yDona, diametro, PaletaReporte.Pista);
                lienzo.Circulo(xDona + 24, yDona + 24, diametro - 48, PaletaReporte.Blanco);
                lienzo.TextoCentrado("Sin gastos", Fuentes.Normal(8), PaletaReporte.TextoSecundario, xDona, yDona, diametro, diametro);
                return;
            }

            List<TotalCategoria> visibles = AgruparCategorias(5);
            double inicio = -90;

            for (int indice = 0; indice < visibles.Count; indice = indice + 1)
            {
                double barrido = 360.0 * (double)visibles[indice].Porcentaje;

                if (barrido <= 0)
                {
                    continue;
                }

                if (barrido >= 359.9)
                {
                    lienzo.Circulo(xDona, yDona, diametro, PaletaReporte.ColorSerie(indice));
                }
                else
                {
                    lienzo.Sector(xDona, yDona, diametro, inicio, barrido, PaletaReporte.ColorSerie(indice));
                }

                inicio = inicio + barrido;
            }

            double hueco = diametro - 44;
            lienzo.Circulo(xDona + 22, yDona + 22, hueco, PaletaReporte.Blanco);
            lienzo.TextoCentrado("Total", Fuentes.Normal(7), PaletaReporte.TextoSecundario, xDona + 22, yDona + 22 + hueco / 2 - 13, hueco, 10);
            lienzo.TextoCentrado(Compacto(calculos.Gastos), Fuentes.Negrita(10), PaletaReporte.Texto, xDona + 22, yDona + 22 + hueco / 2 - 2, hueco, 14);

            double xLeyenda = xDona + diametro + 16;
            double anchoLeyenda = x + ancho - 14 - xLeyenda;
            double yLeyenda = yDona + 4;

            for (int indice = 0; indice < visibles.Count; indice = indice + 1)
            {
                TotalCategoria categoria = visibles[indice];
                double yFila = yLeyenda + indice * 18;
                lienzo.Circulo(xLeyenda, yFila + 2.5, 6, PaletaReporte.ColorSerie(indice));
                lienzo.TextoAjustado(categoria.Categoria, Fuentes.Normal(7.5), PaletaReporte.Texto, xLeyenda + 10, yFila, anchoLeyenda - 44);
                lienzo.TextoDerecha(CalculosReporte.Porcentaje(categoria.Porcentaje), Fuentes.Negrita(7.5), PaletaReporte.Texto, xLeyenda, yFila, anchoLeyenda);
            }
        }

        private List<TotalCategoria> AgruparCategorias(int maximo)
        {
            List<TotalCategoria> resultado = new List<TotalCategoria>();
            TotalCategoria otras = new TotalCategoria();
            otras.Categoria = "Otras";

            for (int indice = 0; indice < calculos.GastosPorCategoria.Count; indice = indice + 1)
            {
                TotalCategoria categoria = calculos.GastosPorCategoria[indice];

                if (indice < maximo)
                {
                    resultado.Add(categoria);
                }
                else
                {
                    otras.Monto = otras.Monto + categoria.Monto;
                    otras.Porcentaje = otras.Porcentaje + categoria.Porcentaje;
                }
            }

            if (otras.Monto > 0)
            {
                resultado.Add(otras);
            }

            return resultado;
        }

        private void DibujarBarrasMensuales(double x, double y, double ancho, double alto)
        {
            lienzo.Texto("Ingresos vs. gastos", Fuentes.Negrita(10), PaletaReporte.Texto, x + 16, y + 14);
            lienzo.Texto("Últimos 6 meses", Fuentes.Normal(7.5), PaletaReporte.TextoSecundario, x + 16, y + 29);

            double xLeyenda = x + ancho - 16 - 100;
            lienzo.RectanguloRedondeado(xLeyenda, y + 31, 7, 7, 2, PaletaReporte.Acento);
            lienzo.Texto("Ingresos", Fuentes.Normal(7), PaletaReporte.TextoSecundario, xLeyenda + 10, y + 30);
            lienzo.RectanguloRedondeado(xLeyenda + 54, y + 31, 7, 7, 2, PaletaReporte.Peligro);
            lienzo.Texto("Gastos", Fuentes.Normal(7), PaletaReporte.TextoSecundario, xLeyenda + 64, y + 30);

            decimal maximo = 0;

            foreach (TotalMes mes in calculos.UltimosMeses)
            {
                if (mes.Ingresos > maximo)
                {
                    maximo = mes.Ingresos;
                }

                if (mes.Gastos > maximo)
                {
                    maximo = mes.Gastos;
                }
            }

            double xGrafico = x + 46;
            double anchoGrafico = ancho - 46 - 16;
            double yGrafico = y + 56;
            double altoGrafico = alto - 56 - 34;

            for (int linea = 0; linea <= 3; linea = linea + 1)
            {
                double yLinea = yGrafico + altoGrafico - altoGrafico * linea / 3.0;
                lienzo.Linea(xGrafico, yLinea, xGrafico + anchoGrafico, yLinea, PaletaReporte.Pista, 0.7);

                if (maximo <= 0 && linea > 0)
                {
                    continue;
                }

                decimal valor = maximo * linea / 3;
                lienzo.TextoDerecha(Compacto(valor), Fuentes.Normal(6.5), PaletaReporte.TextoSecundario, x + 8, yLinea - 4, 34);
            }

            int cantidad = calculos.UltimosMeses.Count;

            if (cantidad == 0)
            {
                return;
            }

            double anchoGrupo = anchoGrafico / cantidad;
            double anchoBarra = anchoGrupo * 0.28;

            for (int indice = 0; indice < cantidad; indice = indice + 1)
            {
                TotalMes mes = calculos.UltimosMeses[indice];
                double centro = xGrafico + anchoGrupo * indice + anchoGrupo / 2;

                DibujarBarraVertical(centro - anchoBarra - 1, yGrafico, altoGrafico, anchoBarra, mes.Ingresos, maximo, PaletaReporte.Acento);
                DibujarBarraVertical(centro + 1, yGrafico, altoGrafico, anchoBarra, mes.Gastos, maximo, PaletaReporte.Peligro);

                lienzo.TextoCentrado(mes.Etiqueta, Fuentes.Normal(7), PaletaReporte.TextoSecundario, centro - anchoGrupo / 2, yGrafico + altoGrafico + 6, anchoGrupo, 10);
            }
        }

        private void DibujarBarraVertical(double x, double yGrafico, double altoGrafico, double ancho, decimal valor, decimal maximo, XColor color)
        {
            if (maximo <= 0 || valor <= 0)
            {
                return;
            }

            double alto = altoGrafico * (double)(valor / maximo);

            if (alto < 2)
            {
                alto = 2;
            }

            lienzo.RectanguloRedondeado(x, yGrafico + altoGrafico - alto, ancho, alto, 2, color);
        }

        // ---------- Metas ----------

        private void DibujarMetas()
        {
            TituloSeccion("Metas de ahorro", datos.Metas.Count + (datos.Metas.Count == 1 ? " meta" : " metas"), 60);

            if (datos.Metas.Count == 0)
            {
                EstadoVacio("Aún no hay metas registradas. Crea una desde el módulo Metas.");
                return;
            }

            string[] titulos = { "Meta", "Prioridad", "Ahorrado / Objetivo", "Progreso", "Fecha límite" };
            double[] anchos = { 140, 62, 135, 98, lienzo.AnchoUtil - 435 };
            int[] alineaciones = { 0, 0, 1, 0, 1 };
            EncabezadoTabla(titulos, anchos, alineaciones);

            XFont normal = Fuentes.Normal(8.5);
            XFont negrita = Fuentes.Negrita(8.5);

            foreach (Meta meta in datos.Metas)
            {
                NuevaFila(titulos, anchos, alineaciones, AltoFila);

                double x = LienzoPdf.MargenX;
                double yTexto = lienzo.Y + 6.5;

                CeldaTexto(meta.Nombre, negrita, PaletaReporte.Texto, x, yTexto, anchos[0], 0);
                x = x + anchos[0];

                XColor fondo = PaletaReporte.InfoSuave;
                XColor color = PaletaReporte.Info;

                if (meta.Prioridad == PrioridadMeta.Alta)
                {
                    fondo = PaletaReporte.PeligroSuave;
                    color = PaletaReporte.Peligro;
                }
                else if (meta.Prioridad == PrioridadMeta.Media)
                {
                    fondo = PaletaReporte.AvisoSuave;
                    color = XColor.FromArgb(180, 110, 0);
                }

                lienzo.Etiqueta(CalculosReporte.NombrePrioridad(meta.Prioridad), x + 8, lienzo.Y + 4.5, fondo, color);
                x = x + anchos[1];

                CeldaTexto(CalculosReporte.Moneda(meta.MontoActual) + " / " + CalculosReporte.Moneda(meta.MontoObjetivo), normal, PaletaReporte.Texto, x, yTexto, anchos[2], 1);
                x = x + anchos[2];

                decimal progreso = 0;

                if (meta.MontoObjetivo > 0)
                {
                    progreso = meta.MontoActual / meta.MontoObjetivo;
                }

                XColor colorBarra = progreso >= 1 ? PaletaReporte.Acento : PaletaReporte.Info;
                lienzo.Barra(x + 8, lienzo.Y + 8, anchos[3] - 50, 6, progreso, colorBarra);
                lienzo.TextoDerecha(CalculosReporte.Porcentaje(progreso > 1 ? 1 : progreso), Fuentes.Negrita(8), PaletaReporte.Texto, x + anchos[3] - 42, yTexto, 36);
                x = x + anchos[3];

                CeldaTexto(CalculosReporte.FechaCorta(meta.FechaLimite), normal, PaletaReporte.TextoSecundario, x, yTexto, anchos[4], 1);

                SeparadorFila(AltoFila);
            }

            lienzo.Y = lienzo.Y + 24;
        }

        // ---------- Deudas ----------

        private void DibujarDeudas()
        {
            TituloSeccion("Deudas", "Pendiente: " + CalculosReporte.Moneda(calculos.SaldoDeudas), 60);

            if (datos.Deudas.Count == 0)
            {
                EstadoVacio("No hay deudas registradas. ¡Excelente!");
                return;
            }

            string[] titulos = { "Acreedor", "Saldo", "Tasa", "Cuota mínima", "Vence", "Pagado" };
            double[] anchos = { 140, 86, 50, 80, 76, lienzo.AnchoUtil - 432 };
            int[] alineaciones = { 0, 1, 1, 1, 1, 0 };
            EncabezadoTabla(titulos, anchos, alineaciones);

            XFont normal = Fuentes.Normal(8.5);
            XFont negrita = Fuentes.Negrita(8.5);

            foreach (Deuda deuda in datos.Deudas)
            {
                NuevaFila(titulos, anchos, alineaciones, AltoFila);

                double x = LienzoPdf.MargenX;
                double yTexto = lienzo.Y + 6.5;

                CeldaTexto(deuda.Acreedor, negrita, PaletaReporte.Texto, x, yTexto, anchos[0], 0);
                x = x + anchos[0];
                CeldaTexto(CalculosReporte.Moneda(deuda.Saldo), negrita, PaletaReporte.Peligro, x, yTexto, anchos[1], 1);
                x = x + anchos[1];
                CeldaTexto(deuda.TasaInteres.ToString("0.##") + "%", normal, PaletaReporte.Texto, x, yTexto, anchos[2], 1);
                x = x + anchos[2];
                CeldaTexto(CalculosReporte.Moneda(deuda.PagoMinimo), normal, PaletaReporte.Texto, x, yTexto, anchos[3], 1);
                x = x + anchos[3];
                CeldaTexto(CalculosReporte.FechaCorta(deuda.FechaVencimiento), normal, PaletaReporte.TextoSecundario, x, yTexto, anchos[4], 1);
                x = x + anchos[4];

                decimal pagado = 0;

                if (deuda.Capital > 0)
                {
                    pagado = (deuda.Capital - deuda.Saldo) / deuda.Capital;
                }

                if (pagado < 0)
                {
                    pagado = 0;
                }

                lienzo.Barra(x + 8, lienzo.Y + 8, anchos[5] - 52, 6, pagado, PaletaReporte.Acento);
                lienzo.TextoDerecha(CalculosReporte.Porcentaje(pagado), Fuentes.Negrita(8), PaletaReporte.Texto, x + anchos[5] - 44, yTexto, 38);

                SeparadorFila(AltoFila);
            }

            lienzo.Y = lienzo.Y + 24;
        }

        // ---------- Movimientos ----------

        private void DibujarMovimientos()
        {
            int total = calculos.MovimientosOrdenados.Count;
            string detalle = total > MaximoMovimientos
                ? "Los " + MaximoMovimientos + " más recientes de " + total
                : total + (total == 1 ? " movimiento" : " movimientos");

            TituloSeccion("Movimientos recientes", detalle, 60);

            if (total == 0)
            {
                EstadoVacio("Aún no hay movimientos registrados.");
                return;
            }

            string[] titulos = { "Fecha", "Descripción", "Sobre", "Tipo", "Monto" };
            double[] anchos = { 74, 197, 92, 62, lienzo.AnchoUtil - 425 };
            int[] alineaciones = { 0, 0, 0, 0, 1 };
            EncabezadoTabla(titulos, anchos, alineaciones);

            XFont normal = Fuentes.Normal(8);
            XFont negrita = Fuentes.Negrita(8.5);
            XFont pequena = Fuentes.Normal(6.5);
            double altoFila = 26;
            int mostrados = 0;

            foreach (Movimiento movimiento in calculos.MovimientosOrdenados)
            {
                if (mostrados >= MaximoMovimientos)
                {
                    break;
                }

                NuevaFila(titulos, anchos, alineaciones, altoFila);

                double x = LienzoPdf.MargenX;
                bool esIngreso = movimiento.Tipo == TipoMovimiento.Ingreso;

                CeldaTexto(CalculosReporte.FechaCorta(movimiento.Fecha), normal, PaletaReporte.TextoSecundario, x, lienzo.Y + 8.5, anchos[0], 0);
                x = x + anchos[0];

                CeldaTexto(movimiento.Descripcion, Fuentes.Negrita(8), PaletaReporte.Texto, x, lienzo.Y + 4, anchos[1], 0);
                CeldaTexto(movimiento.Categoria, pequena, PaletaReporte.TextoSecundario, x, lienzo.Y + 14.5, anchos[1], 0);
                x = x + anchos[1];

                CeldaTexto(calculos.NombreSobre(movimiento.SobreId), normal, PaletaReporte.Texto, x, lienzo.Y + 8.5, anchos[2], 0);
                x = x + anchos[2];

                if (esIngreso)
                {
                    lienzo.Etiqueta("Ingreso", x + 8, lienzo.Y + 6.5, PaletaReporte.AcentoSuave, XColor.FromArgb(10, 130, 62));
                }
                else
                {
                    lienzo.Etiqueta("Gasto", x + 8, lienzo.Y + 6.5, PaletaReporte.PeligroSuave, PaletaReporte.Peligro);
                }

                x = x + anchos[3];

                string monto = (esIngreso ? "+ " : "− ") + CalculosReporte.Moneda(movimiento.Monto);
                XColor colorMonto = esIngreso ? XColor.FromArgb(10, 130, 62) : PaletaReporte.Peligro;
                CeldaTexto(monto, negrita, colorMonto, x, lienzo.Y + 8, anchos[4], 1);

                SeparadorFila(altoFila);
                mostrados = mostrados + 1;
            }
        }

        // ---------- Formato ----------

        private static string Compacto(decimal valor)
        {
            decimal absoluto = Math.Abs(valor);
            string signo = valor < 0 ? "-" : "";

            if (absoluto >= 1000000000)
            {
                return signo + "$" + (absoluto / 1000000000).ToString("0.#") + " mil M";
            }

            if (absoluto >= 1000000)
            {
                return signo + "$" + (absoluto / 1000000).ToString("0.#") + " M";
            }

            if (absoluto >= 1000)
            {
                return signo + "$" + (absoluto / 1000).ToString("0") + " mil";
            }

            return signo + "$" + absoluto.ToString("0");
        }
    }
}
