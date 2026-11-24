using FinOptima.Datos.Reportes;

namespace FinOptima.Datos.Contratos
{
    public interface IGeneradorReportes
    {
        void ExportarExcel(string ruta, DatosReporte datos);
        void ExportarPdf(string ruta, DatosReporte datos);
    }
}
