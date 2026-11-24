using System;
using System.Collections.Generic;
using System.Globalization;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Totales y agrupaciones que comparten el reporte de Excel y el PDF.
    /// </summary>
    public class CalculosReporte
    {
        private static readonly CultureInfo Espanol = new CultureInfo("es-ES");

        public decimal Ingresos { get; private set; }
        public decimal Gastos { get; private set; }
        public decimal SaldoSobres { get; private set; }
        public decimal AhorroMetas { get; private set; }
        public decimal ObjetivoMetas { get; private set; }
        public decimal SaldoDeudas { get; private set; }
        public List<TotalCategoria> GastosPorCategoria { get; private set; }
        public List<TotalMes> UltimosMeses { get; private set; }
        public List<Movimiento> MovimientosOrdenados { get; private set; }
        public Dictionary<int, string> NombresSobres { get; private set; }

        public decimal Disponible
        {
            get { return Ingresos - Gastos; }
        }

        public decimal TasaAhorro
        {
            get
            {
                if (Ingresos <= 0)
                {
                    return 0;
                }

                return (Ingresos - Gastos) / Ingresos;
            }
        }

        public CalculosReporte(DatosReporte datos, int cantidadMeses)
        {
            GastosPorCategoria = new List<TotalCategoria>();
            UltimosMeses = new List<TotalMes>();
            MovimientosOrdenados = new List<Movimiento>();
            NombresSobres = new Dictionary<int, string>();

            foreach (Sobre sobre in datos.Sobres)
            {
                NombresSobres[sobre.Id] = sobre.Nombre;

                if (sobre.Activo)
                {
                    SaldoSobres = SaldoSobres + sobre.Saldo;
                }
            }

            foreach (Meta meta in datos.Metas)
            {
                AhorroMetas = AhorroMetas + meta.MontoActual;
                ObjetivoMetas = ObjetivoMetas + meta.MontoObjetivo;
            }

            foreach (Deuda deuda in datos.Deudas)
            {
                SaldoDeudas = SaldoDeudas + deuda.Saldo;
            }

            Dictionary<string, TotalCategoria> categorias = new Dictionary<string, TotalCategoria>();

            foreach (Movimiento movimiento in datos.Movimientos)
            {
                MovimientosOrdenados.Add(movimiento);

                if (movimiento.Tipo == TipoMovimiento.Ingreso)
                {
                    Ingresos = Ingresos + movimiento.Monto;
                    continue;
                }

                Gastos = Gastos + movimiento.Monto;

                string nombre = TextoCategoria(movimiento.Categoria);
                string clave = nombre.ToLowerInvariant();
                TotalCategoria total;

                if (!categorias.TryGetValue(clave, out total))
                {
                    total = new TotalCategoria();
                    total.Categoria = nombre;
                    categorias[clave] = total;
                    GastosPorCategoria.Add(total);
                }

                total.Monto = total.Monto + movimiento.Monto;
                total.Cantidad = total.Cantidad + 1;
            }

            foreach (TotalCategoria total in GastosPorCategoria)
            {
                if (Gastos > 0)
                {
                    total.Porcentaje = total.Monto / Gastos;
                }
            }

            GastosPorCategoria.Sort(new ComparadorCategorias());
            MovimientosOrdenados.Sort(new ComparadorMovimientos());
            CalcularMeses(datos.Movimientos, datos.FechaGeneracion, cantidadMeses);
        }

        public string NombreSobre(int sobreId)
        {
            string nombre;

            if (NombresSobres.TryGetValue(sobreId, out nombre))
            {
                return nombre;
            }

            return "Sin sobre";
        }

        public static string TipoTexto(TipoMovimiento tipo)
        {
            return tipo == TipoMovimiento.Ingreso ? "Ingreso" : "Gasto";
        }

        public static string NombrePrioridad(PrioridadMeta prioridad)
        {
            if (prioridad == PrioridadMeta.Alta)
            {
                return "Alta";
            }

            if (prioridad == PrioridadMeta.Baja)
            {
                return "Baja";
            }

            return "Media";
        }

        public static string NombreTipoSobre(TipoSobre tipo)
        {
            switch (tipo)
            {
                case TipoSobre.Necesidades:
                    return "Necesidades";
                case TipoSobre.Ahorro:
                    return "Ahorro";
                case TipoSobre.Inversion:
                    return "Inversión";
                case TipoSobre.Educacion:
                    return "Educación";
                case TipoSobre.Ocio:
                    return "Ocio";
                case TipoSobre.Generosidad:
                    return "Generosidad";
                default:
                    return "Metas";
            }
        }

        public static string FechaLarga(DateTime fecha)
        {
            return fecha.ToString("d 'de' MMMM 'de' yyyy", Espanol);
        }

        private static readonly string[] MesesCortos = { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

        public static string FechaCorta(DateTime fecha)
        {
            return fecha.Day.ToString("00") + " " + MesesCortos[fecha.Month - 1] + " " + fecha.Year;
        }

        public static string Moneda(decimal monto)
        {
            return monto.ToString("C0", CultureInfo.CurrentCulture);
        }

        public static string Porcentaje(decimal fraccion)
        {
            return (fraccion * 100).ToString("0.#", CultureInfo.CurrentCulture) + "%";
        }

        private void CalcularMeses(List<Movimiento> movimientos, DateTime referencia, int cantidad)
        {
            DateTime primero = new DateTime(referencia.Year, referencia.Month, 1).AddMonths(-(cantidad - 1));

            for (int indice = 0; indice < cantidad; indice = indice + 1)
            {
                DateTime inicio = primero.AddMonths(indice);
                TotalMes mes = new TotalMes();
                mes.Anio = inicio.Year;
                mes.Mes = inicio.Month;
                string nombre = MesesCortos[inicio.Month - 1];
                mes.Etiqueta = char.ToUpper(nombre[0]) + nombre.Substring(1);
                UltimosMeses.Add(mes);
            }

            foreach (Movimiento movimiento in movimientos)
            {
                foreach (TotalMes mes in UltimosMeses)
                {
                    if (mes.Anio != movimiento.Fecha.Year || mes.Mes != movimiento.Fecha.Month)
                    {
                        continue;
                    }

                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        mes.Ingresos = mes.Ingresos + movimiento.Monto;
                    }
                    else
                    {
                        mes.Gastos = mes.Gastos + movimiento.Monto;
                    }
                }
            }
        }

        private static string TextoCategoria(string categoria)
        {
            if (categoria == null || categoria.Trim().Length == 0)
            {
                return "General";
            }

            string limpio = categoria.Trim();
            return char.ToUpper(limpio[0]) + limpio.Substring(1);
        }

        private class ComparadorCategorias : IComparer<TotalCategoria>
        {
            public int Compare(TotalCategoria x, TotalCategoria y)
            {
                return y.Monto.CompareTo(x.Monto);
            }
        }

        private class ComparadorMovimientos : IComparer<Movimiento>
        {
            public int Compare(Movimiento x, Movimiento y)
            {
                int porFecha = y.Fecha.CompareTo(x.Fecha);

                if (porFecha != 0)
                {
                    return porFecha;
                }

                return y.Id.CompareTo(x.Id);
            }
        }
    }

    public class TotalCategoria
    {
        public string Categoria { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
        public int Cantidad { get; set; }

        public TotalCategoria()
        {
            Categoria = "";
            Monto = 0;
            Porcentaje = 0;
            Cantidad = 0;
        }
    }

    public class TotalMes
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Etiqueta { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }

        public TotalMes()
        {
            Etiqueta = "";
        }
    }
}
