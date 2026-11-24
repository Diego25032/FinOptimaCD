using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloReportes : ModuloBase
    {
        public ModuloReportes(FabricaServicios servicios) : base(servicios, "Reportes")
        {
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Controls.Add(UtilidadesVista.CrearNota("Genera un resumen de tus finanzas en PDF o Excel.", Margen, 55, 880));

            Panel tarjetaExcel = UtilidadesVista.CrearTarjeta("Reporte de Excel", Margen, 100, 380, 190, TemaActual.Actual.Exito);
            Controls.Add(tarjetaExcel);

            Label detalleExcel = new Label();
            detalleExcel.Text = "Hoja de resumen con indicadores y tablas con filtros y totales de sobres, movimientos, metas y deudas.";
            detalleExcel.Location = new Point(12, 65);
            detalleExcel.AutoSize = true;
            detalleExcel.MaximumSize = new Size(350, 0);
            detalleExcel.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaExcel.Controls.Add(detalleExcel);

            Button excel = UtilidadesVista.CrearBoton("Exportar Excel", 12, 138, 180, 34, TemaActual.Actual.Exito);
            excel.Click += AlExportarExcel;
            tarjetaExcel.Controls.Add(excel);

            Panel tarjetaPdf = UtilidadesVista.CrearTarjeta("Reporte PDF", Margen + 410, 100, 380, 190, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaPdf);

            Label detallePdf = new Label();
            detallePdf.Text = "Informe con portada, indicadores, gráficos de gastos, metas, deudas y últimos movimientos.";
            detallePdf.Location = new Point(12, 65);
            detallePdf.AutoSize = true;
            detallePdf.MaximumSize = new Size(350, 0);
            detallePdf.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaPdf.Controls.Add(detallePdf);

            Button pdf = UtilidadesVista.CrearBoton("Exportar PDF", 12, 138, 180, 34, TemaActual.Actual.Peligro);
            pdf.Click += AlExportarPdf;
            tarjetaPdf.Controls.Add(pdf);

            Controls.Add(UtilidadesVista.CrearNota("Los reportes se guardan solo en el equipo donde estas usando la aplicacion.", Margen, 310, 880));
        }

        private void AlExportarExcel(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte de Excel";
            dialogo.Filter = "Archivo de Excel (*.xlsx)|*.xlsx";
            dialogo.FileName = "FinOptima-reporte-" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarExcel(dialogo.FileName);
                MostrarResultadoYAbrir(resultado, dialogo.FileName);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlExportarPdf(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte PDF";
            dialogo.Filter = "Archivo PDF (*.pdf)|*.pdf";
            dialogo.FileName = "FinOptima-reporte-" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarPdf(dialogo.FileName);
                MostrarResultadoYAbrir(resultado, dialogo.FileName);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void MostrarResultadoYAbrir(Resultado resultado, string ruta)
        {
            if (!resultado.Exito)
            {
                MostrarResultado(resultado);
                return;
            }

            if (!UtilidadesVista.Confirmar(resultado.Mensaje + "\n\n¿Deseas abrirlo ahora?"))
            {
                return;
            }

            try
            {
                System.Diagnostics.ProcessStartInfo inicio = new System.Diagnostics.ProcessStartInfo(ruta);
                inicio.UseShellExecute = true;
                System.Diagnostics.Process.Start(inicio);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
