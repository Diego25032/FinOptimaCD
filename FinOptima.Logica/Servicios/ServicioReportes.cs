using System;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Reportes;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioReportes
    {
        private IGeneradorReportes generador;
        private IRepositorioSobres sobres;
        private IRepositorioMovimientos movimientos;
        private IRepositorioMetas metas;
        private IRepositorioDeudas deudas;
        private SesionActual sesion;

        public ServicioReportes(IGeneradorReportes generadorReportes, IRepositorioSobres repositorioSobres, IRepositorioMovimientos repositorioMovimientos, IRepositorioMetas repositorioMetas, IRepositorioDeudas repositorioDeudas, SesionActual sesionActual)
        {
            generador = generadorReportes;
            sobres = repositorioSobres;
            movimientos = repositorioMovimientos;
            metas = repositorioMetas;
            deudas = repositorioDeudas;
            sesion = sesionActual;
        }

        public Resultado ExportarExcel(string ruta)
        {
            try
            {
                generador.ExportarExcel(ruta, ArmarDatos());
                return Resultado.Bien("Reporte de Excel generado.");
            }
            catch (Exception error)
            {
                return Resultado.Mal(error.Message);
            }
        }

        public Resultado ExportarPdf(string ruta)
        {
            try
            {
                generador.ExportarPdf(ruta, ArmarDatos());
                return Resultado.Bien("Reporte PDF generado.");
            }
            catch (Exception error)
            {
                return Resultado.Mal(error.Message);
            }
        }

        private DatosReporte ArmarDatos()
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);

            DatosReporte datos = new DatosReporte();
            datos.NombreUsuario = sesion.Usuario.NombreCompleto;
            datos.FechaGeneracion = DateTime.Now;
            datos.Sobres = sobres.ListarPorUsuario(usuarioId);
            datos.Movimientos = movimientos.ListarPorUsuario(usuarioId);
            datos.Metas = metas.ListarPorUsuario(usuarioId);
            datos.Deudas = deudas.ListarPorUsuario(usuarioId);
            return datos;
        }
    }
}
