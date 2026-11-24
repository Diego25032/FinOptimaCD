using System;
using System.IO;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Punto de entrada de los reportes. Delega el armado en ReporteExcel y ReportePdf
    /// y traduce los errores de archivo a mensajes claros para el usuario.
    /// </summary>
    public class GeneradorReportes : IGeneradorReportes
    {
        public void ExportarExcel(string ruta, DatosReporte datos)
        {
            try
            {
                ReporteExcel reporte = new ReporteExcel(datos);
                reporte.Generar(ruta);
            }
            catch (IOException error)
            {
                throw new ErrorBaseDatos("No se pudo escribir el archivo de Excel. Verifica que no esté abierto en otro programa.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorBaseDatos("No hay permisos para guardar el archivo de Excel en esa carpeta.", error);
            }
        }

        public void ExportarPdf(string ruta, DatosReporte datos)
        {
            try
            {
                ReportePdf reporte = new ReportePdf(datos);
                reporte.Generar(ruta);
            }
            catch (IOException error)
            {
                throw new ErrorBaseDatos("No se pudo escribir el archivo PDF. Verifica que no esté abierto en otro programa.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorBaseDatos("No hay permisos para guardar el archivo PDF en esa carpeta.", error);
            }
        }
    }
}
