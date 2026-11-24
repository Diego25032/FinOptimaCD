using System;
using System.Collections.Generic;
using FinOptima.Entidades.Enums;

namespace FinOptima.Bot
{
    /// <summary>
    /// Movimiento que el usuario esta armando con los botones o que espera confirmacion.
    /// </summary>
    public class OperacionPendiente
    {
        public TipoMovimiento Tipo { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public DateTime Creada { get; set; }
        public List<AsignacionSobre> Asignaciones { get; set; }
        public EstadoConversacion Estado { get; set; }
        public bool EsSueldo { get; set; }
        public decimal Monto { get; set; }
        public int SobreElegidoId { get; set; }
        public int MensajeId { get; set; }

        public OperacionPendiente()
        {
            Tipo = TipoMovimiento.Gasto;
            Categoria = "";
            Descripcion = "";
            Creada = DateTime.Now;
            Asignaciones = new List<AsignacionSobre>();
            Estado = EstadoConversacion.Ninguno;
            EsSueldo = false;
            Monto = 0;
            SobreElegidoId = 0;
            MensajeId = 0;
        }
    }
}
