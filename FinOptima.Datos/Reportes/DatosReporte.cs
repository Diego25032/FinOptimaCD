using System;
using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Agrupa todo lo que necesita un reporte para no pasar seis parametros sueltos.
    /// </summary>
    public class DatosReporte
    {
        public string NombreUsuario { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public List<Sobre> Sobres { get; set; }
        public List<Movimiento> Movimientos { get; set; }
        public List<Meta> Metas { get; set; }
        public List<Deuda> Deudas { get; set; }

        public DatosReporte()
        {
            NombreUsuario = "";
            FechaGeneracion = DateTime.Now;
            Sobres = new List<Sobre>();
            Movimientos = new List<Movimiento>();
            Metas = new List<Meta>();
            Deudas = new List<Deuda>();
        }
    }
}
